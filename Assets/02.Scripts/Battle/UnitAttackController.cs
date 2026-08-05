using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitAttackController : MonoBehaviour
    {
        private const float MinimumAttackInterval = 0.05f;

        private BattleUnit owner;
        private BattleManager battleManager;
        private UnitTargetFinder targetFinder;
        private UnitMovement unitMovement;

        private float nextAttackTime;

        public void Initialize(
            BattleUnit unitOwner,
            BattleManager manager,
            UnitTargetFinder finder,
            UnitMovement movement)
        {
            owner = unitOwner;
            battleManager = manager;
            targetFinder = finder;
            unitMovement = movement;

            nextAttackTime = 0f;
        }

        private void Update()
        {
            if (!CanAttack())
            {
                return;
            }

            if(Time.time < nextAttackTime)
            {
                return;
            }

            PerformBasicAttack();
        }

        private bool CanAttack()
        {
            // 컴포넌트 검사
            if(owner == null ||
                owner.Stats == null ||
                battleManager == null ||
                targetFinder == null ||
                unitMovement == null)
            {
                return false;
            }

            // 전투 상태 검사
            if(battleManager.CurrentState != BattleState.Fighting)
            {
                return false;
            }

            // 생존 상태 확인
            if(!owner.Stats.IsAlive || !targetFinder.HasValidTarget)
            {
                return false;
            }

            return unitMovement.IsTargetInAttackRange;
        }

        private void PerformBasicAttack()
        {
            BattleUnit target = targetFinder.CurrentTarget;

            if(target == null ||
               target.Health == null||
               target.Stats == null ||
               !target.Stats.IsAlive)
            {
                targetFinder.ClearTarget();
                return;
            }

            float attackInterval = Mathf.Max(MinimumAttackInterval, owner.Stats.AttackInterval);

            nextAttackTime = Time.time + attackInterval;

            int finalDamage = DamageCalculator.CalculateBasicAttackDamage(
                owner.Stats,
                target.Stats);

            target.Health.TakeDamage(finalDamage, owner);
        }

    }
}