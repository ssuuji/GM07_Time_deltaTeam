using UnityEngine;

namespace AFKHero.Battle
{
    public class UnitMovement : MonoBehaviour
    {
        private BattleUnit owner;

        private BattleManager battleManager;

        private UnitTargetFinder targetFinder;

        public bool IsTargetInAttackRange
        {
            get
            {
                if (owner == null ||
                    owner.Stats == null ||
                    targetFinder == null ||
                    !targetFinder.HasValidTarget) 
                {
                    return false;
                }

                Vector2 currentPosition = transform.position;
                Vector2 targetPosition = targetFinder.CurrentTarget.transform.position;

                float sqrDistance = (targetPosition - currentPosition).sqrMagnitude;

                float attackRange = owner.Stats.AttackRange;

                return sqrDistance <= attackRange * attackRange;
            }
        }

        public void Initialize(BattleUnit unitOwner, BattleManager manager, UnitTargetFinder finder)
        {
            owner= unitOwner;
            battleManager= manager;
            targetFinder = finder;
        }

        private void Update()
        {
            if (!CanMove())
            {
                return;
            }

            MoveTowardTarget();
        }

        // 현재 유닛이 움직일 수 있는 상태인지
        private bool CanMove()
        {
            if (owner == null ||
                owner.Stats == null ||
                battleManager == null ||
                targetFinder == null)
            {
                return false;
            }

            if (battleManager.CurrentState != BattleState.Fighting ||
                !owner.Stats.IsAlive ||
                !targetFinder.HasValidTarget)
            {
                return false;
            }

            // 이미 사거리 안 이라면 공격하는 동안 이동 X
            return !IsTargetInAttackRange;
        }

        // 타겟을 향해 이동 및 공격 사거리에 들어오면 정지
        private void MoveTowardTarget()
        {
            BattleUnit target = targetFinder.CurrentTarget;

            Vector2 currentPosition = transform.position;
            Vector2 targetPosition = target.transform.position;
            Vector2 direction = targetPosition - currentPosition;

            float distance = direction.magnitude;
            float attackRange = owner.Stats.AttackRange;

            // 타겟이 사거리 안에 있다면 이동 X
            if (distance <= attackRange ||
                distance <= Mathf.Epsilon) 
            {
                return;
            }

            // 이번 프레임 이동 최대 거리
            float maximumMoveDistance = owner.Stats.MoveSpeed * Time.deltaTime;

            // 타겟 중심까지 이동하지 않고, 공격 사거리 까지만 이동
            float distanceUnitlAttackRange = distance - attackRange;

            float moveDistance = Mathf.Min(maximumMoveDistance, distanceUnitlAttackRange);

            Vector2 normalizedDirection = direction / distance;

            Vector2 nextPosition = currentPosition + normalizedDirection * moveDistance;

            transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);

        }


    }
}