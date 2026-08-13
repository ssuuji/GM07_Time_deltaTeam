using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitAnimationController : MonoBehaviour
    {
        private Animator animator;
        private BattleUnit owner;

        private readonly int hashState = Animator.StringToHash("State"); // 0:Idle, 1:Run, 4:Death
        private readonly int hashAttackNormal = Animator.StringToHash("Attack_Normal");
        private readonly int hashAttackBow = Animator.StringToHash("Attack_Bow");
        private readonly int hashAttackMagic = Animator.StringToHash("Attack_Magic");
        private readonly int hashDie = Animator.StringToHash("Die");

        private bool isDead = false;

        public void Initialize(BattleUnit unitOwner)
        {
            owner = unitOwner;
            animator = GetComponentInChildren<Animator>();
            isDead = false;
            PlayIdle(); // 초기 상태는 대기
        }

        // 대기 모션
        public void PlayIdle()
        {
            if (animator != null && !isDead) animator.SetInteger(hashState, 0);
        }

        // 이동 모션
        public void PlayMove()
        {
            if (animator != null && !isDead) animator.SetInteger(hashState, 1);
        }

        // 직업에 따른 공격 모션
        public void PlayAttack(JobType jobType)
        {
            if (animator == null || isDead) return;

            // 직업에 맞춰 애니메이션 트리거 작동
            if (jobType == JobType.Warrior || jobType == JobType.Tank)
                animator.SetTrigger(hashAttackNormal);
            else if (jobType == JobType.Archer)
                animator.SetTrigger(hashAttackBow);
            else
                animator.SetTrigger(hashAttackMagic);
        }

        // 사망 모션
        public void PlayDeath()
        {
            if (animator != null && !isDead)
            {
                isDead = true;
                animator.SetInteger(hashState, 4);
                animator.SetTrigger(hashDie);
            }
        }
    }
}