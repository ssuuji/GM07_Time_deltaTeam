using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AFKHero.Quest
{
    //EditorWindow - 퀘스트 SO를 생성
    public class QuestCreateWindow : EditorWindow
    {
        //퀘스트 생성 방식
        private enum CreateMode
        {
            Single, //단일 생성
            Batch   //일괄 생성
        }
        
        private const string QuestRootPath = "Assets/03.Data/Resources/Quest"; //퀘스트 SO가 저장되는 최상위 경로

        private CreateMode createMode = CreateMode.Single;                   //현재 생성 방식
        private List<int> batchTargets = new List<int> { 10, 20, 30 };       //일괄 생성 시 사용할 목표 수치 ( 우선 10회 20회 30회 )

        //퀘스트 설정
        private QuestType questType;               //타입 : 일일/반복/메인
        private QuestConditionType conditionType;  //완료조건
        private int targetCount = 1;               //목표숫자

        //보상 설정
        private RewardType rewardType;            //타입 : 골드/다이아/무료뽑기권
        private int rewardAmount = 1;             //보상갯수

        //스테이지 클리어 조건
        private int targetStageNumber = 1;
        private int targetSectionNumber = 1;

        #region 생성창

        //퀘스트 생성창 열기
        public static void OpenWindow(QuestType questType)
        {
            QuestCreateWindow window = GetWindow<QuestCreateWindow>("새 퀘스트");
            window.questType = questType; //Quest Editor에서 선택한 타입을 전달받아 생성 타입으로 사용
            window.minSize = new Vector2(350, 300);
        }

        private void OnGUI()
        {
            DrawQuestType();         //퀘스트 타입
            DrawCondition();         //완료 조건
            DrawCreateMode();        //생성 방식
            DrawTarget();            //목표 설정
            DrawReward();            //보상 설정
            DrawValidationMessage(); //유효성 검사 안내
            DrawCreateButton();      //생성 버튼
        }

        #endregion

        #region 생성 UI

        //현재 생성할 퀘스트 타입 표시
        private void DrawQuestType()
        {
            EditorGUILayout.LabelField("퀘스트 타입", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(questType.ToString());
        }

        //퀘스트 완료 조건 선택
        private void DrawCondition()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("완료 조건", EditorStyles.boldLabel);

            
            QuestConditionType[] conditions = GetAvailableConditions(); //현재 QuestType에서 사용할 수 있는 조건 목록 가져오기
            string[] conditionNames = new string[conditions.Length];    //Enum 대신 한글로 이름 표시

            for (int i = 0; i < conditions.Length; i++)
            {
                conditionNames[i] = GetConditionName(conditions[i]);
            }
            
            int selectedIndex = System.Array.IndexOf(conditions, conditionType); //현재 conditionType이 목록에서 몇 번째인지 확인
            if (selectedIndex < 0)
            {
                selectedIndex = 0;
                conditionType = conditions[0]; //현재 값이 해당 QuestType에서 사용할 수 없는 조건이라면 첫 번째 조건으로 초기화
            }

            selectedIndex = EditorGUILayout.Popup("조건", selectedIndex, conditionNames); //완료 조건 드롭다운 선택
            conditionType = conditions[selectedIndex];                                    //선택한 조건 저장
        }

        //단일 / 일괄 생성 방식 선택
        private void DrawCreateMode()
        {
            if (!CanBatchCreate())
            {
                createMode = CreateMode.Single; //현재 퀘스트가 일괄 생성을 지원하지 않는다면 단일 생성으로 고정
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("생성 방식", EditorStyles.boldLabel);

            createMode = (CreateMode)GUILayout.Toolbar((int)createMode, new[] { "단일", "일괄" });
        }

        //퀘스트 목표 설정
        private void DrawTarget()
        {
            EditorGUILayout.Space(5);
            if (conditionType == QuestConditionType.DailyLogin) return; //일일 접속은 목표 수치를 별도로 입력하지 않음
            
            if (conditionType == QuestConditionType.StageClear)
            {
                //스테이지 클리어는 횟수가 아니라 Stage / Section을 입력
                targetStageNumber = EditorGUILayout.IntField("스테이지", targetStageNumber);
                targetSectionNumber = EditorGUILayout.IntField("구간", targetSectionNumber);
                return;
            }
            
            if (createMode == CreateMode.Batch)
            {
                DrawBatchTargets(); //일괄 생성 중이라면 여러 목표 수치를 입력
                return;
            }

            //조건에 따라 목표 수치의 표시 이름 변경
            switch (conditionType)
            {
                //소환, 레벨업, 일일보상수령횟수
                case QuestConditionType.HeroSummon:
                case QuestConditionType.HeroLevelUp:
                case QuestConditionType.DailyQuestRewardClaim:
                    targetCount = EditorGUILayout.IntField("목표 횟수", targetCount);
                    break;
                //적 처치
                case QuestConditionType.EnemyKill:
                    targetCount = EditorGUILayout.IntField("목표 마릿수", targetCount);
                    break;
                //파티배치
                case QuestConditionType.PartyDeploy:
                    targetCount = EditorGUILayout.IntField("목표 인원", targetCount);
                    break;
            }
        }

        //일괄 생성할 목표 수치 목록
        private void DrawBatchTargets()
        {
            EditorGUILayout.LabelField("목표 목록", EditorStyles.boldLabel);

            for (int i = 0; i < batchTargets.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                batchTargets[i] = EditorGUILayout.IntField($"목표 {i + 1}", batchTargets[i]); //각 퀘스트의 목표 수치
                
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    batchTargets.RemoveAt(i); //해당 목표 삭제
                    i--;                      //삭제 후 다음 항목이 현재 Index로 당겨지므로 건너뛰지 않도록 Index 보정
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ 목표 추가"))
            {
                batchTargets.Add(1); //새로운 목표 수치 추가
            }

            if (batchTargets.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("생성 예정", EditorStyles.boldLabel);

                foreach (int target in batchTargets)
                {
                    EditorGUILayout.LabelField($"• {GetQuestName(target)}"); //현재 입력값으로 생성될 퀘스트 이름 미리 표시
                }
            }
        }

        //보상 설정
        private void DrawReward()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("보상", EditorStyles.boldLabel);

            RewardType[] rewardTypes = (RewardType[])System.Enum.GetValues(typeof(RewardType)); //RewardType 전체 목록 가져오기
            
            string[] rewardNames = new string[rewardTypes.Length]; //Enum 한글로 이름을 표시
            for (int i = 0; i < rewardTypes.Length; i++)
            {
                rewardNames[i] = GetRewardName(rewardTypes[i]);
            }
            
            int selectedIndex = System.Array.IndexOf(rewardTypes, rewardType); //현재 RewardType의 Index 찾기
            if (selectedIndex < 0)
            {
                selectedIndex = 0;
                rewardType = rewardTypes[0];
            }
            
            selectedIndex = EditorGUILayout.Popup("보상 타입", selectedIndex, rewardNames);  //보상 타입 선택
            rewardType = rewardTypes[selectedIndex];
            
            rewardAmount = EditorGUILayout.IntField("수량", rewardAmount); //보상 수량
        }

        //생성 버튼
        private void DrawCreateButton()
        {
            EditorGUILayout.Space(15);
            
            bool isValid = IsValidQuest();//현재 입력값이 유효한지 확인
            EditorGUI.BeginDisabledGroup(!isValid); //유효하지 않다면 생성 버튼 비활성화

            if (GUILayout.Button("생성", GUILayout.Height(30)))
            {
                CreateQuest();
            }

            EditorGUI.EndDisabledGroup();
        }

        #endregion

        #region 퀘스트 생성

        //입력한 설정을 기준으로 QuestData 생성
        private void CreateQuest()
        {
            List<QuestData> existingQuests = LoadExistingQuests(); //현재 존재하는 퀘스트를 불러와 ID / Main Order 계산에 사용

            //마지막으로 생성된 퀘스트
            //생성 후 Quest Editor에서 자동으로 선택하기 위해 사용꺅
            QuestData lastCreatedQuest = null;

            //일괄 생성
            if (createMode == CreateMode.Batch && CanBatchCreate())
            {
                foreach (int target in batchTargets)
                {
                    lastCreatedQuest = CreateQuestAsset(existingQuests, target);
                }
            }
            //단일 생성
            else
            {
                lastCreatedQuest = CreateQuestAsset(existingQuests, targetCount);
            }

            AssetDatabase.SaveAssets();
            if (lastCreatedQuest != null)
            {
                Selection.activeObject = lastCreatedQuest; //Project창에서 마지막 생성 퀘스트 선택
                QuestEditorWindow.RefreshOpenWindow(lastCreatedQuest); //열려있는 Quest Editor 목록 갱신 후 생성한 퀘스트 선택
            }

            Close();
        }

        //QuestData SO 하나 생성
        private QuestData CreateQuestAsset(List<QuestData> existingQuests, int target)
        {
            string questId = GenerateQuestId(existingQuests); //중복되지 않는 QuestId 생성
            string folderPath = GetQuestFolderPath();         //QuestType에 맞는 저장 폴더 가져오기
            CreateQuestFolder(folderPath);                    //저장 폴더가 없다면 생성
            
            QuestData questData = CreateInstance<QuestData>(); //새로운 QuestData 생성
            
            //기본 정보
            SerializedObject questObject = new SerializedObject(questData); 
            questObject.FindProperty("questId").stringValue = questId;
            questObject.FindProperty("questType").enumValueIndex = (int)questType;
            questObject.FindProperty("questName").stringValue = GetQuestName(target);
            questObject.FindProperty("description").stringValue = GetQuestDescription(target);
            questObject.FindProperty("isEnabled").boolValue = true;

            //완료 조건
            questObject.FindProperty("conditionType").enumValueIndex = (int)conditionType;
            questObject.FindProperty("targetCount").intValue = GetTargetCount(target);

            //스테이지 클리어 조건
            questObject.FindProperty("targetStageNumber").intValue = targetStageNumber;
            questObject.FindProperty("targetSectionNumber").intValue = targetSectionNumber;

            //보상은 현재 하나만 자동 생성
            SerializedProperty rewards = questObject.FindProperty("rewards");
            rewards.arraySize = 1;

            SerializedProperty reward = rewards.GetArrayElementAtIndex(0);

            reward.FindPropertyRelative("rewardType").enumValueIndex = (int)rewardType;
            reward.FindPropertyRelative("amount").intValue = rewardAmount;

            //Main 퀘스트라면 진행 순서 / 가이드 목적지도 추가 설정
            if (questType == QuestType.Main)
            {
                questObject.FindProperty("order").intValue = GetNextMainOrder(existingQuests);
                questObject.FindProperty("guideTarget").enumValueIndex = (int)GetGuideTarget();
            }
            
            questObject.ApplyModifiedPropertiesWithoutUndo();         //SerializedObject의 변경값을 QuestData에 적용
            string assetPath = $"{folderPath}/Quest_{questId}.asset"; //실제 Asset 저장 경로
            AssetDatabase.CreateAsset(questData, assetPath);          //QuestData를 SO Asset으로 생성
            existingQuests.Add(questData);                            //일괄 생성 시 다음 QuestId / Main Order 계산에 방금 생성한 퀘스트도 포함되도록 목록에 추가

            return questData;
        }

        //현재 존재하는 모든 QuestData 불러오기
        private List<QuestData> LoadExistingQuests()
        {
            List<QuestData> quests = new List<QuestData>();

            string[] guids = AssetDatabase.FindAssets("t:QuestData", new[] { QuestRootPath }); //QuestRootPath 아래의 모든 QuestData 검색

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);                //GUID를 실제 Asset 경로로 변환
                
                QuestData quest = AssetDatabase.LoadAssetAtPath<QuestData>(path); //QuestData 불러오기
                if (quest != null)
                {
                    quests.Add(quest);
                }
            }
            return quests;
        }

        //QuestType에 맞는 중복되지 않는 ID 자동 생성
        private string GenerateQuestId(List<QuestData> quests)
        {
            string prefix = questType.ToString().ToUpper();
            int number = 1;

            while (true)
            {
                
                string questId = $"{prefix}_{number:000}"; //ex) DAILY_001

                //현재 존재하는 QuestId와 중복되는지 확인
                bool isDuplicate = false;
                foreach (QuestData quest in quests)
                {
                    if (quest.QuestId == questId)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                //중복이 아니라면 해당 ID 사용
                if (!isDuplicate)
                {
                    return questId;
                }

                //중복이면 다음 번호 확인
                number++;
            }
        }

        //QuestType에 맞는 저장 폴더 반환
        private string GetQuestFolderPath()
        {
            switch (questType)
            {
                case QuestType.Daily:
                    return $"{QuestRootPath}/Daily";

                case QuestType.Repeat:
                    return $"{QuestRootPath}/Repeat";

                case QuestType.Main:
                    return $"{QuestRootPath}/Main";
            }

            return QuestRootPath;
        }

        //Main 퀘스트의 다음 Order 계산
        private int GetNextMainOrder(List<QuestData> quests)
        {
            int maxOrder = 0;

            //현재 Main 퀘스트 중 가장 높은 Order 찾기
            foreach (QuestData quest in quests)
            {
                if (quest.QuestType == QuestType.Main && quest.Order > maxOrder)
                {
                    maxOrder = quest.Order;
                }
            }
            return maxOrder + 1;
        }

        //QuestType별 저장 폴더 생성
        private void CreateQuestFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return; //이미 폴더가 존재한다면 생성하지 않음
            
            string folderName = System.IO.Path.GetFileName(folderPath); //ex) Assets/.../Quest/Daily -> Daily
            AssetDatabase.CreateFolder(QuestRootPath, folderName);
        }

        #endregion

        #region 퀘스트 자동 설정

        //Main 퀘스트의 완료 조건에 따라 가이드 목적지 자동 설정
        private GuideTarget GetGuideTarget()
        {
            switch (conditionType)
            {
                case QuestConditionType.PartyDeploy:
                    return GuideTarget.Party;

                case QuestConditionType.HeroLevelUp:
                    return GuideTarget.HeroUpgrade;

                case QuestConditionType.HeroSummon:
                    return GuideTarget.HeroSummon;

                case QuestConditionType.StageClear:
                    return GuideTarget.None;
            }

            return GuideTarget.None;
        }

        //조건과 목표 수치를 기준으로 퀘스트 이름 자동 생성
        private string GetQuestName(int target)
        {
            switch (conditionType)
            {
                case QuestConditionType.DailyLogin:
                    return "일일 접속하기";

                case QuestConditionType.HeroSummon:
                    return $"영웅 소환 {target}회";

                case QuestConditionType.HeroLevelUp:
                    return $"영웅 {target}회 레벨업";

                case QuestConditionType.EnemyKill:
                    return $"적 {target}마리 처치";

                case QuestConditionType.StageClear:
                    return $"{targetStageNumber}-{targetSectionNumber} 스테이지 클리어";

                case QuestConditionType.PartyDeploy:
                    return $"파티에 영웅 {target}명 배치";

                case QuestConditionType.DailyQuestRewardClaim:
                    return $"일일 퀘스트 보상 {target}회 받기";
            }

            return "새 퀘스트";
        }

        //조건과 목표 수치를 기준으로 퀘스트 설명 자동 생성
        private string GetQuestDescription(int target)
        {
            switch (conditionType)
            {
                case QuestConditionType.DailyLogin:
                    return "게임에 접속하세요.";

                case QuestConditionType.HeroSummon:
                    return $"영웅을 {target}회 소환하세요.";

                case QuestConditionType.HeroLevelUp:
                    return $"영웅을 {target}회 레벨업하세요.";

                case QuestConditionType.EnemyKill:
                    return $"적을 {target}마리 처치하세요.";

                case QuestConditionType.StageClear:
                    return $"{targetStageNumber}-{targetSectionNumber} 스테이지를 클리어하세요.";

                case QuestConditionType.PartyDeploy:
                    return $"파티에 영웅을 {target}명 배치하세요.";

                case QuestConditionType.DailyQuestRewardClaim:
                    return $"일일 퀘스트 보상을 {target}회 받으세요.";
            }

            return "";
        }

        //조건에 맞는 실제 TargetCount 반환
        private int GetTargetCount(int target)
        {
            switch (conditionType)
            {
                //DailyLogin과 StageClear는 조건 달성 여부만 확인하므로 항상 1
                case QuestConditionType.DailyLogin:
                case QuestConditionType.StageClear:
                    return 1;

                //나머지는 사용자가 입력한 목표 수치 사용
                default:
                    return target;
            }
        }

        #endregion

        #region 조건 / 보상 설정

        //현재 QuestType에서 사용할 수 있는 완료 조건 목록 반환
        private QuestConditionType[] GetAvailableConditions()
        {
            switch (questType)
            {
                //일일 퀘스트
                case QuestType.Daily:
                    return new[]
                    {
                        QuestConditionType.DailyLogin,
                        QuestConditionType.HeroSummon,
                        QuestConditionType.HeroLevelUp,
                        QuestConditionType.EnemyKill,
                        QuestConditionType.DailyQuestRewardClaim
                    };

                //반복 퀘스트
                case QuestType.Repeat:
                    return new[]
                    {
                        QuestConditionType.HeroSummon,
                        QuestConditionType.EnemyKill
                    };

                //메인 퀘스트
                case QuestType.Main:
                    return new[]
                    {
                        QuestConditionType.PartyDeploy,
                        QuestConditionType.StageClear,
                        QuestConditionType.HeroSummon,
                        QuestConditionType.HeroLevelUp
                    };
            }

            return new QuestConditionType[0];
        }

        //조건 이름을 Editor에서 한글로 표시
        private string GetConditionName(QuestConditionType conditionType)
        {
            switch (conditionType)
            {
                case QuestConditionType.DailyLogin:
                    return "일일 접속";

                case QuestConditionType.HeroSummon:
                    return "영웅 소환";

                case QuestConditionType.HeroLevelUp:
                    return "영웅 레벨업";

                case QuestConditionType.EnemyKill:
                    return "적 처치";

                case QuestConditionType.StageClear:
                    return "스테이지 클리어";

                case QuestConditionType.PartyDeploy:
                    return "파티 배치";

                case QuestConditionType.DailyQuestRewardClaim:
                    return "일일 퀘스트 보상 수령";
            }

            return conditionType.ToString();
        }

        //보상 이름을 Editor에서 한글로 표시
        private string GetRewardName(RewardType rewardType)
        {
            switch (rewardType)
            {
                case RewardType.Gold:
                    return "골드";

                case RewardType.Dia:
                    return "다이아";

                case RewardType.FreeTicket:
                    return "소환권";
            }

            return rewardType.ToString();
        }

        //현재 퀘스트가 일괄 생성을 지원하는지 확인
        private bool CanBatchCreate()
        {
            if (questType == QuestType.Main) return false;                    //Main은 순서가 존재하므로 일괄 생성하지 않음
            if (conditionType == QuestConditionType.DailyLogin) return false; //DailyLogin은 목표 수치를 사용하지 않음
            if (conditionType == QuestConditionType.StageClear) return false; //StageClear는 Stage / Section 조합을 사용하므로 일괄 생성하지 않음

            return true;
        }

        #endregion

        #region 유효성 검사

        //현재 입력값으로 퀘스트를 생성할 수 있는지 확인
        private bool IsValidQuest()
        {
            //일괄 생성
            if (createMode == CreateMode.Batch && CanBatchCreate())
            {
                if (batchTargets.Count == 0) return false; //생성할 목표가 하나도 없다면 생성 불가

                foreach (int target in batchTargets)
                {
                    if (target <= 0) //모든 목표 수치는 1 이상이어야 함
                    {
                        return false;
                    }
                }
            }
            //스테이지 클리어
            else if (conditionType == QuestConditionType.StageClear)
            {
                if (targetStageNumber <= 0 || targetSectionNumber <= 0) return false;
            }
            //일일 접속을 제외한 일반 횟수형 퀘스트
            else if (conditionType != QuestConditionType.DailyLogin)
            {
                if (targetCount <= 0) return false;
            }

            //보상은 1 이상이어야 함
            if (rewardAmount <= 0) return false;

            return true;
        }

        //잘못된 입력값이 있다면 경고 메시지 표시
        private void DrawValidationMessage()
        {
            //일괄 생성
            if (createMode == CreateMode.Batch && CanBatchCreate())
            {
                if (batchTargets.Count == 0)
                {
                    EditorGUILayout.HelpBox("생성할 목표를 하나 이상 추가해주세요.", MessageType.Warning);
                    return;
                }

                foreach (int target in batchTargets)
                {
                    if (target <= 0)
                    {
                        EditorGUILayout.HelpBox("모든 목표 수치는 1 이상이어야 합니다.", MessageType.Warning);
                        return;
                    }
                }
            }
            //스테이지 클리어
            else if (conditionType == QuestConditionType.StageClear)
            {
                if (targetStageNumber <= 0 || targetSectionNumber <= 0)
                {
                    EditorGUILayout.HelpBox("스테이지와 구간은 1 이상이어야 합니다.", MessageType.Warning);
                    return;
                }
            }
            //일반 목표 수치
            else if (conditionType != QuestConditionType.DailyLogin && targetCount <= 0)
            {
                EditorGUILayout.HelpBox("목표 수치는 1 이상이어야 합니다.", MessageType.Warning);
                return;
            }

            //보상 수량
            if (rewardAmount <= 0)
            {
                EditorGUILayout.HelpBox("보상 수량은 1 이상이어야 합니다.", MessageType.Warning);
            }
        }

        #endregion
    }
}