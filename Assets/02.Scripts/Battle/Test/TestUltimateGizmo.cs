using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AFKHero.Battle
{
    // ===== 궁극기 범위 상시 표시 = 변경 시작 =====

    /// <summary>
    /// 궁극기 이펙트의 크기를 실제 스킬 범위에 맞출 수 있도록
    /// Scene 창에 범위 원과 이름을 항상 표시합니다.
    /// 실제 전투 판정에는 영향을 주지 않습니다.
    /// </summary>
    public sealed class UltimateRangeGizmo : MonoBehaviour
    {
        [Header("표시할 궁극기")]
        [SerializeField] private string skillName = "전사 휘두르기";

        [Header("실제 스킬 범위")]
        [SerializeField, Min(0f)] private float effectRange = 3f;

        [Header("표시 색상")]
        [SerializeField] private Color gizmoColor = new Color(1f, 0.3f, 0.1f, 0.9f);

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (effectRange <= 0f)
            {
                return;
            }

            Color previousHandlesColor = Handles.color;
            Handles.color = gizmoColor;

            // 2D 전투가 사용하는 XY 평면에 실제 스킬 범위를 표시합니다.
            Handles.DrawWireDisc(transform.position, Vector3.forward, effectRange);

            // 범위 원 위쪽에 스킬 이름과 범위 값을 표시합니다.
            Vector3 labelPosition = transform.position + Vector3.up * effectRange;
            Handles.Label(labelPosition, $"{skillName} 범위: {effectRange:0.##}");

            Handles.color = previousHandlesColor;
        }
#endif
    }

    // ===== 궁극기 범위 상시 표시 = 변경 끝 =====
}