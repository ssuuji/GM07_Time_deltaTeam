using System;
using Newtonsoft.Json;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitHealth : MonoBehaviour
    {
        [Header("죽자마자 사라짐")]
        [SerializeField] private bool deactivateOnDeath = true;

        [Header("Test 로그 표시")]
        [SerializeField] private bool logDamage = true;

        private BattleUnit owner;
        private BattleManager battleManager;
        private bool isDead;

        // 유닛에 현재 체력이 들어가 있지않으면 현재 체력 0 대입
        public int CurrentHealth => owner != null && owner.Stats != null
            ? owner.Stats.CurrentHealth : 0;

        public int MaxHealth => owner != null && owner.Stats != null
            ? owner.Stats.MaxHealth : 0;

        public bool IsDead => isDead;

        // 현재 남아 있는 보호막
        public int CurrentShield { get; private set; }

        public event Action<BattleUnit, int> ShieldChanged;

        //유닛, 현재 체력, 최대 체력, 전달
        public event Action<BattleUnit, int, int> HealthChanged;

        public void Initialize(BattleUnit unitOwner, BattleManager manager)
        {
            owner = unitOwner;
            battleManager = manager;
            
            isDead = owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive;
            CurrentShield = 0;
        }
        public int TakeDamage(int finalDamage, BattleUnit attacker)
        {
            if (isDead || owner == null || owner.Stats == null || !owner.Stats.IsAlive)
            {
                return 0;
            }

            if(attacker != null && attacker.Team == owner.Team)
            {
                return 0;
            }

            // 궁극기 중 피해 처리 정지
            if(battleManager != null && battleManager.IsDamageApplicationPaused)
            {
                return 0;
            }

            return ApplyDamageImmediately(finalDamage, attacker);
        }

        public int TakeUltimateDamage(int finalDamage, BattleUnit attacker)
        {
            if(isDead ||
                owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive ||
                attacker == null||
                attacker.Team == owner.Team ||
                battleManager == null ||
                battleManager.CurrentUltimateUnit != attacker ||
                finalDamage <= 0)
            {
                return 0;
            }
            return ApplyDamageImmediately(finalDamage,attacker);
        }

        private int ApplyDamageImmediately(int finalDamage, BattleUnit attacker)
        {
            if(isDead ||
                owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive ||
                finalDamage <= 0)
            {
                return 0;
            }

            int absorbedDamage =
               AbsorbDamageWithShield(finalDamage);

            int remainingDamage =
                finalDamage - absorbedDamage;

            int appliedHealthDamage =
                owner.Stats.ApplyDamage(remainingDamage);

            int totalAppliedDamage =
                absorbedDamage + appliedHealthDamage;

            if (totalAppliedDamage <= 0)
            {
                return 0;
            }

            if (appliedHealthDamage > 0)
            {
                HealthChanged?.Invoke(
                    owner,
                    CurrentHealth,
                    MaxHealth);
            }

            if (logDamage)
            {
                string attackName = attacker != null ? attacker.name : "Unknown";

                Debug.Log($"[피해] {attackName} -> {owner.name} / [데미지] {appliedHealthDamage} / " +
                    $"[보호막 흡수] {absorbedDamage} / [HP] ({owner.Stats.CurrentHealth})/({owner.Stats.MaxHealth})");
            }

            if(owner.Stats.IsAlive)
            {
                owner.Energy?.GainFromDamageTake();
            }
            else
            {
                Die();
            }
            return totalAppliedDamage;
        }

        // 회복
        public int RestoreHealthFromUltimate(
           int amount,
           BattleUnit healer)
        {
            if (isDead ||
                owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive ||
                healer == null ||
                healer.Team != owner.Team ||
                battleManager == null ||
                battleManager.CurrentUltimateUnit != healer ||
                amount <= 0)
            {
                return 0;
            }

            int restoredHealth =
                owner.Stats.RestoreHealth(amount);

            if (restoredHealth <= 0)
            {
                return 0;
            }

            HealthChanged?.Invoke(
                owner,
                CurrentHealth,
                MaxHealth);

            Debug.Log(
                $"[궁극기 회복] {healer.name} -> {owner.name} / " +
                $"[회복량] {restoredHealth} / " +
                $"[HP] {CurrentHealth}/{MaxHealth}",
                owner);

            return restoredHealth;
        }

        // 보호막
        public int AddShieldFromUltimate(
            int amount,
            BattleUnit shieldProvider)
        {
            if (isDead ||
                owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive ||
                shieldProvider == null ||
                shieldProvider.Team != owner.Team ||
                battleManager == null ||
                battleManager.CurrentUltimateUnit != shieldProvider ||
                amount <= 0)
            {
                return 0;
            }

            int previousShield =
                CurrentShield;

            CurrentShield = Mathf.Min(
                MaxHealth,
                CurrentShield + amount);

            int addedShield =
                CurrentShield - previousShield;

            if (addedShield <= 0)
            {
                return 0;
            }

            ShieldChanged?.Invoke(
                owner,
                CurrentShield);

            Debug.Log(
                $"[궁극기 보호막] {shieldProvider.name} -> {owner.name} / " +
                $"[추가량] {addedShield} / " +
                $"[현재 보호막] {CurrentShield}",
                owner);

            return addedShield;
        }

        // 유닛 죽음
        private void Die()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;

            // 타겟 참조 clear
            owner.TargetFinder?.ClearTarget();

            // 죽으면 군중제어 해제
            owner.StatusEffects?.ClearAllStatusEffects();

            Debug.Log($"[죽음] {owner.name}", owner);

            // 승리/패배 판정 후 오브젝트 비활성화
            battleManager?.NotifyUnitDied(owner);

            if (deactivateOnDeath)
            {
                gameObject.SetActive(false);
            }
        }
        private int AbsorbDamageWithShield(int damage)
        {
            if (damage <= 0 || CurrentShield <= 0)
            {
                return 0;
            }

            int absorbedDamage =
                Mathf.Min(CurrentShield, damage);

            CurrentShield -= absorbedDamage;

            ShieldChanged?.Invoke(
                owner,
                CurrentShield);

            return absorbedDamage;
        }
    }
}