using AFKHero.UI;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.Quest
{
    //EditorWindow - 퀘스트 SO 조회 / 수정 / 삭제 / 미리보기
    public class QuestEditorWindow : EditorWindow
    {
        private const string QuestRootPath = "Assets/03.Data/Resources/Quest";                      //퀘스트 SO 저장 경로
        private const string PreviewContentPath = "Quest_Daily_Repeat_List/Viewport/Content";       //미리보기 Content 경로
        private const float QuestListWidth = 300f;                                                  //왼쪽 퀘스트 목록 너비

        //퀘스트 목록
        private QuestType selectedQuestType = QuestType.Daily;                      //현재 선택한 퀘스트 타입
        private List<QuestData> allQuestList = new List<QuestData>();               //전체 퀘스트
        private List<QuestData> questList = new List<QuestData>();                  //현재 탭에 표시할 퀘스트

        //선택 퀘스트
        private QuestData selectedQuestData;                                        //현재 선택한 퀘스트
        private SerializedObject selectedQuestObject;                               //선택한 퀘스트 수정용 SerializedObject

        //메인 퀘스트
        private ReorderableList mainQuestList;                                      //메인 퀘스트 순서 변경용 리스트

        //스크롤
        private Vector2 questListScrollPosition;                                    //퀘스트 목록 스크롤
        private Vector2 questEditScrollPosition;                                    //퀘스트 편집 영역 스크롤

        //검색 / 필터
        private string searchText = "";                                             //퀘스트 검색어
        private int enabledFilter;                                                  //0: 전체 / 1: 활성 / 2: 비활성

        //미리보기 프리팹
        [SerializeField] private GameObject questPanelPrefab;                       //실제 게임 QuestPanel 프리팹
        [SerializeField] private UIQuestSlot questSlotPrefab;                       //실제 게임 QuestSlot 프리팹

        //미리보기
        private GameObject previewRoot;                                             //미리보기 오브젝트 Root
        private Camera previewCamera;                                               //미리보기 카메라
        private RenderTexture previewTexture;                                       //카메라 렌더 결과
        private UIQuestSlot previewQuestSlot;                                       //미리보기 QuestSlot
        private RectTransform previewContent;                                       //실제 QuestPanel의 Content

        private int previewProgress;                                                //미리보기 진행도
        private bool previewCompleted;                                              //미리보기 보상 수령 완료 여부
        private float previewAspectRatio = 1f;                                      //QuestSlot 가로 / 세로 비율

        #region EditorWindow

        private void OnEnable()
        {
            LoadAllQuests();      //전체 퀘스트 불러오기
            SetupMainQuestList(); //메인 퀘스트 드래그 리스트 설정
        }

        private void OnDisable()
        {
            ClearQuestPreview(); //에디터가 닫힐 때 미리보기 오브젝트 제거
        }

        //Quest Editor 열기
        [MenuItem("AFKHero/Quest Editor")]
        private static void OpenWindow()
        {
            QuestEditorWindow window = GetWindow<QuestEditorWindow>("Quest Editor");
            window.minSize = new Vector2(700, 450);
        }

        private void OnGUI()
        {
            DrawHeader();        //상단 타이틀
            DrawQuestType();     //일일 / 반복 / 메인 탭

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            DrawQuestList();     //왼쪽 퀘스트 목록
            DrawSelectedQuest(); //오른쪽 선택 퀘스트 정보

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region 퀘스트 타입

        //상단 타이틀
        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Quest Editor", EditorStyles.boldLabel);
        }

        //일일 / 반복 / 메인 퀘스트 탭
        private void DrawQuestType()
        {
            QuestType previousType = selectedQuestType; //탭 변경 여부 확인용

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle(selectedQuestType == QuestType.Daily, "일일", EditorStyles.miniButtonLeft))
            {
                selectedQuestType = QuestType.Daily;
            }

            if (GUILayout.Toggle(selectedQuestType == QuestType.Repeat, "반복", EditorStyles.miniButtonMid))
            {
                selectedQuestType = QuestType.Repeat;
            }

            if (GUILayout.Toggle(selectedQuestType == QuestType.Main, "메인", EditorStyles.miniButtonRight))
            {
                selectedQuestType = QuestType.Main;
            }

            EditorGUILayout.EndHorizontal();

            if (previousType != selectedQuestType)
            {
                selectedQuestData = null;    //선택 퀘스트 초기화
                selectedQuestObject = null;
                mainQuestList.index = -1;    //메인 리스트 선택 초기화

                searchText = "";             //검색 초기화
                enabledFilter = 0;           //필터 초기화

                previewProgress = 0;         //미리보기 진행도 초기화
                previewCompleted = false;    //미리보기 완료 상태 초기화

                FilterQuestList();           //선택한 타입의 퀘스트만 다시 표시
            }
        }

        #endregion

        #region 퀘스트 목록

        //왼쪽 퀘스트 목록 표시
        private void DrawQuestList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(QuestListWidth), GUILayout.ExpandHeight(true));

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"{selectedQuestType} 퀘스트", EditorStyles.boldLabel);

            if (GUILayout.Button("+ 새 퀘스트", GUILayout.Width(100)))
            {
                QuestCreateWindow.OpenWindow(selectedQuestType); //현재 타입으로 퀘스트 생성창 열기
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            if (selectedQuestType != QuestType.Main)
            {
                DrawQuestFilter(); //일일 / 반복만 검색 및 활성 필터 표시
            }

            EditorGUILayout.Space(5);

            questListScrollPosition = EditorGUILayout.BeginScrollView(questListScrollPosition);

            if (selectedQuestType == QuestType.Main)
            {
                mainQuestList.DoLayoutList(); //메인은 드래그 가능한 리스트 표시
            }
            else
            {
                foreach (QuestData quest in questList)
                {
                    if (!IsQuestVisible(quest)) continue; //검색 / 활성 필터에 맞지 않으면 제외

                    string modifiedMark = EditorUtility.IsDirty(quest) ? " *" : ""; //수정 상태 표시
                    bool isSelected = selectedQuestData == quest;                    //현재 선택된 퀘스트인지 확인

                    if (GUILayout.Toggle(isSelected, $"{quest.QuestId}  {quest.QuestName}{modifiedMark}", EditorStyles.miniButton))
                    {
                        if (!isSelected)
                        {
                            selectedQuestData = quest;                               //선택 퀘스트 저장
                            selectedQuestObject = new SerializedObject(quest);       //선택 퀘스트 수정용 객체 생성

                            previewProgress = 0;                                     //미리보기 초기화
                            previewCompleted = false;
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        //퀘스트 검색 / 활성 상태 필터
        private void DrawQuestFilter()
        {
            searchText = EditorGUILayout.TextField("검색", searchText);                              //퀘스트 ID / 이름 검색
            enabledFilter = GUILayout.Toolbar(enabledFilter, new[] { "전체", "활성", "비활성" });     //활성 상태 필터
        }

        //검색 / 활성 상태 조건에 맞는 퀘스트인지 확인
        private bool IsQuestVisible(QuestData quest)
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                bool idMatch = quest.QuestId.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase) >= 0;      //ID 검색
                bool nameMatch = quest.QuestName.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase) >= 0;  //이름 검색

                if (!idMatch && !nameMatch)
                {
                    return false;
                }
            }

            switch (enabledFilter)
            {
                case 1:
                    if (!quest.IsEnabled) return false; //활성 퀘스트만 표시
                    break;

                case 2:
                    if (quest.IsEnabled) return false;  //비활성 퀘스트만 표시
                    break;
            }

            return true;
        }

        //전체 QuestData 불러오기
        private void LoadAllQuests()
        {
            allQuestList.Clear();

            string[] guids = AssetDatabase.FindAssets("t:QuestData", new[] { QuestRootPath }); //QuestRootPath 아래 QuestData 검색

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);                              //GUID -> Asset 경로
                QuestData questData = AssetDatabase.LoadAssetAtPath<QuestData>(path);           //QuestData 불러오기

                if (questData != null)
                {
                    allQuestList.Add(questData);
                }
            }

            FilterQuestList(); //현재 선택한 타입의 퀘스트만 필터링
        }

        //현재 선택한 타입의 퀘스트만 목록에 저장
        private void FilterQuestList()
        {
            questList.Clear();

            foreach (QuestData quest in allQuestList)
            {
                if (quest.QuestType == selectedQuestType)
                {
                    questList.Add(quest);
                }
            }

            if (selectedQuestType == QuestType.Main)
            {
                questList.Sort((a, b) => a.Order.CompareTo(b.Order)); //메인은 Order 순으로 정렬
            }
        }

        #endregion

        #region 퀘스트 편집

        //선택한 퀘스트 정보 표시 / 수정
        private void DrawSelectedQuest()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            questEditScrollPosition = EditorGUILayout.BeginScrollView(questEditScrollPosition);

            if (selectedQuestData == null || selectedQuestObject == null)
            {
                EditorGUILayout.HelpBox("편집할 퀘스트를 선택해주세요.", MessageType.Info);

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                return;
            }

            selectedQuestObject.Update(); //현재 QuestData 값을 SerializedObject에 반영

            string modifiedMark = EditorUtility.IsDirty(selectedQuestData) ? " *" : ""; //수정 상태 표시

            EditorGUILayout.LabelField($"퀘스트 정보{modifiedMark}", EditorStyles.boldLabel);

            //기본 정보
            EditorGUILayout.PropertyField(selectedQuestObject.FindProperty("questId"));
            EditorGUILayout.PropertyField(selectedQuestObject.FindProperty("questName"));
            EditorGUILayout.PropertyField(selectedQuestObject.FindProperty("description"));
            EditorGUILayout.PropertyField(selectedQuestObject.FindProperty("isEnabled"));

            EditorGUILayout.Space(5);

            //완료 조건
            SerializedProperty conditionProperty = selectedQuestObject.FindProperty("conditionType");
            EditorGUILayout.PropertyField(conditionProperty);

            QuestConditionType conditionType = (QuestConditionType)conditionProperty.enumValueIndex; //현재 완료 조건

            switch (conditionType)
            {
                case QuestConditionType.DailyLogin:
                    break;

                case QuestConditionType.HeroSummon:
                case QuestConditionType.HeroLevelUp:
                case QuestConditionType.EnemyKill:
                case QuestConditionType.PartyDeploy:
                case QuestConditionType.DailyQuestRewardClaim:
                    EditorGUILayout.PropertyField(selectedQuestObject.FindProperty("targetCount")); //목표 수치
                    break;

                case QuestConditionType.StageClear:
                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("스테이지 클리어 조건", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(selectedQuestObject.FindProperty("targetStageNumber"), new GUIContent("스테이지"));
                    EditorGUILayout.PropertyField(selectedQuestObject.FindProperty("targetSectionNumber"), new GUIContent("구간"));
                    break;
            }

            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(selectedQuestObject.FindProperty("rewards"), true); //보상 목록

            //메인 퀘스트 가이드
            if (selectedQuestData.QuestType == QuestType.Main)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("가이드", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(selectedQuestObject.FindProperty("guideTarget"),new GUIContent("목적지"));

                EditorGUILayout.PropertyField(selectedQuestObject.FindProperty("autoGuide"),new GUIContent("자동 가이드"));
            }

            selectedQuestObject.ApplyModifiedProperties(); //Editor에서 수정한 값을 실제 QuestData에 적용

            EditorGUILayout.Space(10);

            DrawQuestPreview(selectedQuestData); //실제 UI 미리보기

            EditorGUI.BeginDisabledGroup(!EditorUtility.IsDirty(selectedQuestData)); //변경사항이 없으면 저장 버튼 비활성화

            if (GUILayout.Button("저장", GUILayout.Height(30)))
            {
                AssetDatabase.SaveAssetIfDirty(selectedQuestData); //변경된 QuestData 저장
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(5);

            if (GUILayout.Button("퀘스트 삭제", GUILayout.Height(25)))
            {
                DeleteSelectedQuest(); //현재 선택 퀘스트 삭제
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region 메인 퀘스트 순서

        //메인 퀘스트 드래그 리스트 설정
        private void SetupMainQuestList()
        {
            mainQuestList = new ReorderableList(
                questList,
                typeof(QuestData),
                true,   //드래그 순서 변경
                false,  //Header
                false,  //추가 버튼
                false   //삭제 버튼
            );

            //각 메인 퀘스트 표시
            mainQuestList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index < 0 || index >= questList.Count) return;

                QuestData quest = questList[index];

                EditorGUI.LabelField(rect, $"{index + 1}.  {quest.QuestId}  {quest.QuestName}");
            };

            //메인 퀘스트 선택
            mainQuestList.onSelectCallback = list =>
            {
                if (list.index < 0 || list.index >= questList.Count) return;

                selectedQuestData = questList[list.index];                           //선택 퀘스트
                selectedQuestObject = new SerializedObject(selectedQuestData);      //선택 퀘스트 수정용 객체

                previewProgress = 0;                                                //미리보기 초기화
                previewCompleted = false;
            };

            //드래그로 순서 변경
            mainQuestList.onReorderCallback = list =>
            {
                UpdateMainQuestOrder(); //변경된 리스트 순서에 맞춰 Order 갱신
            };
        }

        //현재 리스트 순서에 맞게 메인 퀘스트 Order 재설정
        private void UpdateMainQuestOrder()
        {
            for (int i = 0; i < questList.Count; i++)
            {
                QuestData quest = questList[i];
                SerializedObject questObject = new SerializedObject(quest);                 //QuestData 수정용 객체

                questObject.FindProperty("order").intValue = i + 1;                         //현재 Index + 1을 Order로 설정
                questObject.ApplyModifiedProperties();                                      //변경값 적용

                AssetDatabase.SaveAssetIfDirty(quest);                                      //변경된 QuestData 저장
            }
        }

        #endregion

        #region 미리보기

        //선택한 퀘스트 실제 UI 미리보기
        private void DrawQuestPreview(QuestData questData)
        {
            if (questData == null) return;

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("미리보기", EditorStyles.boldLabel);

            GameObject newPanelPrefab = (GameObject)EditorGUILayout.ObjectField("퀘스트 패널 프리팹", questPanelPrefab, typeof(GameObject), false);
            UIQuestSlot newSlotPrefab = (UIQuestSlot)EditorGUILayout.ObjectField("퀘스트 슬롯 프리팹", questSlotPrefab, typeof(UIQuestSlot), false);

            if (newPanelPrefab != questPanelPrefab || newSlotPrefab != questSlotPrefab)
            {
                questPanelPrefab = newPanelPrefab; //미리보기 Panel 변경
                questSlotPrefab = newSlotPrefab;   //미리보기 Slot 변경

                CreateQuestPreview();              //미리보기 다시 생성
            }

            if (questPanelPrefab == null || questSlotPrefab == null)
            {
                EditorGUILayout.HelpBox("QuestPanel과 QuestSlot 프리팹을 연결해주세요.", MessageType.Info);
                return;
            }

            if (questData.QuestType == QuestType.Main)
            {
                EditorGUILayout.HelpBox("메인 퀘스트는 별도 UI이므로 일일/반복 미리보기에서 제외됩니다.", MessageType.Info);
                return;
            }

            int maxProgress = Mathf.Max(1, questData.TargetCount); //미리보기 최대 진행도

            previewProgress = EditorGUILayout.IntSlider("진행도", previewProgress, 0, maxProgress); //가상 진행도

            if (questData.QuestType == QuestType.Daily)
            {
                previewCompleted = EditorGUILayout.Toggle("보상 수령 완료", previewCompleted); //완료 상태 테스트
            }
            else
            {
                previewCompleted = false;
            }

            if (previewTexture == null || previewQuestSlot == null || previewContent == null)
            {
                CreateQuestPreview(); //미리보기 오브젝트가 없으면 생성
            }

            if (previewQuestSlot == null || previewContent == null || previewTexture == null) return;

            previewQuestSlot.SetPreview(questData, previewProgress, previewCompleted); //현재 QuestData를 실제 Slot에 적용

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(previewContent);               //Content 레이아웃 다시 계산
            Canvas.ForceUpdateCanvases();

            float previewWidth = position.width - QuestListWidth - 80f;                //미리보기 영역 너비
            float previewHeight = previewWidth / previewAspectRatio;                   //QuestSlot 비율에 맞는 높이

            Rect previewRect = GUILayoutUtility.GetRect(100f, previewHeight, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
            {
                previewCamera.Render();                                                //Camera -> RenderTexture 렌더링
                GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit, false); //Editor에 미리보기 출력
            }
        }

        //실제 QuestPanel / QuestSlot을 사용한 미리보기 환경 생성
        private void CreateQuestPreview()
        {
            ClearQuestPreview(); //기존 미리보기 제거

            if (questPanelPrefab == null || questSlotPrefab == null) return;

            previewRoot = new GameObject("QuestPreviewRoot");
            previewRoot.hideFlags = HideFlags.HideAndDontSave;                         //Hierarchy / 저장 대상에서 숨김
            previewRoot.transform.position = new Vector3(10000f, 10000f, 0f);         //게임 오브젝트와 겹치지 않는 위치

            //미리보기 Canvas
            GameObject canvasObject = new GameObject("QuestPreviewCanvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.hideFlags = HideFlags.HideAndDontSave;
            canvasObject.transform.SetParent(previewRoot.transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;                                 //Camera로 촬영하기 위해 WorldSpace 사용

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1080f, 1920f);                          //실제 UI 기준 크기
            canvasRect.localPosition = Vector3.zero;
            canvasRect.localScale = Vector3.one * 0.01f;

            //실제 QuestPanel 생성 - Content 레이아웃 계산용
            GameObject panelObject = Instantiate(questPanelPrefab, canvasObject.transform);
            panelObject.name = "QuestPanelPreview";
            panelObject.hideFlags = HideFlags.HideAndDontSave;
            panelObject.SetActive(true);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.localScale = Vector3.one;

            //실제 QuestPanel 내부 Content 찾기
            Transform contentTransform = panelObject.transform.Find(PreviewContentPath);

            if (contentTransform == null)
            {
                Debug.LogError($"[QuestEditor] QuestPanel에서 Content를 찾을 수 없습니다. 경로 : {PreviewContentPath}");
                ClearQuestPreview();
                return;
            }

            previewContent = contentTransform.GetComponent<RectTransform>();

            if (previewContent == null)
            {
                Debug.LogError("[QuestEditor] Content에 RectTransform이 없습니다.");
                ClearQuestPreview();
                return;
            }

            //Content 안의 기존 슬롯 제거
            for (int i = previewContent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(previewContent.GetChild(i).gameObject);
            }

            //실제 게임처럼 Content 아래에 QuestSlot 생성
            previewQuestSlot = Instantiate(questSlotPrefab, previewContent);
            previewQuestSlot.gameObject.name = "QuestSlotPreview";
            previewQuestSlot.gameObject.hideFlags = HideFlags.HideAndDontSave;

            ResetPreviewZ(panelObject.transform);                                      //WorldSpace에서 UI Z값 문제 방지

            //실제 게임처럼 레이아웃 계산
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(previewContent);
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
            Canvas.ForceUpdateCanvases();

            //QuestSlot 실제 크기 계산
            RectTransform slotRect = previewQuestSlot.GetComponent<RectTransform>();

            Vector3[] corners = new Vector3[4];
            slotRect.GetWorldCorners(corners);                                         //QuestSlot 네 모서리 World 좌표

            float slotWidth = Vector3.Distance(corners[0], corners[3]);                 //QuestSlot 너비
            float slotHeight = Vector3.Distance(corners[0], corners[1]);                //QuestSlot 높이

            if (slotWidth <= 0f || slotHeight <= 0f)
            {
                Debug.LogError("[QuestEditor] QuestSlot의 크기를 계산할 수 없습니다.");
                ClearQuestPreview();
                return;
            }

            previewAspectRatio = slotWidth / slotHeight;                                //QuestSlot 가로 / 세로 비율

            //QuestSlot 비율에 맞는 RenderTexture
            int textureWidth = 700;
            int textureHeight = Mathf.RoundToInt(textureWidth / previewAspectRatio);

            previewTexture = new RenderTexture(textureWidth, textureHeight, 24, RenderTextureFormat.ARGB32);
            previewTexture.hideFlags = HideFlags.HideAndDontSave;
            previewTexture.Create();

            //미리보기 Camera
            GameObject cameraObject = new GameObject("QuestPreviewCamera");
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            cameraObject.transform.SetParent(previewRoot.transform, false);

            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);
            previewCamera.orthographic = true;
            previewCamera.nearClipPlane = 0.1f;
            previewCamera.farClipPlane = 100f;
            previewCamera.targetTexture = previewTexture;                               //렌더 결과를 RenderTexture에 출력

            canvas.worldCamera = previewCamera;

            Vector3 slotCenter = (corners[0] + corners[2]) * 0.5f;                     //QuestSlot 중심 위치

            previewCamera.transform.position = new Vector3(slotCenter.x, slotCenter.y, slotCenter.z - 10f);
            previewCamera.orthographicSize = slotHeight * 0.52f;                       //QuestSlot 높이에 맞게 Camera 크기 설정

            Canvas.ForceUpdateCanvases();
        }

        //미리보기 오브젝트 제거
        private void ClearQuestPreview()
        {
            if (previewCamera != null)
            {
                previewCamera.targetTexture = null; //RenderTexture 연결 해제
                previewCamera = null;
            }

            if (previewTexture != null)
            {
                previewTexture.Release();           //RenderTexture 메모리 해제
                DestroyImmediate(previewTexture);
                previewTexture = null;
            }

            if (previewRoot != null)
            {
                DestroyImmediate(previewRoot);       //Canvas / Panel / Slot / Camera 함께 제거
                previewRoot = null;
            }

            previewQuestSlot = null;
            previewContent = null;
        }

        //미리보기 UI Z값 초기화
        private void ResetPreviewZ(Transform root)
        {
            RectTransform[] rectTransforms = root.GetComponentsInChildren<RectTransform>(true);

            foreach (RectTransform rectTransform in rectTransforms)
            {
                Vector3 position = rectTransform.localPosition;
                position.z = 0f;                             //WorldSpace Canvas 렌더링 문제 방지를 위해 Z = 0
                rectTransform.localPosition = position;
            }
        }

        #endregion

        #region 삭제 / 새로고침

        //선택한 퀘스트 삭제
        private void DeleteSelectedQuest()
        {
            if (selectedQuestData == null) return;

            bool isDelete = EditorUtility.DisplayDialog(
                "퀘스트 삭제",
                $"{selectedQuestData.QuestId} - {selectedQuestData.QuestName}\n\n정말 삭제하시겠습니까?",
                "삭제",
                "취소"
            );

            if (!isDelete) return;

            bool wasMainQuest = selectedQuestData.QuestType == QuestType.Main; //메인 퀘스트인지 확인
            string assetPath = AssetDatabase.GetAssetPath(selectedQuestData);   //삭제할 Asset 경로

            AssetDatabase.DeleteAsset(assetPath);                               //QuestData Asset 삭제

            selectedQuestData = null;                                           //선택 상태 초기화
            selectedQuestObject = null;
            Selection.activeObject = null;

            LoadAllQuests();                                                    //퀘스트 목록 다시 불러오기

            if (wasMainQuest)
            {
                UpdateMainQuestOrder();                                         //메인 삭제 후 남은 Order 재정렬
            }

            Repaint();                                                          //Editor 다시 그리기
        }

        //새 퀘스트 생성 후 열려있는 Quest Editor 갱신
        public static void RefreshOpenWindow(QuestData newQuest)
        {
            QuestEditorWindow[] windows = Resources.FindObjectsOfTypeAll<QuestEditorWindow>(); //열려있는 QuestEditor 찾기

            foreach (QuestEditorWindow window in windows)
            {
                window.LoadAllQuests();                                          //새 QuestData까지 다시 불러오기

                window.selectedQuestType = newQuest.QuestType;                   //새 퀘스트 타입으로 탭 변경
                window.FilterQuestList();                                        //해당 타입 목록 갱신

                window.selectedQuestData = newQuest;                             //새로 생성한 퀘스트 자동 선택
                window.selectedQuestObject = new SerializedObject(newQuest);

                window.previewProgress = 0;                                     //미리보기 초기화
                window.previewCompleted = false;

                if (newQuest.QuestType == QuestType.Main)
                {
                    window.mainQuestList.index = window.questList.IndexOf(newQuest); //Main 리스트 선택 위치 갱신
                }

                window.Repaint();                                                //Editor 다시 그리기
            }
        }

        #endregion
    }
}