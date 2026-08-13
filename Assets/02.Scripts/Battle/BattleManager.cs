using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

namespace AFKHero.Battle
{
    public sealed class BattleManager : MonoBehaviour
    {
        // 0초 이하의 제한시간이 설정되지 않게
        private const float MinimumBattleTimeLimit = 1f;

        [Header("전투 시간")]
        [SerializeField, Min(MinimumBattleTimeLimit)]
        private float battleTimeLimit = 90f;

        [Header("궁극기 자동/수동 모드")]
        [SerializeField] private UltimateUseMode ultimateUseMode = UltimateUseMode.Auto;

        // 현재 전투에 등록된 아군 유닛
        private readonly List<BattleUnit> allyUnits = new();

        // 현재 전투에 등록된 적군 유닛
        private readonly List<BattleUnit> enemyUnits = new();

        // 궁극기 대기 큐
        private readonly UltimateQueue ultimateQueue = new();

        // 같은 프레임에 사망해도 승패 중복 방지
        private bool isBattleResultConfirmed;

        // 남은 시간
        private float remainingBattleTime;

        // 궁극기가 동시에 실행 방지용
        private BattleUnit currentUltimateUnit;

        // 다른 궁극기가 실행 중일 때 종료 직후 선택한 궁극기 먼저 저장
        private BattleUnit manualSelectUltimateUnit;

        // 현재 전투의 진행 상태
        public BattleState CurrentState { get; private set; } = BattleState.None;

        // 아군 유닛 목록
        public IReadOnlyList<BattleUnit> AllyUnits => allyUnits;

        // 적군 유닛 목록
        public IReadOnlyList<BattleUnit> EnemyUnits => enemyUnits;

        // 궁극기 큐
        public UltimateQueue UltimateQueue => ultimateQueue;

        // 궁극기 실행 상태
        public BattleUnit CurrentUltimateUnit => currentUltimateUnit;
        public bool IsUltimatePlaying => currentUltimateUnit != null;

        // 궁극기 실행 상태일때 데미지 반영 멈춤
        public bool IsDamageApplicationPaused => IsUltimatePlaying;

        // 궁극기 제어 상태
        public UltimateUseMode UltimateMode => ultimateUseMode;
        public BattleUnit ManualSelectUltimateUnit => manualSelectUltimateUnit;

        // 최소한 보정
        public float BattleTimeLimit => Mathf.Max(MinimumBattleTimeLimit, battleTimeLimit);

        public float RemainingBattleTime => remainingBattleTime;

        // UI에서 활용(0 또는 1로 진행률로 사용)
        public float RemainingBattleTimeNormalized => BattleTimeLimit > 0f ? remainingBattleTime / BattleTimeLimit : 0f;

        // 전투 상태 변경
        public event Action<BattleState> StateChanged;

        // 죽음 상태 변경
        public event Action<BattleUnit> UnitDied;

        // 첫 번째 값은 남은 시간, 두 번째 값은 전체 제한시간
        public event Action<float, float> BattleTimeChanged;

        // UI, 카메라, 연출용
        public event Action<BattleUnit> UltimateStarted;
        public event Action<BattleUnit> UltimateFinished;

        // 궁극기 모드, 수동 선택
        public event Action<UltimateUseMode> UltimateUseModeChanged;
        public event Action<BattleUnit> UltimateManualSelected;

        private void Awake()
        {
            ResetBattleTimer();
            ChangeState(BattleState.Preparing);
        }

        private void LateUpdate()
        {
            if(CurrentState == BattleState.Fighting && currentUltimateUnit == null)
            {
                TryStartNextUltimate();
            }
            // 제한시간이 끝나는 마지막 프레임에 공격 결과 판정 반영
            UpdateBattleTimer();
        }

