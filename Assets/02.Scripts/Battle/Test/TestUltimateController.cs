using UnityEngine;

namespace AFKHero.Battle
{
    public sealed  class TestUltimateController : MonoBehaviour
    {
        [Header("전투 관리자")]
        [SerializeField]
        private BattleManager battleManager;

        [Header("테스트 패널")]
        [SerializeField]
        private bool showPanel = true;

        // ===== [변경 시작: 테스트 패널 크기 설정 추가] =====

        [Tooltip("테스트 패널의 가로 크기입니다.")]
        [SerializeField, Min(320f)]
        private float panelWidth = 480f;

        [Tooltip("테스트 패널 제목의 글씨 크기입니다.")]
        [SerializeField, Min(20)]
        private int titleFontSize = 36;

        [Tooltip("상태 표시와 버튼에 사용하는 글씨 크기입니다.")]
        [SerializeField, Min(16)]
        private int contentFontSize = 26;

        [Tooltip("테스트 버튼의 세로 크기입니다.")]
        [SerializeField, Min(40f)]
        private float buttonHeight = 60f;

        // 제목, 상태 표시, 버튼에서 재사용할 UI 스타일입니다.
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;

        // ===== [변경 끝: 테스트 패널 크기 설정 추가] =====

        private void Awake()
        {
            if (battleManager == null)
            {
                battleManager =
                    FindObjectOfType<BattleManager>();
            }
        }

        private void OnGUI()
        {
            if (!showPanel ||
                battleManager == null)
            {
                return;
            }

            // ===== [변경 시작: 확대된 테스트 UI 스타일 준비] =====

            InitializeStyles();

            // ===== [변경 끝: 확대된 테스트 UI 스타일 준비] =====

            // 버튼을 누르는 동안 대기열이 변경될 수 있으므로
            // 실제 선택 요청은 반복문이 끝난 후 처리합니다.
            BattleUnit requestedUnit = null;

            GUILayout.BeginArea(
                new Rect(
                    10f,
                    10f,
                    panelWidth,
                    Screen.height - 20f),
                GUI.skin.box);

            // ===== [변경 시작: 제목과 상태 표시 글씨 확대] =====

            GUILayout.Label(
                "궁극기 자동/수동 테스트",
                titleStyle);

            GUILayout.Space(10f);

            GUILayout.Label(
                $"현재 상태: {battleManager.CurrentState}",
                labelStyle);

            GUILayout.Label(
                $"현재 모드: {battleManager.UltimateMode}",
                labelStyle);

            GUILayout.Space(10f);

            // ===== [변경 끝: 제목과 상태 표시 글씨 확대] =====

            // ===== [변경 시작: 모드 전환 버튼 확대] =====

            if (GUILayout.Button(
                    "자동 / 수동 모드 전환",
                    buttonStyle,
                    GUILayout.Height(buttonHeight)))
            {
                battleManager.ToggleUltimateUseMode();
            }

            // ===== [변경 끝: 모드 전환 버튼 확대] =====

            GUILayout.Space(16f);

            // ===== [변경 시작: 대기열 제목 글씨 확대] =====

            GUILayout.Label(
                "대기 중인 궁극기",
                titleStyle);

            // ===== [변경 끝: 대기열 제목 글씨 확대] =====

            var waitingUnits =
                battleManager.UltimateQueue.WaitingUnits;

            if (waitingUnits.Count == 0)
            {
                // ===== [변경 시작: 대기열 안내 글씨 확대] =====

                GUILayout.Label(
                    "대기 중인 유닛이 없습니다.",
                    labelStyle);

                // ===== [변경 끝: 대기열 안내 글씨 확대] =====
            }

            for (int i = 0;
                 i < waitingUnits.Count;
                 i++)
            {
                BattleUnit unit = waitingUnits[i];

                if (unit == null)
                {
                    continue;
                }

                // 수동 선택은 아군 궁극기만 가능합니다.
                bool canSelect =
                    unit.Team == TeamType.Ally;

                GUI.enabled = canSelect;

                string buttonText = canSelect
                    ? $"{i + 1}. {unit.name} 수동 사용"
                    : $"{i + 1}. {unit.name} (적 자동 사용)";

                // ===== [변경 시작: 궁극기 선택 버튼 확대] =====

                if (GUILayout.Button(
                        buttonText,
                        buttonStyle,
                        GUILayout.Height(buttonHeight)))
                {
                    requestedUnit = unit;
                    break;
                }

                // ===== [변경 끝: 궁극기 선택 버튼 확대] =====
            }

            // 이후 다른 OnGUI 요소가 비활성화되지 않도록 복구합니다.
            GUI.enabled = true;

            GUILayout.EndArea();

            // 버튼 클릭 직후 대기열이 변경될 수 있으므로
            // 대기열 반복이 끝난 다음 실제 선택을 요청합니다.
            if (requestedUnit != null)
            {
                battleManager.TrySelectQueueUltimate(
                    requestedUnit);
            }
        }

        // ===== [변경 시작: 테스트 UI 글씨 스타일 생성] =====

        /// <summary>
        /// 테스트 패널에 사용할 제목, 설명, 버튼 스타일을 준비합니다.
        /// Inspector에서 크기를 변경하면 플레이 중에도 바로 반영됩니다.
        /// </summary>
        private void InitializeStyles()
        {
            if (titleStyle == null)
            {
                titleStyle =
                    new GUIStyle(GUI.skin.label);
            }

            if (labelStyle == null)
            {
                labelStyle =
                    new GUIStyle(GUI.skin.label);
            }

            if (buttonStyle == null)
            {
                buttonStyle =
                    new GUIStyle(GUI.skin.button);
            }

            // 제목 스타일
            titleStyle.fontSize = titleFontSize;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment =
                TextAnchor.MiddleCenter;
            titleStyle.wordWrap = true;

            // 상태 및 안내 문구 스타일
            labelStyle.fontSize = contentFontSize;
            labelStyle.fontStyle = FontStyle.Normal;
            labelStyle.alignment =
                TextAnchor.MiddleLeft;
            labelStyle.wordWrap = true;

            // 버튼 스타일
            buttonStyle.fontSize = contentFontSize;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment =
                TextAnchor.MiddleCenter;
            buttonStyle.wordWrap = true;
        }

        // ===== [변경 끝: 테스트 UI 글씨 스타일 생성] =====

#if UNITY_EDITOR
        private void Reset()
        {
            battleManager =
                FindObjectOfType<BattleManager>();
        }
#endif
    }
}