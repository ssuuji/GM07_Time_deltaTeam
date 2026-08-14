using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitBattleView : MonoBehaviour
    {
        [Header("애니메이터")]
        [SerializeField] private Animator animator;

        [Header("애니메이션 Trigger")]
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string hitTrigger = "Hit";

        [Header("데미지 텍스트")]
        [SerializeField] private DamageTextView damageTextPrefab;

        [SerializeField] private Transform damageTextAnchor;

        private BattleUnit owner;
        private UnitHealth unitHealth;
        private UnitAttackController attackController;

        private int previousHealth;

        private void Start()
        {
            owner = GetComponent<BattleUnit>();

            if(owner == null || !owner.IsInitialized)
            {
                Debug.LogError("초기화 된 BattleUnit을 찾을 수 없습니다.",this);

                enabled = false;
                return;
            }

            if(animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            unitHealth = owner.Health;
            attackController = owner.AttackController;

            if(damageTextAnchor == null)
            {
                damageTextAnchor = transform;
            }

            if(unitHealth != null)
            {
                previousHealth = unitHealth.CurrentHealth;
                unitHealth.HealthChanged += HandleHealthChanged;
            }

            if(attackController != null)
            {
                attackController.BasicAttackStarted += HandleBasicAttackStarted;
            }
        }

        // 기본 공격이 시작되면 실행
        private void HandleBasicAttackStarted(BattleUnit target)
        {
            if(animator == null || string.IsNullOrWhiteSpace(attackTrigger))
            {
                return;
            }

            animator.SetTrigger(attackTrigger);
        }

        // 체력 감소량 계산해서 Hit Trigger와 데미지 텍스트 실행
        private void HandleHealthChanged(BattleUnit target, int currentHealth, int maxHealth)
        {
            int appliedDamage = Mathf.Max(0, previousHealth - currentHealth);

            previousHealth = currentHealth;

            if(appliedDamage <= 0)
            {
                return;
            }

            if(animator != null && !string.IsNullOrWhiteSpace(hitTrigger))
            {
                animator.SetTrigger(hitTrigger);
            }

            if(damageTextPrefab != null)
            {
                DamageTextView damageText = Instantiate(damageTextPrefab);

                damageText.Play(appliedDamage, damageTextAnchor.position);
            }
        }

        private void OnDestroy()
        {
            if(unitHealth != null)
            {
                unitHealth.HealthChanged -= HandleHealthChanged;
            }

            if(attackController != null)
            {
                attackController.BasicAttackStarted -= HandleBasicAttackStarted;
            }
        }



    }
}