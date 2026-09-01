using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitTargetFinder : MonoBehaviour
    {
        // 전열
        private const int FrontRowSlotCount = 2;

        // 부동소수점 거리 오차로 같은 거리의 타겟이 흔들리는 것을 방지
        private const float DistanceComparisonEpsion = 0.0001f;

        [Header("타겟 탐색")]
        [SerializeField, Min(0.05f)]
        // 탐색 인터벌 시간
        private float searchInterval = 0.2f;

        private BattleUnit owner;

        private BattleManager battleManager;

        private TargetPriority targetRule = TargetPriority.NearestEnemy;

        // 다음 탐색 시간
        private float nextSrearchTime;

        // 현재 타겟
        public BattleUnit CurrentTarget { get; private set; }

        public bool HasValidTarget => IsValidTarget(CurrentTarget);

        public void Initialize(BattleUnit unitOwner,BattleManager manger)
        {
            owner = unitOwner;
            battleManager = manger;
            targetRule = GetBattleTargetRule(unitOwner);

            CurrentTarget = null;
            nextSrearchTime = 0;
        }

        private static TargetPriority GetBattleTargetRule(BattleUnit unitOwner)
        {
            if (unitOwner == null ||
                unitOwner.Data == null ||
                unitOwner.Data.TargetRule == TargetPriority.LowestHpAlly)
            {
                return TargetPriority.NearestEnemy;
            }

            return unitOwner.Data.TargetRule;
        }

        private void Update()
        {
            if (!CanSearchTarget())
            {
                return;
            }

            // 유효 공격 타겟인지 검사
            if (HasValidTarget)
            {
                return;
            }

            // 죽거나 사라진 타겟 참조 제거
            CurrentTarget = null;

            // 탐색 인터벌이 돌아오지 않았으면 기다림
            if (Time.time < nextSrearchTime)
            {
                {
                    return;
                }
            }

            nextSrearchTime = Time.time + searchInterval;

            FindClosestTarget();
        }

        // 현재 타겟이 사망했다는 알림을 받으면 같은 프레임에 다음 적을 찾음
        public void HandleTargetInvalidated(BattleUnit invalidTarget)
        {
            if(CurrentTarget != invalidTarget)
            {
                return;
            }

            ClearTarget();

            if (!CanSearchTarget())
            {
                return;
            }

            SearchImmediately();
        }

        // 우선순위 적 찾기
        // 1. 전열 (0~1번 슬롯) 2. 같은 열에서는 가까운 거리 3. 거리도 낮으면 더 낮은 슬롯 번호
        public void FindPriorityTarget()
        {
            if(owner == null || battleManager == null)
            {
                CurrentTarget = null;
                return;
            }

            if (TryFindTauntTarget(out BattleUnit tauntTarget))
            {
                CurrentTarget = tauntTarget;
                return;
            }

            IReadOnlyList<BattleUnit> opponents = battleManager.GetOpponents(owner.Team);

            BattleUnit bestTarget = null;
            int bestRowPriority = int.MaxValue;
            float bestSqrDistance = float.MaxValue;

            for(int i = 0; i < opponents.Count; i++)
            {
                BattleUnit oppoenet = opponents[i];

                if (!IsValidTarget(oppoenet))
                {
                    continue;
                }

                Vector3 difference = oppoenet.transform.position - owner.transform.position;

                float sqrDistance = difference.sqrMagnitude;

                int rowPriority = GetRowPriority(oppoenet, targetRule);


                if (!IsHigherPriorityCandidate(
                    oppoenet,
                    rowPriority,
                    sqrDistance,
                    bestTarget,
                    bestRowPriority,
                    bestSqrDistance))
                {
                    continue;
                }

                bestTarget = oppoenet;
                bestRowPriority = rowPriority;
                bestSqrDistance = sqrDistance;
            }

            CurrentTarget = bestTarget;
        }

        // 직업별 우선순위 계산
        private static int GetRowPriority(BattleUnit unit, TargetPriority targetRule)
        {
            bool isFrontRow = unit.FormationSlotIndex >= 0 && unit.FormationSlotIndex < FrontRowSlotCount;

            // 궁수처럼 BacklineEnemy를 사용하는 유닛은 후열을 먼저 공격합니다.
            if (targetRule == TargetPriority.BacklineEnemy)
            {
                return isFrontRow ? 1 : 0;
            }

            // 기존 전투 규칙을 보존하여 그 외 직업은 전열을 먼저 공격합니다.
            return isFrontRow ? 0 : 1;
        }

        // 가장 가까운 유닛 탐색
        public void FindClosestTarget()
        {
            FindPriorityTarget();
        }

        private static bool IsHigherPriorityCandidate(
            BattleUnit candidate,
            int candidateRowPriority,
            float candidateSqrDistnace,
            BattleUnit currentBest,
            int currentBestRowPriority,
            float currentBestSqrDistance)
        {
            if(currentBest == null)
            {
                return true;
            }

            // 전열(0)이 후열(1)보다 항상 우선함
            if (candidateRowPriority != currentBestRowPriority)
            {
                return candidateRowPriority < currentBestRowPriority;
            }

            float distanceDiffernce = candidateSqrDistnace - currentBestSqrDistance;

            // 같은 열에서는 더 가까운 적을 우선
            if(distanceDiffernce < - DistanceComparisonEpsion)
            {
                return true;
            }

            if(Mathf.Abs(distanceDiffernce) > DistanceComparisonEpsion)
            {
                return false;
            }

            // 거리까지 같으면 슬롯 번호로 결과를 고정
            return candidate.FormationSlotIndex < currentBest.FormationSlotIndex; 
        }

        // 전열은 0, 후열은 1을 반환하여 숫자가 낮을 수록 우선하게 함
        private static int GetRowPriority(BattleUnit unit)
        {
            return unit.FormationSlotIndex >= 0 && unit.FormationSlotIndex < FrontRowSlotCount ? 0 : 1;
        }

        private bool TryFindTauntTarget(out BattleUnit tauntTarget)
        {
            tauntTarget = null;

            if (owner.StatusEffects == null ||
                !owner.StatusEffects.TryGetTauntSource(out BattleUnit source) ||
                !IsValidTarget(source))
            {
                return false;
            }

            tauntTarget = source;
            return true;
        }

        // 타겟 정리
        public void ClearTarget()
        {
            CurrentTarget = null;
            nextSrearchTime = 0f;
        }
        
        // 탐색 가능한 전투 상태 인지 검사
        private bool CanSearchTarget()
        {
            return owner != null &&
                owner.Stats != null &&
                owner.Stats.IsAlive &&
                battleManager != null &&
                battleManager.CurrentState == BattleState.Fighting;
        }
        
        private void SearchImmediately()
        {
            FindPriorityTarget();

            nextSrearchTime = HasValidTarget ? 0f : Time.time + searchInterval;
        }

        // 공격 가능한 유효 타깃인지 검사
        private bool IsValidTarget(BattleUnit target)
        {
            if (owner == null ||
                target == null ||
                target == owner) 
            {
                return false;
            }

            if(!target.IsInitialized ||
                target.Stats == null||
                !target.Stats.IsAlive)
            {
                return false;
            }

            return target.Team != owner.Team;
        }
    }
}