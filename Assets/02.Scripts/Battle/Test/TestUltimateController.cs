using UnityEngine;

namespace AFKHero.Battle
{
    public sealed  class UltimateController : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private bool showPanel = true;
        [SerializeField, Min(240f)] private float panelWidth = 360f;

        private void Awake()
        {
            if (battleManager == null)
            {
                battleManager = FindObjectOfType<BattleManager>();
            }
        }

        private void OnGUI()
        {
            if (!showPanel || battleManager == null)
            {
                return;
            }

            BattleUnit requestedUnit = null;

            GUILayout.BeginArea(
                new Rect(10f, 10f, panelWidth, Screen.height - 20f),
                GUI.skin.box);

            GUILayout.Label("궁극기 자동/수동 테스트");
            GUILayout.Label($"전투 상태: {battleManager.CurrentState}");
            GUILayout.Label($"현재 모드: {battleManager.UltimateMode}");

            if (GUILayout.Button("자동 / 수동 모드 전환"))
            {
                battleManager.ToggleUltimateUseMode();
            }

            GUILayout.Space(8f);
            GUILayout.Label("대기 중인 궁극기");

            var waitingUnits = battleManager.UltimateQueue.WaitingUnits;

            if (waitingUnits.Count == 0)
            {
                GUILayout.Label("대기 중인 유닛이 없습니다.");
            }

            for (int i = 0; i < waitingUnits.Count; i++)
            {
                BattleUnit unit = waitingUnits[i];

                if (unit == null)
                {
                    continue;
                }

                bool canSelect = unit.Team == TeamType.Ally;
                GUI.enabled = canSelect;

                string buttonText = canSelect
                    ? $"{i + 1}. {unit.name} 먼저 사용"
                    : $"{i + 1}. {unit.name} (적 자동 사용)";

                if (GUILayout.Button(buttonText))
                {
                    requestedUnit = unit;
                    break;
                }
            }

            GUI.enabled = true;
            GUILayout.EndArea();

            // 버튼 클릭으로 대기열이 변경되므로 반복문이 끝난 뒤 실행합니다.
            if (requestedUnit != null)
            {
                battleManager.TrySelectQueueUltimate(requestedUnit);
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            battleManager = FindObjectOfType<BattleManager>();
        }
#endif
    }

    // ===== [변경 끝: 궁극기 자동/수동 선택 테스트 패널 추가] =====
}