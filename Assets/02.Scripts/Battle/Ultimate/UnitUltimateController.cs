using System;
using System.Collections;
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

        private BattleUnit owner;
        private Coroutine executionRoutine;
        private Action<BattleUnit> completionCallback;

        public bool IsExecuting => executionRoutine != null;

        public event Action<BattleUnit> ExecutionStarted;
        public event Action<BattleUnit> ExecutionCompleted;

        public bool CheckCanUseUltimate()
        {
            // 데이터에 체크박스가 꺼져있으면 무조건 스킬 사용 불가
            if (!owner.Data.CanUseUltimate) return false;

            return true;
        }

        public void Initialize(BattleUnit UnitOwner)
        {
            owner = UnitOwner;

            if(owner == null)
            {
                Debug.LogError("궁극기 컨트롤러에 BattleUnit이 비어있습니다.", this);
            }
        }

        public bool TryExecute(Action<BattleUnit> onCompleted)
        {
            if (owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive ||
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

        // 사망 전투 종료 시 콜백 없이 현재 궁극기 중단
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