        // 해당 진영 목록에 유닛 등록
        public void RegisterUnit(BattleUnit unit)
        {
            if(unit == null || !unit.IsInitialized)
            {
                Debug.LogError("초기화되지 않은 BattleUnit은 등록할 수 없습니다.",this);
                return;
            }

            // 유닛 진영에 따라 등록할 내부 목록 결정
            List<BattleUnit> targetList = unit.Team == TeamType.Ally ? allyUnits : enemyUnits;

            // 같은 유닛이 여러 번 등록되지 않도록 방지
            if (!targetList.Contains(unit))
            {
                targetList.Add(unit);
                unit.Energy.UltimateReady += HandleUltimateReady;
            }
        }

        // 반대 진영 목록 반환 (가장 가까운 적 탐색용)
        public IReadOnlyList<BattleUnit> GetOpponents(TeamType requesterTeam)
        {
            return requesterTeam == TeamType.Ally ? enemyUnits : allyUnits;
        }

        // 양 진영에 한 명 이상 등록되어 있다면 전투 시작
        public void StartBattle()
        {
            // 전투 중복 실행 방지
            if (CurrentState == BattleState.Fighting || CurrentState == BattleState.UltimateSequence)
            {
                Debug.LogWarning("이미 전투가 진행 중입니다.");
                return;
            }

            if(!HasLivingUnit(TeamType.Ally)||!HasLivingUnit(TeamType.Enemy))
            {
                Debug.LogError("양 진영에 유닛이 한 명 이상 있어야 전투를 시작할 수 있습니다.", this);
                return;
            }

            isBattleResultConfirmed = false;
            currentUltimateUnit = null;

            manualSelectUltimateUnit = null;

            ultimateQueue.Clear();

            ClearAllUnitStatusEffects();

            ResetAllUnitEnergy();

            ResetBattleTimer();
            ChangeState(BattleState.Fighting);

            Debug.Log($"전투 시작!\n" +
                $"아군 [{allyUnits.Count}]명 / 적군 [{enemyUnits.Count}]명 ");
        }

        public void NotifyUnitDied(BattleUnit deadunit)
        {
            if(deadunit == null)
            {
                return;
            }

            bool isRegistered = allyUnits.Contains(deadunit) || enemyUnits.Contains(deadunit);

            if (!isRegistered)
            {
                Debug.LogWarning($"등록되지 않은 유닛 {deadunit.name}이 죽었습니다.",this);

                return;
            }

            ultimateQueue.Remove(deadunit);

            if(manualSelectUltimateUnit == deadunit)
            {
                manualSelectUltimateUnit = null;
            }

            if(deadunit.Energy != null)
            {
                deadunit.Energy.UltimateReady -= HandleUltimateReady;
            }

            // 궁극기 실행 중 사망 처리
            bool wasCurrentUltimateUnit = deadunit == currentUltimateUnit;

            if (wasCurrentUltimateUnit)
            {
                deadunit.UltimateController?.CancelUltimate();
                currentUltimateUnit = null;
            }

            UnitDied?.Invoke(deadunit);

            // 적이 죽으면 다음 적을 찾도록 알림
            NotifyTargetInvalidated(deadunit);

            if(IsBattleRunning() && !isBattleResultConfirmed)
            {
                CheckBattleResult();
            }

            if(isBattleResultConfirmed)
            {
                return;
            }

            if (wasCurrentUltimateUnit)
            {
                TryStartNextUltimate();
            }
        }

        // 새로운 전투 시작 시 유닛 재정비
        public void ClearRegisteredUnits()
        {
            CancelCurrentUltimate();
            ClearAllUnitStatusEffects();
            UnsubscribeUltimateReadyInList(allyUnits);
            UnsubscribeUltimateReadyInList(enemyUnits);

            ultimateQueue.Clear();

            manualSelectUltimateUnit = null;

            allyUnits.Clear();
            enemyUnits.Clear();

            // 재시작할 때 이전 전투의 승패 판정이 남지않게 초기화
            isBattleResultConfirmed = false;

            ResetBattleTimer();

            ChangeState(BattleState.Preparing);
        }

        // 죽은 유닛을 바라보던 생존 유닛 갱신
        private void NotifyTargetInvalidated(BattleUnit deadUnit)
        {
            NotifyTargetInvalidatedInList(allyUnits, deadUnit);
            NotifyTargetInvalidatedInList(enemyUnits, deadUnit);
        }

