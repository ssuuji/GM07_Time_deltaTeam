using System;
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
        private HeroBase ownerHero;

        private float nextAttackTime;

        public event Action<BattleUnit> BasicAttackStarted;

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
            ownerHero =
            owner != null ? owner.GetComponent<HeroBase>() : null;
            nextAttackTime = 0f;
        }

        private void Update()
        {
            if (!TryGetAttackableTarget(out BattleUnit target))
            {
                return;
            }

            if (Time.time < nextAttackTime)
            {
                return;
            }

            PerformBasicAttack(target);
        }

        private bool TryGetAttackableTarget(out BattleUnit target)
        {
            target = null;

            if (owner == null ||
                owner.Stats == null ||
                battleManager == null ||
                targetFinder == null ||
                unitMovement == null)
            {
                return false;
            }

            if (battleManager.CurrentState != BattleState.Fighting ||
                !owner.Stats.IsAlive ||
                !targetFinder.HasValidTarget)
            {
                return false;
            }

            if (owner.StatusEffects != null && !owner.StatusEffects.CanUseBasicAttack)
            {
                return false;
            }

            target = targetFinder.CurrentTarget;

            if (target == null ||
                target.Health == null ||
                target.Stats == null ||
                !target.Stats.IsAlive ||
                target.Team == owner.Team)
            {
                targetFinder.ClearTarget();
                target = null;
                return false;
            }

            // 이동에서 검사한 것과 동일한 타깃과 동일한 여유 사거리 사용
            return unitMovement.IsTargetInAttackRange(target);
        }



        private void PerformBasicAttack(BattleUnit target)
        {
            if (target == null ||
       target.Health == null ||
       target.Stats == null ||
       !target.Stats.IsAlive)
            {
                targetFinder.ClearTarget();
                return;
            }

            float attackInterval = Mathf.Max(MinimumAttackInterval, owner.Stats.AttackInterval);
            nextAttackTime = Time.time + attackInterval;

            BasicAttackStarted?.Invoke(target);

            if (TryLaunchProjectile(target))
            {
                return;
            }

            ApplyBasicAttackImmediately(target);
        }

        private bool TryLaunchProjectile(
            BattleUnit target)
        {
            if (owner == null ||
                owner.Data == null ||
                ownerHero == null)
            {
                return false;
            }

            JobType jobType =
                owner.Data.JobType;

            bool usesProjectile =
                jobType == JobType.Archer ||
                jobType == JobType.Mage ||
                jobType == JobType.Healer;

            if (!usesProjectile ||
                owner.Data.ProjectilePrefab == null)
            {
                return false;
            }

            HeroBase targetHero =
                target.GetComponent<HeroBase>();

            if (targetHero == null)
            {
                Debug.LogWarning(
                    $"[{target.name}] HeroBase가 없어 투사체 공격을 사용할 수 없습니다.",
                    target);

                return false;
            }

            return ownerHero.SpawnProjectile(targetHero);
        }

        private void ApplyBasicAttackImmediately(BattleUnit target)
        {
            int finalDamage = DamageCalculator.CalculateBasicAttackDamage(owner.Stats, target.Stats);
            int appliedDamage = target.Health.TakeDamage(finalDamage, owner);

            if (appliedDamage <= 0)
            {
                return;
            }

            owner.Energy?.GainFromBasicAttack();
        }
    }
}