using UnityEngine;

namespace AFKHero.Battle
{
    public class UnitMovement : MonoBehaviour
    {
        private float AttackRangeEpsilon = 0.1f;

        private BattleUnit owner;

        private BattleManager battleManager;

        private UnitTargetFinder targetFinder;

        // 공격할 대상을 직접 받아서 검사
        public bool IsTargetInAttackRange(BattleUnit target)
        {
            if (!TryGetTargetDistanceData(target, out Vector2 difference, out float effectiveAttackRange))
            {
                return false;
            }

            return IsAttackRange(difference, effectiveAttackRange);
        }

        private bool TryGetTargetDistanceData(
            BattleUnit target,
            out Vector2 difference,
            out float effectiveAttackRange)
        {
            difference = Vector2.zero;
            effectiveAttackRange = 0f;

            if (owner == null ||
                owner.Stats == null ||
                target == null ||
                target.Stats == null ||
                !target.Stats.IsAlive)
            {
                return false;
            }

            Vector2 currentPosition = transform.position;
            Vector2 targetPosition = target.transform.position;

            difference = targetPosition - currentPosition;

            // 기본 공격 사거리에 작은 여유 범위를 더해 경계값에서 공격이 막히는 현상 방지
            effectiveAttackRange = Mathf.Max(0f, owner.Stats.AttackRange) + AttackRangeEpsilon;

            return true;
        }

        private static bool IsAttackRange(Vector2 difference, float effectiveAttackRange)
        {
            float effectiveAttackRangeSqr = effectiveAttackRange * effectiveAttackRange;
            return difference.sqrMagnitude <= effectiveAttackRangeSqr;
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
            return !IsTargetInAttackRange(targetFinder.CurrentTarget);
        }

        // 타겟을 향해 이동 및 공격 사거리에 들어오면 정지
        private void MoveTowardTarget()
        {
            BattleUnit target = targetFinder.CurrentTarget;

            if (!TryGetTargetDistanceData(target, out Vector2 direction, out float effectiveAttackRange))
            {
                return;
            }

            if (IsAttackRange(direction, effectiveAttackRange))
            {
                return;
            }

            float distance = direction.magnitude;

            if (distance <= Mathf.Epsilon)
            {
                return;
            }

            float maximumMoveDistance = owner.Stats.MoveSpeed * Time.deltaTime;
            float distanceUntilAttackRange = Mathf.Max(0f, distance - effectiveAttackRange);
            float moveDistance = Mathf.Min(maximumMoveDistance, distanceUntilAttackRange);

            Vector2 normalizedDirection = direction / distance;
            Vector2 currentPosition = transform.position;
            Vector2 nextPosition = currentPosition + normalizedDirection * moveDistance;

            transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
        }
    }
}