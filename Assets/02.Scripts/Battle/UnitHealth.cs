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
        public void TakeDamage(int finalDamage, BattleUnit attacker)
        {
            if (isDead || owner == null || owner.Stats == null || !owner.Stats.IsAlive)
            {
                return;
            }

            int appliedDamage = owner.Stats.ApplyDamage(finalDamage);

            if(appliedDamage <= 0)
            {
                return;
            }

            // 코드 확인용
            if (logDamage)
            {
                string attackName = attacker != null ? attacker.name : "Unknown";

                Debug.Log(
                    $"[기본 공격] {attackName} -> {owner.name} / " +
                    $"[피해] {appliedDamage} / " +
                    $"[HP] [{owner.Stats.CurrentHealth}]/[{owner.Stats.MaxHealth}]",owner);
            }

            if (!owner.Stats.IsAlive)
            {
                Die(attacker);
            }

        }

        // 유닛 죽음
        private void Die(BattleUnit attacker)
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