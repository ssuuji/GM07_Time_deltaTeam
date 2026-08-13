using UnityEngine;
namespace AFKHero.Battle
{
    public sealed class BattleSystemTester : MonoBehaviour
    {
        [Header("전투 관리자")]

        [Tooltip("현재 테스트 씬의 BattleManager를 연결합니다.")]
        [SerializeField]
        private BattleManager battleManager;

        [Header("테스트 유닛")]

        [Tooltip("기절·침묵을 적용하거나 궁극기를 선택할 유닛입니다.")]
        [SerializeField]
        private BattleUnit testUnit;

        [Header("군중제어 지속시간")]

        [Tooltip("테스트할 기절 지속시간입니다.")]
        [SerializeField, Min(0.1f)]
        private float stunDuration = 3f;

        [Tooltip("테스트할 침묵 지속시간입니다.")]
        [SerializeField, Min(0.1f)]
        private float silenceDuration = 3f;

        /// <summary>
        /// 궁극기를 자동 모드로 변경합니다.
        /// </summary>
        [ContextMenu("궁극기/자동 모드로 변경")]
        private void SetAutomaticUltimateMode()
        {
            if (!ValidateBattleManager())
            {
                return;
            }

            battleManager.SetAutomaticUltimateUse(true);

            Debug.Log(
                "[테스트] 궁극기를 자동 모드로 변경했습니다.",
                battleManager);
        }

        /// <summary>
        /// 궁극기를 수동 모드로 변경합니다.
        /// </summary>
        [ContextMenu("궁극기/수동 모드로 변경")]
        private void SetManualUltimateMode()
        {
            if (!ValidateBattleManager())
            {
                return;
            }

            battleManager.SetAutomaticUltimateUse(false);

            Debug.Log(
                "[테스트] 궁극기를 수동 모드로 변경했습니다.",
                battleManager);
        }

        /// <summary>
        /// testUnit을 수동 궁극기 실행 대상으로 선택합니다.
        /// 아군이며 궁극기 에너지가 가득 차고 대기열에 있어야 합니다.
        /// </summary>
        [ContextMenu("궁극기/테스트 유닛 수동 선택")]
        private void SelectTestUnitUltimate()
        {
            if (!ValidateBattleManager() ||
                !ValidateTestUnit())
            {
                return;
            }

            bool wasSelected =
                battleManager.TrySelectQueueUltimate(testUnit);

            if (wasSelected)
            {
                Debug.Log(
                    $"[테스트] {testUnit.name}의 궁극기를 선택했습니다.",
                    testUnit);
            }
            else
            {
                Debug.LogWarning(
                    $"[테스트] {testUnit.name}의 궁극기를 선택하지 못했습니다. " +
                    "아군 여부, 에너지, 대기열, 기절·침묵 상태를 확인하세요.",
                    testUnit);
            }
        }

        /// <summary>
        /// testUnit에게 기절을 적용합니다.
        /// 기절 중에는 이동, 기본 공격, 궁극기를 사용할 수 없습니다.
        /// </summary>
        [ContextMenu("군중제어/테스트 유닛 기절")]
        private void ApplyStun()
        {
            if (!ValidateTestUnit())
            {
                return;
            }

            if (testUnit.StatusEffects == null)
            {
                Debug.LogError(
                    $"{testUnit.name}에 " +
                    "UnitStatusEffectController가 없습니다.",
                    testUnit);

                return;
            }

            testUnit.StatusEffects.ApplyStatusEffect(
                StatusEffectType.Stun,
                stunDuration);
        }

        /// <summary>
        /// testUnit에게 침묵을 적용합니다.
        /// 침묵 중에는 이동과 기본 공격이 가능하지만 궁극기는 사용할 수 없습니다.
        /// </summary>
        [ContextMenu("군중제어/테스트 유닛 침묵")]
        private void ApplySilence()
        {
            if (!ValidateTestUnit())
            {
                return;
            }

            if (testUnit.StatusEffects == null)
            {
                Debug.LogError(
                    $"{testUnit.name}에 " +
                    "UnitStatusEffectController가 없습니다.",
                    testUnit);

                return;
            }

            testUnit.StatusEffects.ApplyStatusEffect(
                StatusEffectType.Silence,
                silenceDuration);
        }

        /// <summary>
        /// testUnit에게 적용된 모든 군중제어를 즉시 해제합니다.
        /// </summary>
        [ContextMenu("군중제어/모든 상태 즉시 해제")]
        private void ClearAllStatusEffects()
        {
            if (!ValidateTestUnit())
            {
                return;
            }

            testUnit.StatusEffects?.ClearAllStatusEffects();

            Debug.Log(
                $"[테스트] {testUnit.name}의 모든 상태이상을 해제했습니다.",
                testUnit);
        }

        private bool ValidateBattleManager()
        {
            if (battleManager != null)
            {
                return true;
            }

            Debug.LogError(
                "[테스트] BattleManager가 연결되지 않았습니다.",
                this);

            return false;
        }

        private bool ValidateTestUnit()
        {
            if (testUnit != null)
            {
                return true;
            }

            Debug.LogError(
                "[테스트] 테스트할 BattleUnit이 연결되지 않았습니다.",
                this);

            return false;
        }
    }

    // ===== [변경 끝: 궁극기 모드 및 군중제어 테스트 기능 추가] =====
}

