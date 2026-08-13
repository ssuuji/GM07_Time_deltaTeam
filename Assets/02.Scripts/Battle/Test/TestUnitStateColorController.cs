using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class TestUnitStateColorController : MonoBehaviour
    {
        private enum VisualState
        {
            Normal,
            Ultimate,
            Stun,
            Silence
        }

        [Header("상태별 색상")]

        [Tooltip("궁극기 실행 중 표시할 색상입니다.")]
        [SerializeField]
        private Color ultimateColor =
            new Color(1f, 0.75f, 0.2f, 1f);

        [Tooltip("기절 중 표시할 색상입니다.")]
        [SerializeField]
        private Color stunColor =
            new Color(0.35f, 0.7f, 1f, 1f);

        [Tooltip("침묵 중 표시할 색상입니다.")]
        [SerializeField]
        private Color silenceColor =
            new Color(0.75f, 0.4f, 1f, 1f);

        [Header("색상을 변경할 이미지")]

        [Tooltip(
            "비어 있으면 현재 유닛의 모든 자식 SpriteRenderer를 " +
            "자동으로 찾습니다.")]
        [SerializeField]
        private SpriteRenderer[] spriteRenderers;

        private BattleUnit owner;

        // 각 SpriteRenderer가 원래 가지고 있던 색상입니다.
        private Color[] originalColors;

        private VisualState currentVisualState =
            VisualState.Normal;

        private void Awake()
        {
            owner = GetComponent<BattleUnit>();

            FindSpriteRenderers();
            SaveOriginalColors();
        }

        private void LateUpdate()
        {
            if (owner == null)
            {
                return;
            }

            VisualState nextState =
                GetCurrentVisualState();

            // 상태가 바뀌지 않았다면 색상을 다시 적용하지 않습니다.
            if (currentVisualState == nextState)
            {
                return;
            }

            currentVisualState = nextState;

            ApplyCurrentStateColor();
        }

        /// <summary>
        /// 현재 유닛 상태에 따라 표시할 상태를 결정합니다.
        /// 위쪽 조건일수록 색상 우선순위가 높습니다.
        /// </summary>
        private VisualState GetCurrentVisualState()
        {
            // 궁극기 실행 색상을 가장 먼저 표시합니다.
            if (owner.UltimateController != null &&
                owner.UltimateController.IsExecuting)
            {
                return VisualState.Ultimate;
            }

            if (owner.StatusEffects != null)
            {
                // 기절과 침묵이 동시에 적용되면
                // 행동 제한이 더 강한 기절 색상을 표시합니다.
                if (owner.StatusEffects.IsStunned)
                {
                    return VisualState.Stun;
                }

                if (owner.StatusEffects.IsSilenced)
                {
                    return VisualState.Silence;
                }
            }

            return VisualState.Normal;
        }

        /// <summary>
        /// 현재 상태에 맞는 색상을 모든 SpriteRenderer에 적용합니다.
        /// </summary>
        private void ApplyCurrentStateColor()
        {
            if (spriteRenderers == null ||
                originalColors == null)
            {
                return;
            }

            for (int i = 0;
                 i < spriteRenderers.Length;
                 i++)
            {
                SpriteRenderer spriteRenderer =
                    spriteRenderers[i];

                if (spriteRenderer == null)
                {
                    continue;
                }

                if (currentVisualState ==
                    VisualState.Normal)
                {
                    spriteRenderer.color =
                        originalColors[i];

                    continue;
                }

                Color stateColor =
                    GetStateColor(currentVisualState);

                spriteRenderer.color =
                    MultiplyColor(
                        originalColors[i],
                        stateColor);
            }
        }

        /// <summary>
        /// 상태에 대응하는 색상을 반환합니다.
        /// </summary>
        private Color GetStateColor(
            VisualState visualState)
        {
            switch (visualState)
            {
                case VisualState.Ultimate:
                    return ultimateColor;

                case VisualState.Stun:
                    return stunColor;

                case VisualState.Silence:
                    return silenceColor;

                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// 프리팹의 원래 색상에 상태 색상을 곱합니다.
        /// SpriteRenderer의 기존 투명도는 유지합니다.
        /// </summary>
        private static Color MultiplyColor(
            Color originalColor,
            Color stateColor)
        {
            return new Color(
                originalColor.r * stateColor.r,
                originalColor.g * stateColor.g,
                originalColor.b * stateColor.b,
                originalColor.a);
        }

        /// <summary>
        /// 색상을 바꿀 SpriteRenderer를 자동으로 찾습니다.
        /// 비활성화된 자식 오브젝트도 포함합니다.
        /// </summary>
        private void FindSpriteRenderers()
        {
            if (spriteRenderers != null &&
                spriteRenderers.Length > 0)
            {
                return;
            }

            spriteRenderers =
                GetComponentsInChildren<SpriteRenderer>(
                    true);
        }

        /// <summary>
        /// 상태가 끝났을 때 복구할 원래 색상을 저장합니다.
        /// </summary>
        private void SaveOriginalColors()
        {
            if (spriteRenderers == null)
            {
                originalColors = null;
                return;
            }

            originalColors =
                new Color[spriteRenderers.Length];

            for (int i = 0;
                 i < spriteRenderers.Length;
                 i++)
            {
                SpriteRenderer spriteRenderer =
                    spriteRenderers[i];

                originalColors[i] =
                    spriteRenderer != null
                        ? spriteRenderer.color
                        : Color.white;
            }
        }

        /// <summary>
        /// 사망 또는 풀 반환으로 오브젝트가 비활성화될 때
        /// 원래 색상으로 되돌립니다.
        /// </summary>
        private void OnDisable()
        {
            RestoreOriginalColors();

            currentVisualState =
                VisualState.Normal;
        }

        private void RestoreOriginalColors()
        {
            if (spriteRenderers == null ||
                originalColors == null)
            {
                return;
            }

            int restoreCount =
                Mathf.Min(
                    spriteRenderers.Length,
                    originalColors.Length);

            for (int i = 0;
                 i < restoreCount;
                 i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].color =
                        originalColors[i];
                }
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            spriteRenderers =
                GetComponentsInChildren<SpriteRenderer>(
                    true);
        }
#endif
    }

    // ===== [변경 끝: 궁극기 및 군중제어 색상 표시 추가] =====
}