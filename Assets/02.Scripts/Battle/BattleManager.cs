using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace AFKHero.Battle
{
    public sealed class BattleManager : MonoBehaviour
    {
        // 현재 전투에 등록된 아군 유닛
        private readonly List<BattleUnit> allyUnits = new();

        // 현재 전투에 등록된 적군 유닛
        private readonly List<BattleUnit> enemyUnits = new();

        // 현재 전투의 진행 상태
        public BattleState CurrentState { get; private set; } = BattleState.None;

        // 아군 유닛 목록
        public IReadOnlyList<BattleUnit> AllyUnits => allyUnits;
        // 적군 유닛 목록
        public IReadOnlyList<BattleUnit> EnemyUnits => enemyUnits;

        // 같은 프레임에 사망해도 승패 중복 방지
        private bool isBattleResultConfirmed;

        // 전투 상태 변경
        public event Action<BattleState> StateChanged;

        // 죽음 상태 변경
        public event Action<BattleUnit> UnitDied;

        private void Awake()
        {
            ChangeState(BattleState.Preparing);
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
            if (CurrentState == BattleState.Fighting)
            {
                Debug.LogWarning("이미 전투가 진행 중입니다.");
            }

            if(!HasLivingUnit(TeamType.Ally)||!HasLivingUnit(TeamType.Enemy))
            {
                Debug.LogError("양 진영에 유닛이 한 명 이상 있어야 전투를 시작할 수 있습니다.", this);
                return;
            }

            isBattleResultConfirmed = false;

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

            UnitDied?.Invoke(deadunit);

            // 적이 죽으면 다음 적을 찾도록 알림
            NotifyTargetInvalidated(deadunit);

            if(CurrentState != BattleState.Fighting)
            {
                return;
            }

            CheckBattleResult();
        }

        // 새로운 전투 시작 시 유닛 재정비
        public void ClearRegisteredUnits()
        {
            allyUnits.Clear();
            enemyUnits.Clear();

            // 재시작할 때 이전 전투의 승패 판정이 남지않게 초기화
            isBattleResultConfirmed = false;

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

        private void CheckBattleResult()
        {
            if(isBattleResultConfirmed || CurrentState != BattleState.Fighting)
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

            isBattleResultConfirmed = true;

            // 양쪽이 동시에 전멸하면 적이 다 죽었으므로 승리
            BattleState resultState = hasLivingEnemy ? BattleState.Defeat : BattleState.Victory;

            ChangeState(resultState);

            Debug.Log(resultState == BattleState.Victory ? "전투 승리!" : "전투 패배...", this);
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