        private static void NotifyTargetInvalidatedInList(IReadOnlyList<BattleUnit> units, BattleUnit deadUnit)
        {
            for(int i = 0;i< units.Count; i++)
            {
                BattleUnit unit = units[i];

                if (unit == null ||
                    !unit.IsInitialized ||
                    unit.Stats == null ||
                    !unit.Stats.IsAlive)
                {
                    continue;
                }

                unit.TargetFinder?.HandleTargetInvalidated(deadUnit);
            }
        }

        private bool HasLivingUnit(TeamType team)
        {
            IReadOnlyList<BattleUnit> units = team == TeamType.Ally ? allyUnits : enemyUnits;

            for(int i = 0; i< units.Count; i++)
            {
                BattleUnit unit = units[i];

                if (unit != null &&
                    unit.IsInitialized &&
                    unit.Stats != null &&
                    unit.Stats.IsAlive)
                {
                    return true;
                } 
            }

            return false;
        }

        private bool IsBattleRunning()
        {
            return CurrentState == BattleState.Fighting ||
                CurrentState == BattleState.UltimateSequence;
        }

        private void CheckBattleResult()
        {
            if(isBattleResultConfirmed || !IsBattleRunning() || currentUltimateUnit != null)
            {
                return;
            }

            bool hasLivingAlly = HasLivingUnit(TeamType.Ally);

            bool hasLivingEnemy = HasLivingUnit(TeamType.Enemy);

            // 양쪽 모두 생존자가 있으면 전투 지속
            if(hasLivingAlly && hasLivingEnemy)
            {
                return;
            }

            // 양쪽이 동시에 전멸하면 아군 패배
            BattleState resultState = hasLivingAlly ? BattleState.Victory : BattleState.Defeat;

            ConfirmBattleResult(resultState, resultState == BattleState.Victory ? "전투 승리!" : "전투 패배...");
        }
        
        private void UpdateBattleTimer()
        {
            if(CurrentState != BattleState.Fighting || isBattleResultConfirmed)
            {
                return;
            }

            float previousTime = remainingBattleTime;

            remainingBattleTime = Mathf.Max(0f, remainingBattleTime - Time.deltaTime);

            if(!Mathf.Approximately(previousTime, remainingBattleTime))
            {
                NotifyBattleTimeChanged();
            }

            if(remainingBattleTime > 0f)
            {
                return;
            }

            ConfirmBattleResult(BattleState.Defeat, "전투 제한시간 종료 - 패배...");
        }

        private void ResetBattleTimer()
        {
            remainingBattleTime = BattleTimeLimit;
            NotifyBattleTimeChanged();
        }

        private void NotifyBattleTimeChanged()
        {
            BattleTimeChanged?.Invoke(remainingBattleTime, BattleTimeLimit);
        }

        private void ConfirmBattleResult(BattleState resultState, string resultLog)
        {
            if(isBattleResultConfirmed || !IsBattleRunning())
            {
                return;
            }

            if(resultState != BattleState.Victory && resultState != BattleState.Defeat)
            {
                Debug.LogError($"{resultState}는 전투 결과 상태가 아닙니다.", this);

                return;
            }

            isBattleResultConfirmed = true;

            CancelCurrentUltimate();
            manualSelectUltimateUnit = null;
            ultimateQueue.Clear();

            ClearAllUnitStatusEffects();

            ChangeState(resultState);
            Debug.Log(resultLog,this);
        }

        // 유닛 대기열 등록
        private void HandleUltimateReady(BattleUnit readyUnit)
        {
            if(CurrentState != BattleState.Fighting || !IsRegisterUnit(readyUnit))
            {
                return;
            }

            if (!ultimateQueue.TryEnqueue(readyUnit, Time.frameCount))
            {
                return;
            }

            Debug.Log($"궁극기 큐 등록 - {readyUnit.name} | 현재 대기 {ultimateQueue.Count}명", readyUnit);

            LogUltimateQueueOrder();
        }

