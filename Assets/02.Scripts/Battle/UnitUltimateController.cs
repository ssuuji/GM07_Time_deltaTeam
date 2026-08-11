using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitUltimateController : MonoBehaviour
    {
        private const float MinimumDuration = 0.01f;

        [Header("±√±ÿ±‚ ø¨√‚")]
        [SerializeField, Min(MinimumDuration)]
        private float ultimateDuration = 1.5f;

        [Header("±√±ÿ±‚ æ÷¥œ∏ﬁ¿Ã≈Õ")]
        [SerializeField] private Animator animator;
        [SerializeField] private string animationTrigger = "Ultimate";

        private BattleUnit owner;
        private Coroutine executionRoutine;
        private Action<BattleUnit> completionCallback;

        public bool IsExecuting => executionRoutine != null;

        public event Action<BattleUnit> ExecutionStarted;
        public event Action<BattleUnit> ExecutionCompleted;

        public void Initialized(BattleUnit UnitOwner)
        {
            owner = UnitOwner;

            if(owner == null)
            {
                Debug.LogError("±√±ÿ±‚ Ω««‡¿⁄¿« BattleUnit¿Ã æ¯Ω¿¥œ¥Ÿ.", this);
            }
        }

        public bool TryExecute(Action<BattleUnit> onCompleted)
        {
            if (owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive ||
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
            if(!IsExecuting ||
               owner == null ||
                target == null ||
                target.Health == null ||
                finalDamage <= 0)
            {
                return 0;
            }

            return target.Health.TakeUltimateDamage(finalDamage, owner);
        }

        // ªÁ∏¡ ¿¸≈ı ¡æ∑· Ω√ ƒ›πÈ æ¯¿Ã «ˆ¿Á ±√±ÿ±‚ ¡ﬂ¥‹
        public void CancelUltimate()
        {
            if(executionRoutine != null)
            {
                StopCoroutine(executionRoutine);
                executionRoutine = null;
            }
            completionCallback = null;
        }

        private IEnumerator ExecuteRoutine()
        {
            yield return new WaitForSeconds(Mathf.Max(MinimumDuration, ultimateDuration));

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