using System.Collections.Generic;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitTargetFinder : MonoBehaviour
    {
        [Header("타겟 탐색")]

        [SerializeField, Min(0.05f)]
        // 탐색 인터벌 시간
        private float searchInterval = 0.2f;

        private BattleUnit owner;

        private BattleManager battleManager;

        // 다음 탐색 시간
        private float nextSrearchTime;

        // 현재 타겟
        public BattleUnit CurrentTarget { get; private set; }

        public bool HasValidTarget => IsValidTarget(CurrentTarget);

        public void Initialize(BattleUnit unitOwner,BattleManager manger)
        {
            owner = unitOwner;
            battleManager = manger;
            CurrentTarget = null;
            nextSrearchTime = 0;
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


        // 가장 가까운 유닛 탐색
        public void FindClosestTarget()
        {
            if(owner == null || battleManager == null)
            {
                CurrentTarget = null;
                return;
            }

            IReadOnlyList<BattleUnit> opponents = battleManager.GetOpponents(owner.Team);

            BattleUnit closestTarget = null;
            float closestSqrDistance = float.MaxValue;

            for(int i = 0; i < opponents.Count; i++)
            {
                BattleUnit opponent = opponents[i];

                if(!IsValidTarget(opponent))
                {
                    continue;
                }

                Vector3 difference = opponent.transform.position - owner.transform.position;

                float sqrDistance = difference.sqrMagnitude;

                if(sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closestTarget = opponent;
                }
            }

            CurrentTarget = closestTarget;
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
            FindClosestTarget();

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