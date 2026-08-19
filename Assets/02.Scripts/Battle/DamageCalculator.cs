using UnityEngine;

namespace AFKHero.Battle
{
    // 기본 공격 피해 계산
    public static class DamageCalculator
    {
        private const int MinimunDamage = 1;

        public static int CalculateBasicAttackDamage(UnitStats attackerStats, UnitStats defenderStats)
        {
            if (attackerStats == null)
            {
                Debug.LogError("공격 유닛의 UnitStats가 비어있습니다.");

                return 0;
            }

            if (defenderStats == null)
            {
                Debug.LogError("방어 유닛의 UnitStats가 비어있습니다");
                return 0;
            }

            if (attackerStats.AttackPower <= 0)
            {
                return 0;
            }

            int damageBeforeMinimum = attackerStats.AttackPower - defenderStats.Defense;

            return Mathf.Max(MinimunDamage, damageBeforeMinimum);
        }

        // 공격력 배율과 방어력 무시 배율 적용하여 궁극기 피해 계산 defenseIgnoreRate 가 1이면 100% 무시 0.5면 50% 무시
        public static int CalculateUltimateDamage(
                UnitStats attackerStats,
                UnitStats defenderStats,
                float attackMultiplier,
                float defenseIgnoreRate = 0f)
        {
            if (attackerStats == null || defenderStats == null)
            {
                return 0;
            }

            if (attackerStats.AttackPower <= 0 || attackMultiplier <= 0f)
            {
                return 0;
            }

            int scaledAttack = Mathf.RoundToInt(attackerStats.AttackPower * attackMultiplier);

            int appliedDefense = Mathf.RoundToInt(
                defenderStats.Defense * (1f - Mathf.Clamp01(defenseIgnoreRate)));

            return Mathf.Max(MinimunDamage, scaledAttack - appliedDefense);
        }

        // 시전자의 공격력과 회복 배율을 사용하여 궁극기 회복량 계산
        public static int CalculateUltimateHealing(
            UnitStats casterStats,
            float attackMultiplier)
        {
            if (casterStats == null || casterStats.AttackPower <= 0 || attackMultiplier <= 0f)
            {
                return 0;
            }

            return Mathf.Max(1, Mathf.RoundToInt(casterStats.AttackPower * attackMultiplier));
        }
    }
}