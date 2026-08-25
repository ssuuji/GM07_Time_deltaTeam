using System;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitHealth : MonoBehaviour
    {
        private const string EvadeSetName = "EvadeSet";
        private const string ReviveSetName = "ReviveSet";
        private const string VampSetName = "VampSet";

        //  추가 : 콤보 세트용 이름과 스택 저장 변수
        private const string ComboSetName = "ComboSet";
        private int comboStack = 0;

        // 추가 : 콤보가 터졌을 때 텍스트를 띄우기 위한 신호
        public event Action<BattleUnit, BattleUnit, int> OnComboExploded;

        private const float TwoSetEvadeChance = 0.15f;
        private const float FourSetEvadeChance = 0.30f;

        private const float ReviveHealthRate = 0.40f;

        private const float TwoSetVampRate = 0.10f;
        private const float FourSetVampRate = 0.25f;

        [Header("죽자마자 사라짐")]
        [SerializeField] private bool deactivateOnDeath = true;

        public event Action<BattleUnit> OnEvaded;

        [Header("Test 로그 표시")]
        [SerializeField] private bool logDamage = true;

        private BattleUnit owner;
        private BattleManager battleManager;

        private bool isDead;
        private bool hasRevived;

        // 무적 시간
        private float invincibleUntilTime;

        public bool IsInvincible => Time.time < invincibleUntilTime;

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
            invincibleUntilTime = 0f;
        }

        internal void UltimateCutInInvincibility(
            float duration)
        {
            if (duration <= 0f ||
                isDead ||
                owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive)
            {
                return;
            }

            float nextEndTime =
                Time.time + duration;

            invincibleUntilTime =
                Mathf.Max(
                    invincibleUntilTime,
                    nextEndTime);
        }

#if UNITY_EDITOR
        // 테스트 체력을 적용한 뒤 체력 UI가 즉시 갱신되도록 이벤트를 전달합니다.
        public void ApplyHealthMultiplierForTest(float multiplier)
        {
            if (owner == null ||
                owner.Stats == null)
            {
                return;
            }

            owner.Stats.ApplyHealthMultiplierForTest(
                multiplier);

            HealthChanged?.Invoke(
                owner,
                CurrentHealth,
                MaxHealth);
        }
