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

        public void Initialize(BattleUnit unitOwner, BattleManager manager)
        {
            owner = unitOwner;
            battleManager = manager;
            
            isDead = 
                owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive;
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

            return ApplyDamageImmediately(finalDamage,attacker);
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

            int appliedDamage = owner.Stats.ApplyDamage(finalDamage);

            if(appliedDamage <= 0)
            {
                return 0;
            }

            if (logDamage)
            {
                string attackName = attacker != null ? attacker.name : "Unknown";

                Debug.Log($"[피해] {attackName} -> {owner.name} / [데미지] {appliedDamage} / " +
                    $"[HP] ({owner.Stats.CurrentHealth})/({owner.Stats.MaxHealth})");
            }

            if(owner.Stats.IsAlive)
            {
                owner.Energy?.GainFromDamageTake();
            }
            else
            {
                Die();
            }
            return appliedDamage;
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

            Debug.Log($"[죽음] {owner.name}", owner);

            // 승리/패배 판정 후 오브젝트 비활성화
            battleManager?.NotifyUnitDied(owner);

            if (deactivateOnDeath)
            {
                gameObject.SetActive(false);
            }
        }

    }
}