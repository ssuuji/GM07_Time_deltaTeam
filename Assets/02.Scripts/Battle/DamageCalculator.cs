using UnityEngine;

namespace AFKHero.Battle
{
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

            int scaledAttack = Mathf.RoundToInt(attackerStats.AttackPower * Mathf.Max(0f, attackMultiplier));

            int appliedDefense = Mathf.RoundToInt(defenderStats.Defense * (1f - Mathf.Clamp01(defenseIgnoreRate)));

            return Mathf.Max(MinimunDamage, scaledAttack - appliedDefense);
        }

        public static int CalculateUltimateHealing(UnitStats casterStats, float attackMultiPlier)
        {
            if(casterStats == null || casterStats.AttackPower <= 0 || attackMultiPlier <= 0f)
            {
                return 0;
            }

            return Mathf.Max(1, Mathf.RoundToInt(casterStats.AttackPower * Mathf.Max(0f, attackMultiPlier)));
        }
    }
}