        public void SetUltimateUseMode(UltimateUseMode nextMode)
        {
            if(ultimateUseMode == nextMode)
            {
                return;
            }

            ultimateUseMode = nextMode;
            UltimateUseModeChanged?.Invoke(ultimateUseMode);
            Debug.Log($"[궁극기 자동/수동 변경] {ultimateUseMode}", this);
        }

        public void ToggleUltimateUseMode()
        {
            SetUltimateUseMode(ultimateUseMode == UltimateUseMode.Auto ? UltimateUseMode.Manual : UltimateUseMode.Auto);
        }

        // 토글 값 자동/수동 조정
        public void SetAutomaticUltimateUse(bool useAuto)
        {
            SetUltimateUseMode(useAuto ? UltimateUseMode.Auto : UltimateUseMode.Manual);
        }

        // 대기중인 궁극기를 다음 실행 대상으로 지정
        public bool TrySelectQueueUltimate(BattleUnit selectUnit)
        {
            if (!IsBattleRunning() ||
                selectUnit == null||
                selectUnit.Team != TeamType.Ally ||
                !ultimateQueue.Contains(selectUnit)||
                !CanExecuteUltimate(selectUnit))
            {
                return false;
            }

            manualSelectUltimateUnit = selectUnit;
            UltimateManualSelected?.Invoke(selectUnit);

            Debug.Log($"[궁극기 수동 선택] {selectUnit.name}", selectUnit);

            if(currentUltimateUnit == null)
            {
                TryStartNextUltimate();
            }

            return true;
        }

        // 궁극기 순차 실행
        private void TryStartNextUltimate()
        {
            if(isBattleResultConfirmed||
                currentUltimateUnit != null ||
                !IsBattleRunning())
            {
                return;
            }

            if(!TryFindNextUltimate(out BattleUnit nextUnit))
            {
                if(CurrentState == BattleState.UltimateSequence)
                {
                    ChangeState(BattleState.Fighting);
                }

                return;
            }

            currentUltimateUnit = nextUnit;

            if(CurrentState != BattleState.UltimateSequence)
            {
                ChangeState(BattleState.UltimateSequence);
            }

            if (!nextUnit.UltimateController.TryExecute(HandleUltimateCompleted))
            {
                currentUltimateUnit = null;

                if(CurrentState == BattleState.UltimateSequence)
                {
                    ChangeState(BattleState.Fighting);
                }
                Debug.LogWarning($"[궁극기 실행 실패] {nextUnit.name}의 에너지와 대기열을 유지",nextUnit);
                return;
            }

            if (!nextUnit.Energy.TryConsumeUltimateEnergy())
            {
                nextUnit.UltimateController.CancelUltimate();
                currentUltimateUnit = null;

                if(CurrentState == BattleState.UltimateSequence)
                {
                    ChangeState(BattleState.Fighting);
                }
                Debug.LogWarning($"[궁극기 에너지 소비 실패] {nextUnit.name}의 궁극기 실행을 취소",nextUnit);
                return;
            }

            ultimateQueue.Remove(nextUnit);
            if(manualSelectUltimateUnit == nextUnit)
            {
                manualSelectUltimateUnit = null;
            }

            UltimateStarted?.Invoke(nextUnit);
            Debug.Log($"[궁극기 시작] {nextUnit.name}", nextUnit);
        }

        // 수동 선택 모드 처리
        private bool TryFindNextUltimate(out BattleUnit nextUnit)
        {
            // 사용자가 직접 선택한 궁극기가 있으면 가장 먼저 검사
            if(manualSelectUltimateUnit != null)
            {
                if (!ultimateQueue.Contains(manualSelectUltimateUnit))
                {
                    manualSelectUltimateUnit = null;
                }
                else if (CanExecuteUltimate(manualSelectUltimateUnit))
                {
                    nextUnit = manualSelectUltimateUnit;
                    return true;
                }
            }

            if (ultimateUseMode == UltimateUseMode.Auto) 
            {
                return ultimateQueue.TryGetFirst(CanExecuteUltimate, out nextUnit);
            }

            return ultimateQueue.TryGetFirst(IsExecutableEnemyUltimate, out nextUnit);
        }

