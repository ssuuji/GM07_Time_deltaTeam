using UnityEngine;

namespace AFKHero.Battle
{
    public class UnitMovement : MonoBehaviour
    {
        private const float AttackRangeEpsilon = 0.0001f;

        private BattleUnit owner;

        private BattleManager battleManager;

        private UnitTargetFinder targetFinder;

        public bool IsTargetInAttackRange
        {
            get
            {
                if (!TryGetTargetDistanceData(out Vector2 difference, out float attackRange))
                {
                    return false;
                }

                return IsAttackRange(difference, attackRange);
            }
        }

        private bool TryGetTargetDistanceData(out Vector2 difference, out float attackRange)
        {
            difference = Vector2.zero;
            attackRange = 0f;

            if (owner == null ||
                owner.Stats == null ||
                targetFinder == null ||
                !targetFinder.HasValidTarget)
            {
                return false;
            }

            BattleUnit target = targetFinder.CurrentTarget;

            if (target == null)
            {
                return false;
            }

            Vector2 currentPosition = transform.position;
            Vector2 targetPosition = target.transform.position;

            difference = targetPosition - currentPosition;
            attackRange = Mathf.Max(0f, owner.Stats.AttackRange);

            return true;
        }

        private static bool IsAttackRange(Vector2 difference, float attackRange)
        {
            float attackRangeSqr = attackRange * attackRange;

            return difference.sqrMagnitude <= attackRangeSqr + AttackRangeEpsilon;
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

            if(owner.StatusEffects != null && !owner.StatusEffects.CanMove)
            {
                return false;
            }

            // 이미 사거리 안 이라면 공격하는 동안 이동 X
            return !IsTargetInAttackRange;
        }

        // 타겟을 향해 이동 및 공격 사거리에 들어오면 정지
        private void MoveTowardTarget()
        {
            if (!TryGetTargetDistanceData(out Vector2 direction, out float attackRange))
            {
                return;
            }

            if (IsAttackRange(direction, attackRange))
            {
                return;
            }

            float distance = direction.magnitude;

            if (distance <= Mathf.Epsilon)
            {
                return;
            }

            BattleUnit target = targetFinder.CurrentTarget;

            float maximumMoveDistance = owner.Stats.MoveSpeed * Time.deltaTime;
            float distanceUntilAttackRange = Mathf.Max(0f, distance - attackRange);
            float moveDistance = Mathf.Min(maximumMoveDistance, distanceUntilAttackRange);

            Vector2 normalizedDirection = direction / distance;
            Vector2 currentPosition = transform.position;
            Vector2 nextPosition = currentPosition + normalizedDirection * moveDistance;

            transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
        }
    }
}