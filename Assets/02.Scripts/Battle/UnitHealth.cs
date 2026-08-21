using System;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitHealth : MonoBehaviour
    {
        private const string EvadeSetName = "EvadeSet";
        private const string ReviveSetName = "ReviveSet";
        private const string VampSetName = "VampSet";

        private const float TwoSetEvadeChance = 0.15f;
        private const float FourSetEvadeChance = 0.30f;

        private const float ReviveHealthRate = 0.40f;

        private const float TwoSetVampRate = 0.10f;
        private const float FourSetVampRate = 0.25f;

        [Header("죽자마자 사라짐")]
        [SerializeField] private bool deactivateOnDeath = true;

        [Header("Test 로그 표시")]
        [SerializeField] private bool logDamage = true;

        private BattleUnit owner;
        private BattleManager battleManager;

        private bool isDead;
        private bool hasRevived;

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

            hasRevived = false;
            CurrentShield = 0;
        }
        public int TakeDamage(int finalDamage, BattleUnit attacker)
        {
            if (isDead || owner == null || owner.Stats == null || !owner.Stats.IsAlive)
            {
                return 0;
            }

            if (attacker != null && attacker.Team == owner.Team)
            {
                return 0;
            }

            // 궁극기 중 피해 처리 정지
            if (battleManager != null && battleManager.IsDamageApplicationPaused)
            {
                return 0;
            }

            return ApplyDamageImmediately(finalDamage, attacker);
        }

        public int TakeUltimateDamage(int finalDamage, BattleUnit attacker)
        {
            if (isDead ||
                owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive ||
                attacker == null ||
                attacker.Team == owner.Team ||
                battleManager == null ||
                battleManager.CurrentUltimateUnit != attacker ||
                finalDamage <= 0)
            {
                return 0;
            }
            return ApplyDamageImmediately(finalDamage, attacker);
        }

        private int ApplyDamageImmediately(int finalDamage, BattleUnit attacker)
        {
            if (!CanReceiveDamage(finalDamage))
            {
                return 0;
            }

            if (TryEvadeDamage())
            {
                return 0;
            }

            int absorbedDamage =
                 AbsorbDamageWithShield(finalDamage);

            int remainingDamage =
                finalDamage - absorbedDamage;

            int appliedHealthDamage =
                owner.Stats.ApplyDamage(
                    remainingDamage);

            int totalAppliedDamage =
                absorbedDamage +
                appliedHealthDamage;

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
                string attackerName = attacker != null ? attacker.name : "Unknown";

                Debug.Log(
                    $"[피해] {attackerName} -> {owner.name} / " +
                    $"[체력 피해] {appliedHealthDamage} / " +
                    $"[보호막 피해] {absorbedDamage} / " +
                    $"[HP] {CurrentHealth}/{MaxHealth}",
                    owner);
            }

            if (!owner.Stats.IsAlive &&
                !TryReviveFromSet())
            {
                Die();
            }

            if (owner.Stats.IsAlive)
            {
                owner.Energy?.GainFromDamageTake();
            }

            return totalAppliedDamage;
        }

        // 회피
        private bool TryEvadeDamage()
        {
            if (owner == null ||
                owner.HeroInstance == null)
            {
                return false;
            }

            int setCount =
                owner.HeroInstance.GetSetCount(
                    EvadeSetName);

            float evadeChance = 0f;

            if (setCount >= 4)
            {
                evadeChance = FourSetEvadeChance;
            }
            else if (setCount >= 2)
            {
                evadeChance = TwoSetEvadeChance;
            }

            if (evadeChance <= 0f ||
                UnityEngine.Random.value >= evadeChance)
            {
                return false;
            }

            Debug.Log(
                $"<color=green>[회피 세트]</color> " +
                $"{owner.name}이 공격을 회피했습니다.",
                owner);

            return true;
        }

        // 부활세트
        private bool TryReviveFromSet()
        {
            if (hasRevived ||
                owner == null ||
                owner.HeroInstance == null ||
                owner.Stats == null)
            {
                return false;
            }

            int setCount =
                owner.HeroInstance.GetSetCount(
                    ReviveSetName);

            if (setCount < 4)
            {
                return false;
            }

            int reviveHealth =
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        MaxHealth *
                        ReviveHealthRate));

            int restoredHealth =
                owner.Stats.Revive(
                    reviveHealth);

            if (restoredHealth <= 0)
            {
                return false;
            }

            hasRevived = true;

            HealthChanged?.Invoke(
                owner,
                CurrentHealth,
                MaxHealth);

            Debug.Log(
                $"<color=cyan>[부활 세트]</color> " +
                $"{owner.name}이 체력 {CurrentHealth}으로 부활했습니다.",
                owner);

            return true;
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

        // 흡혈 세트
        public int ApplyLifeStealFromDamage(
            int appliedDamage)
        {
            if (isDead ||
                owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive ||
                owner.HeroInstance == null ||
                appliedDamage <= 0)
            {
                return 0;
            }

            int setCount =
                owner.HeroInstance.GetSetCount(
                    VampSetName);

            float vampRate = 0f;

            if (setCount >= 4)
            {
                vampRate =
                    FourSetVampRate;
            }
            else if (setCount >= 2)
            {
                vampRate =
                    TwoSetVampRate;
            }

            if (vampRate <= 0f)
            {
                return 0;
            }

            int healingAmount =
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        appliedDamage *
                        vampRate));

            int restoredHealth =
                owner.Stats.RestoreHealth(
                    healingAmount);

            if (restoredHealth <= 0)
            {
                return 0;
            }

            HealthChanged?.Invoke(
                owner,
                CurrentHealth,
                MaxHealth);

            Debug.Log(
                $"<color=red>[흡혈 세트]</color> " +
                $"{owner.name}이 체력을 {restoredHealth} 회복했습니다.",
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

        private bool CanReceiveDamage(
            int damage)
        {
            return !isDead &&
                   owner != null &&
                   owner.Stats != null &&
                   owner.Stats.IsAlive &&
                   damage > 0;
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