        public bool IsExecutableEnemyUltimate(BattleUnit unit)
        {
            return unit != null && unit.Team == TeamType.Enemy && CanExecuteUltimate(unit);
        }

        private bool CanExecuteUltimate(BattleUnit unit)
        {
            return IsRegisterUnit(unit) &&
                unit.Stats != null &&
                unit.Stats.IsAlive &&
                unit.Energy != null &&
                unit.Energy.IsUltimateReady &&
                unit.UltimateController != null &&
                !unit.UltimateController.IsExecuting &&
                unit.StatusEffects != null &&
                unit.StatusEffects.CanUseUltimate;
        }

        private void HandleUltimateCompleted(BattleUnit completeUnit)
        {
            if(completeUnit == null || completeUnit != currentUltimateUnit)
            {
                return;
            }

            currentUltimateUnit = null;
            UltimateFinished?.Invoke(completeUnit);
            Debug.Log($"[궁극기 종료] {completeUnit}", completeUnit);

            if (isBattleResultConfirmed)
            {
                return;
            }

            CheckBattleResult();

            if (!isBattleResultConfirmed)
            {
                TryStartNextUltimate();
            }
        }

        private void CancelCurrentUltimate()
        {
            if(currentUltimateUnit == null)
            {
                return;
            }

            currentUltimateUnit.UltimateController?.CancelUltimate();
            currentUltimateUnit = null;
        }

        // 전투에 등록 되어있는지 검사
        private bool IsRegisterUnit(BattleUnit unit)
        {
            return unit != null && (allyUnits.Contains(unit) || enemyUnits.Contains(unit));
        }

        // 로그 확인용
        private void LogUltimateQueueOrder()
        {
            IReadOnlyList<BattleUnit> waitingUnits = ultimateQueue.WaitingUnits;
            string queueOrder = string.Empty;

            for(int i = 0; i < waitingUnits.Count; i++)
            {
                if (i > 0)
                {
                    queueOrder += " -> ";
                }
                queueOrder += waitingUnits[i].name;
            }
            Debug.Log($"[궁극기 대기 순서] {queueOrder}",this);
        }

        // 새 전투용 에너지 초기화
        private void ResetAllUnitEnergy()
        {
            ResetEnergyInList(allyUnits);
            ResetEnergyInList(enemyUnits);
        }

        private void ClearAllUnitStatusEffects()
        {
            ClearStatusEffectsInList(allyUnits);
            ClearStatusEffectsInList(enemyUnits);
        }

        private static void ClearStatusEffectsInList(IReadOnlyList<BattleUnit> units)
        {
            for(int i = 0; i < units.Count;i++)
            {
                BattleUnit unit = units[i];

                if(unit != null && unit.StatusEffects != null)
                {
                    unit.StatusEffects.ClearAllStatusEffects();
                }
            }
        }

        private static void ResetEnergyInList(IReadOnlyList<BattleUnit> units)
        {
            for(int i = 0; i < units.Count; i++)
            {
                BattleUnit unit = units[i];

                if(unit != null && unit.Energy != null)
                {
                    unit.Energy.ResetEnergy();
                } 
            }
        }

        // 구독 해제
        private void UnsubscribeUltimateReadyInList(IReadOnlyList<BattleUnit> units)
        {
            for(int i = 0; i< units.Count; i++)
            {
                BattleUnit unit = units[i];

                if (unit == null || unit.Energy == null)
                {
                    continue;
                }

                unit.Energy.UltimateReady -= HandleUltimateReady;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeUltimateReadyInList(AllyUnits);
            UnsubscribeUltimateReadyInList(EnemyUnits);
        }

        // 현재 전투 상태를 새로운 상태로 변경하고 이벤트 발생
        private void ChangeState(BattleState nextState)
        {
            if(CurrentState == nextState)
            {
                return;
            }

            CurrentState = nextState;

            // 상태 변경을 구독하고 있는 UI나 전투 시스템에 알림
            StateChanged?.Invoke(CurrentState);
        }
    }
}
