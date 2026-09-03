using System;
using System.Collections;
using AFKHero.Sound;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitUltimateController : MonoBehaviour
    {
        private const float MinimumDuration = 0.01f;

        [Header("궁극기 시작음 대기 시간")]
        [SerializeField, Min(0f)] private float startSoundDelay = 0.5f;

        [Header("궁극기 실행 시간")]
        [SerializeField, Min(MinimumDuration)]
        private float ultimateDuration = 1.5f;

        [Header("궁극기 애니메이터")]
        [SerializeField] private Animator animator;
        [SerializeField] private string animationTrigger = "Ultimate";

        [Header("궁극기 효과 적용 시점")]
        [SerializeField, Min(0f)]
        private float effectDelay = 0.6f;

        [Header("테스트 궁극기 디버그")]
        [SerializeField]
        private bool showUltimateDebugLog = true;

        private BattleManager battleManager;
        private BattleUnit owner;
        private HeroBase heroBase;

        private Coroutine executionRoutine;
        private Action<BattleUnit> completionCallback;
        public bool IsExecuting => executionRoutine != null;

        public bool ShowUltimateDebugLog => showUltimateDebugLog;

        public event Action<BattleUnit> ExecutionStarted;
        public event Action<BattleUnit> ExecutionCompleted;

        public bool CheckCanUseUltimate()
        {
            return owner != null &&
                   owner.Data != null &&
                   owner.Data.CanUseUltimate;
        }

        public void LogUltimateDebug()
        {
            if (!showUltimateDebugLog ||
                owner == null ||
                owner.Data == null)
            {
                return;
            }

            HeroInstance heroInstance =
                owner.HeroInstance;

            int heroLevel =
                heroInstance != null
                    ? heroInstance.level
                    : 0;

            string heroGrade =
                heroInstance != null
                    ? heroInstance.currentGrade.ToString()
                    : "없음";

            float levelMultiplierBonus =
                JobUltimateSkill.GetLevelMultiplierBonus(
                    owner);

            bool gradeEffectActive =
                heroInstance != null &&
                (int)heroInstance.currentGrade >=
                (int)HeroGrade.Epic;

            string ultimateDescription =
                GetJobUltimateDescription(
                    owner.Data.JobType);

            Debug.Log(
                $"<color=#FFD54F>[궁극기 디버그]</color>\n" +
                $"사용 유닛: {owner.Data.HeroName} / 진영: {owner.Team}\n" +
                $"직업: {owner.Data.JobType} / 스킬: {owner.Data.UltimateSkillName}\n" +
                $"효과: {ultimateDescription}\n" +
                $"레벨: {heroLevel} / 등급: {heroGrade}\n" +
                $"레벨 배율 보정: +{levelMultiplierBonus:0.##} / " +
                $"Epic 추가 효과: {(gradeEffectActive ? "ON" : "OFF")}",
                owner);
        }

        private static string GetJobUltimateDescription(
            JobType jobType)
        {
            switch (jobType)
            {
                case JobType.Healer:
                    return "아군 전체 회복 / Epic일 때 기절·침묵 해제";

                case JobType.Warrior:
                    return "주변 적 광역 피해 / Epic일 때 기절 적용";

                case JobType.Mage:
                    return "가장 가까운 적 주변 범위 피해 / Epic일 때 침묵 적용";

                case JobType.Archer:
                    return "가장 먼 적에게 단일 피해 / Epic일 때 방어력 무시";

                case JobType.Tank:
                    return "자신에게 보호막 적용 / Epic일 때 아군 추가 회복";

                default:
                    return "등록되지 않은 직업 궁극기";
            }
        }

        public void Initialize(BattleUnit UnitOwner, BattleManager manager)
        {
            owner = UnitOwner;
            battleManager = manager;

            if (owner == null)
            {
                Debug.LogError("궁극기 컨트롤러에 BattleUnit이 비어있습니다.", this);

                return;
            }

            if (battleManager == null)
            {
                Debug.LogError( "궁극기 컨트롤러에 BattleManager가 비어있습니다.", this);

                return;
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            heroBase = owner.GetComponent<HeroBase>();

            if (heroBase == null)
            {
                Debug.LogError($"[{owner.name}] HeroBase가 없어 궁극기를 실행할 수 없습니다.", owner);

                return;
            }
        }

        public bool TryExecute(Action<BattleUnit> onCompleted)
        {
            if (owner == null ||
                battleManager == null ||
                heroBase == null ||
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

            PlayUltimateStartSFX();

            executionRoutine = StartCoroutine(ExecuteRoutine());
            ExecutionStarted?.Invoke(owner);

            return true;
        }

        private void PlayUltimateStartSFX()
        {
            if (SoundManager.Instance == null)
            {
                return;
            }

            SoundManager.Instance.PlaySFX(SoundKey.SFX_Ultimate_Start);
        }

        private void PlayUltimateJobSFX()
        {
            if (owner == null || owner.Data == null || SoundManager.Instance == null)
            {
                return;
            }

            SoundKey soundKey;

            // 같은 직업의 영웅은 동일한 궁극기 효과음을 사용합니다.
            switch (owner.Data.JobType)
            {
                case JobType.Warrior:
                    soundKey = SoundKey.SFX_Ultimate_Warrior;
                    break;

                case JobType.Tank:
                    soundKey = SoundKey.SFX_Ultimate_Tank;
                    break;

                case JobType.Mage:
                    soundKey = SoundKey.SFX_Ultimate_Mage;
                    break;

                case JobType.Archer:
                    soundKey = SoundKey.SFX_Ultimate_Archer;
                    break;

                case JobType.Healer:
                    soundKey = SoundKey.SFX_Ultimate_Healer;
                    break;

                default:
                    return;
            }

            SoundManager.Instance.PlaySFX(soundKey);
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

            int appliedDamage = target.Health.TakeUltimateDamage(finalDamage, owner);

            return appliedDamage;
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
            float safeStartSoundDelay = Mathf.Max(0f, startSoundDelay);

            // 궁극기 시작 효과음이 지나간 후 유닛의 궁극기 애니메이션을 시작
            if (safeStartSoundDelay > 0f)
            {
                yield return new WaitForSeconds(safeStartSoundDelay);
            }

            if (animator != null && !string.IsNullOrWhiteSpace(animationTrigger))
            {
                animator.SetTrigger(animationTrigger);
            }

            PlayUltimateJobSFX();

            float safeDuration =
                Mathf.Max(MinimumDuration, ultimateDuration);

            float safeEffectDelay =
                Mathf.Clamp(effectDelay, 0f, safeDuration);

            // 애니메이션이 실제로 타격하는 시점까지 기다립니다.
            if (safeEffectDelay > 0f)
            {
                yield return new WaitForSeconds(safeEffectDelay);
            }

            // 직업별 궁극기 효과는 한 번만 실행됩니다.
            bool effectApplied = heroBase.ExecuteUltimateEffect();

            // 궁극기 직업별 효과음 재생
            if (!effectApplied)
            {
                Debug.LogWarning($"[{owner.name}] 궁극기를 적용할 대상이 없습니다.", owner);
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
            animator = GetComponentInChildren<Animator>(true);
        }
#endif
    }


}