#endif
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

            if (IsInvincible)
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
                IsInvincible ||
                attacker == null ||
                attacker.Team == owner.Team ||
                battleManager == null ||
                battleManager.CurrentUltimateUnit != attacker ||
                finalDamage <= 0)
            {
                return 0;
            }

            int appliedDamage =
                ApplyDamageImmediately(
                    finalDamage,
                    attacker);

            // 회피 등으로 피해가 적용되지 않은 경우도 테스트에서 확인합니다.
            if (appliedDamage <= 0 &&
                ShouldLogUltimateResult(attacker))
            {
                Debug.Log(
                    $"<color=#FF6B6B>[궁극기 피해]</color> " +
                    $"{attacker.name} → {owner.name} / " +
                    $"적용 피해: 0 (회피 또는 무효) / " +
                    $"대상 HP: {CurrentHealth}/{MaxHealth}",
                    owner);
            }

            return appliedDamage;
        }

        private int ApplyDamageImmediately(int finalDamage, BattleUnit attacker)
        {
            if (!CanReceiveDamage(finalDamage)) return 0;

            // 회피 적용 (성공 시 데미지 무시)
            if (TryEvadeDamage()) return 0;

            int absorbedDamage = AbsorbDamageWithShield(finalDamage);
            int remainingDamage = finalDamage - absorbedDamage;
            int appliedHealthDamage = owner.Stats.ApplyDamage(remainingDamage);
            int totalAppliedDamage = absorbedDamage + appliedHealthDamage;

            if (totalAppliedDamage <= 0) return 0;

            if (appliedHealthDamage > 0)
            {
                HealthChanged?.Invoke(owner, CurrentHealth, MaxHealth);

                // 흡혈 적용 : 내 피가 깎인 만큼 공격자에게 피를 채움
                if (attacker != null && attacker.Health != null)
                {
                    attacker.Health.ApplyLifeStealFromDamage(appliedHealthDamage);

                    // 추가 : 내가 맞았으니, 날 때린 공격자의 스택을 1 올려줌
                    attacker.Health.AddAttackStack(owner);
                }
            }

            if (logDamage)
            {
                bool isUltimateDamage =
               attacker != null &&
               battleManager != null &&
               battleManager.CurrentUltimateUnit ==
               attacker;

                // 일반 공격은 기존 logDamage 설정을 사용하고,
                // 궁극기는 시전자의 궁극기 디버그 설정을 사용합니다.
                bool shouldLogDamage =
                    isUltimateDamage
                        ? ShouldLogUltimateResult(attacker)
                        : logDamage;

                if (shouldLogDamage)
                {
                    string attackerName =
                        attacker != null
                            ? attacker.name
                            : "Unknown";

                    string damageType =
                        isUltimateDamage
                            ? "궁극기 피해"
                            : "피해";

                    string color =
                        isUltimateDamage
                            ? "#FF6B6B"
                            : "white";

                    Debug.Log(
                        $"<color={color}>[{damageType}]</color> " +
                        $"{attackerName} → {owner.name} / " +
                        $"총 적용 피해: {totalAppliedDamage} / " +
                        $"체력 피해: {appliedHealthDamage} / " +
                        $"보호막 흡수: {absorbedDamage} / " +
                        $"대상 HP: {CurrentHealth}/{MaxHealth}",
                        owner);
                }
            }

            // 부활 적용 구간 (죽었을 때 1회 부활)
            if (!owner.Stats.IsAlive && !TryReviveFromSet())
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

            OnEvaded?.Invoke(owner);

            Debug.Log(
                $"<color=green>[회피 세트]</color> " +
                $"{owner.name}이 공격을 회피했습니다.",
                owner);

            return true;
        }

        // 부활세트
        private bool TryReviveFromSet()
        {
            if (hasRevived || owner == null || owner.HeroInstance == null || owner.Stats == null) return false;

            int setCount = owner.HeroInstance.GetSetCount(ReviveSetName);

            if (setCount < 2) return false; // 2세트부터 부활 가능

            // 4세트면 40% 체력으로, 2세트면 20% 체력으로 부활
            float reviveRate = (setCount >= 4) ? ReviveHealthRate : 0.20f;

            int reviveHealth = Mathf.Max(1, Mathf.RoundToInt(MaxHealth * reviveRate));
            int restoredHealth = owner.Stats.Revive(reviveHealth);

            if (restoredHealth <= 0) return false;

            hasRevived = true;
            HealthChanged?.Invoke(owner, CurrentHealth, MaxHealth);

            Debug.Log($"<color=cyan>[부활 세트]</color> {owner.name}이 체력 {CurrentHealth}으로 부활했습니다.", owner);
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

            if (restoredHealth > 0)
            {
                HealthChanged?.Invoke(
                    owner,
                    CurrentHealth,
                    MaxHealth);
            }

            if (ShouldLogUltimateResult(healer))
            {
                Debug.Log(
                    $"<color=#66FF99>[궁극기 회복]</color> " +
                    $"{healer.name} → {owner.name} / " +
                    $"요청 회복량: {amount} / " +
                    $"실제 회복량: {restoredHealth} / " +
                    $"대상 HP: {CurrentHealth}/{MaxHealth}",
                    owner);
            }

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

            if (addedShield > 0)
            {
                ShieldChanged?.Invoke(
                    owner,
                    CurrentShield);
            }

            if (ShouldLogUltimateResult(shieldProvider))
            {
                Debug.Log(
                    $"<color=#66CCFF>[궁극기 보호막]</color> " +
                    $"{shieldProvider.name} → {owner.name} / " +
                    $"요청 보호막: {amount} / " +
                    $"실제 추가량: {addedShield} / " +
                    $"현재 보호막: {CurrentShield}",
                    owner);
            }

            return addedShield;
        }

        private static bool ShouldLogUltimateResult(
            BattleUnit caster)
        {
            return caster != null &&
                   caster.UltimateController != null &&
                   caster.UltimateController
                       .ShowUltimateDebugLog;
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

        public void AddAttackStack(BattleUnit target)
        {
            if (isDead || owner == null || owner.HeroInstance == null) return;

            int setCount = owner.HeroInstance.GetSetCount(ComboSetName);
            if (setCount < 2) return; // 2세트부터 발동

            comboStack++; // 스택 누적

            // 4세트면 2타마다, 2세트면 3타마다 터짐
            int requiredHits = (setCount >= 4) ? 2 : 3;

            if (comboStack >= requiredHits)
            {
                comboStack = 0; // 조건 달성 시 스택 초기화

                if (target != null && target.Stats != null && target.Stats.IsAlive)
                {
                    // 공격력의 150% 만큼 추가 피해 계산
                    int extraDamage = Mathf.RoundToInt(owner.HeroInstance.FinalAttack * 1.5f);
                    target.Stats.ApplyDamage(extraDamage);

                    // 화면에 "폭발" 텍스트를 띄우기 위해 밖으로 신호 발송
                    OnComboExploded?.Invoke(owner, target, extraDamage);

                    Debug.Log($"<color=orange>[콤보 공격]</color> {owner.name}이 {target.name}에게 {extraDamage}의 고정 피해를 입혔습니다!");
                }
            }
        }

    }
}