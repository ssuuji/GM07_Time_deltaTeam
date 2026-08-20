using System;
using System.Collections;
using AFKHero.UI;
using JetBrains.Annotations;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitUltimateController : MonoBehaviour
    {
        private const float MinimumDuration = 0.01f;

        [Header("궁극기 연출")]
        [SerializeField, Min(MinimumDuration)]
        private float ultimateDuration = 1.5f;

        [Header("궁극기 애니메이터")]
        [SerializeField] private Animator animator;
        [SerializeField] private string animationTrigger = "Ultimate";

        [Header("궁극기 효과 적용 시점")]
        [SerializeField, Min(MinimumDuration)]
        private float effectDelay = 0.6f;

        private BattleManager battleManager;

        private BattleUnit owner;
        private Coroutine executionRoutine;
        private Action<BattleUnit> completionCallback;

        public bool IsExecuting => executionRoutine != null;

        public event Action<BattleUnit> ExecutionStarted;
        public event Action<BattleUnit> ExecutionCompleted;

        public bool CheckCanUseUltimate()
        {
            return owner != null &&
                   owner.Data != null &&
                   owner.Data.CanUseUltimate;
        }

        public void Initialize(BattleUnit UnitOwner, BattleManager manager)
        {
            owner = UnitOwner;
            battleManager = manager;

            if (owner == null)
            {
                Debug.LogError("궁극기 컨트롤러에 BattleUnit이 비어있습니다.", this);
            }

            if (battleManager == null)
            {
                Debug.LogError( "궁극기 컨트롤러에 BattleManager가 비어있습니다.", this);
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (animator == null)
            {
                Debug.LogWarning($"[{name}] 궁극기 애니메이션을 재생할 Animator를 찾지 못했습니다.", this);
            }
        }

        public bool TryExecute(Action<BattleUnit> onCompleted)
        {
            if (owner == null ||
                battleManager == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive ||
                !CheckCanUseUltimate() ||
                (owner.StatusEffects != null &&
                !owner.StatusEffects.CanUseUltimate) ||
                IsExecuting ||
                onCompleted == null)
            {
                return false;
            }

            completionCallback = onCompleted;

            if (animator != null && !string.IsNullOrWhiteSpace(animationTrigger))
            {
                animator.SetTrigger(animationTrigger);
            }

            executionRoutine = StartCoroutine(ExecuteRoutine());
            ExecutionStarted?.Invoke(owner);

            return true;
        }

        public int ApplyUltimateDamage(BattleUnit target, int finalDamage)
        {
            if (!IsExecuting ||
               owner == null ||
                target == null ||
                target.Health == null ||
                finalDamage <= 0)
            {
                return 0;
            }

            return target.Health.TakeUltimateDamage(finalDamage, owner);
        }

        // 사망 전투 종료 시 콜백 없이 현재 궁극기 중단
        public void CancelUltimate()
        {
            if (executionRoutine != null)
            {
                StopCoroutine(executionRoutine);
                executionRoutine = null;
            }
            completionCallback = null;
        }

        private IEnumerator ExecuteRoutine()
        {
            float safeDuration =
                Mathf.Max( MinimumDuration, ultimateDuration);

            float safeEffectDelay =
                Mathf.Clamp( effectDelay, MinimumDuration, safeDuration);

            // 애니메이션이 실제로 타격하는 시점까지 기다립니다.
            if (safeEffectDelay > 0f)
            {
                yield return new WaitForSeconds(safeEffectDelay);
            }

            // 직업별 궁극기 효과는 한 번만 실행됩니다.
            bool effectApplied =  JobUltimateSkill.TryExecute( owner, battleManager);

            if (!effectApplied)
            {
                Debug.LogWarning(  $"[{owner.name}] 궁극기를 적용할 대상이 없습니다.", owner);
            }

            float remainingDuration =  safeDuration - safeEffectDelay;

            if (remainingDuration > 0f)
            {
                yield return new WaitForSeconds( remainingDuration);
            }

            executionRoutine = null;

            Action<BattleUnit> callback = completionCallback;

            completionCallback = null;

            ExecutionCompleted?.Invoke(owner);
            callback?.Invoke(owner);
        }
        private void OnDestroy()
        {
            CancelUltimate();
        }
#if UNITY_EDITOR
        private void Reset()
        {
            animator = GetComponent<Animator>();
        }
#endif
    }


}