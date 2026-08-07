using UnityEngine;

namespace AFKHero.Battle
{
    public static class DamageCalculator
    {
        private const int MinimunDamage = 1;

        public static int CalculateBasicAttackDamage(UnitStats attackerStats, UnitStats defenderStats)
        {
            if(attackerStats == null)
            {
                Debug.LogError("공격 유닛의 UnitStats가 비어있습니다.");

                return 0;
            }

            if(defenderStats == null)
            {
                Debug.LogError("방어 유닛의 UnitStats가 비어있습니다");
                return 0;
            }

            if(attackerStats.AttackPower <= 0)
            {
                return 0;
            }

            int damageBeforeMinimum = attackerStats.AttackPower - defenderStats.Defense;

            return Mathf.Max(MinimunDamage, damageBeforeMinimum);
        }
    }
}