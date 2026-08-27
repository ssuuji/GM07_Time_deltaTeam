using AFKHeroPlayerManager = AFKHero.Player.PlayerManager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AFKHero.Quest
{
    public class QuestProgress
    {
        public int currentCount;     //현재 진행도
        public bool isRewardClaimed; //보상 수령 여부
    }

    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        //전체 퀘스트 (Assets/03.Data/Resources/Quest 에 생성되어있는 퀘스트SO)
        private QuestData[] questDataList; 

        //일일 퀘스트      QuestID, 현재 진행도/보상 수령 여부
        private Dictionary<string, QuestProgress> dailyQuestProgress = new Dictionary<string, QuestProgress>();
        private string lastDailyResetDate; //마지막 일일 퀘스트 초기화 날짜

        //반복 퀘스트       QuestID, 현재 진행도
        private Dictionary<string, QuestProgress> repeatQuestProgress = new Dictionary<string, QuestProgress>();

        //메인 퀘스트
        private string currentMainQuestId;    //현재 진행중인 메인퀘스트 ID  
        private int currentMainQuestCount;    //현재 진행중인 메인퀘스트의 진행도
        private bool isMainQuestAllCompleted; //모든 메인퀘스트를 완료했는지 확인여부
        
        public event Action OnQuestChanged;   //퀘스트 상태 변경 이벤트

        private void Awake()
        {
            Instance = this;

            LoadQuestData();           //Resources 에서 모든 퀘스트SO 불러오기
            InitializeQuestProgress(); //초기화
        }

        private void Start()
        {
            AddProgress(QuestConditionType.DailyLogin); //테스트 ( 일일접속 퀘스트 진행도 관련 )
        }

        #region 퀘스트 초기화

        //Resources폴더에 있는 모든 퀘스트SO 불러오기
        private void LoadQuestData()
        {
            questDataList = Resources.LoadAll<QuestData>("Quest"); //Quest/Daily, Quest/Repeat, Quest/Main
        }

        //초기화
        private void InitializeQuestProgress()
        {
            //기존에 들어있던 데이터 제거
            dailyQuestProgress.Clear();
            repeatQuestProgress.Clear();

            //메인 퀘스트 상태도 처음 상태로 초기화
            currentMainQuestId = "";
            currentMainQuestCount = 0;
            isMainQuestAllCompleted = false;

            QuestData firstMainQuest = null; //Order가 가장 낮은 메인 퀘스트

            foreach (QuestData quest in questDataList)
            {
                if (quest == null || !quest.IsEnabled) continue;

                switch (quest.QuestType)
                {
                    //일일
                    case QuestType.Daily:
                        dailyQuestProgress.Add(quest.QuestId, new QuestProgress());
                        break;
                    //반복
                    case QuestType.Repeat:
                        repeatQuestProgress.Add(quest.QuestId, new QuestProgress());
                        break;
                    //메인
                    case QuestType.Main:
                        if (firstMainQuest == null || quest.Order < firstMainQuest.Order)
                        {
                            firstMainQuest = quest;
                        }
                        break;
                }
            }

            //가장 Order가 낮은 Main 퀘스트를 첫 메인 퀘스트로 설정
            if (firstMainQuest != null)
            {
                currentMainQuestId = firstMainQuest.QuestId;
            }
        }

        //일일 퀘스트 초기화 체크
        private void CheckDailyQuestReset()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd"); //오늘 날짜

            if (string.IsNullOrEmpty(lastDailyResetDate))
            {
                lastDailyResetDate = today;
                return;
            }

            if (lastDailyResetDate == today) return;

            ResetDailyQuest();

            lastDailyResetDate = today;
        }

        //일일 퀘스트 초기화
        private void ResetDailyQuest()
        {
            foreach (QuestProgress progress in dailyQuestProgress.Values)
            {
                progress.currentCount = 0;
                progress.isRewardClaimed = false;
            }

            OnQuestChanged?.Invoke(); //퀘스트 UI 갱신
        }

        #endregion

        #region 퀘스트 저장/불러오기

        //퀘스트 저장 데이터 생성
        public QuestSaveData CreateQuestSaveData()
        {
            QuestSaveData saveData = new QuestSaveData();

            saveData.lastDailyResetDate = lastDailyResetDate; //마지막 일일퀘스트 초기화 날짜 저장

            //일일 퀘스트
            foreach (KeyValuePair<string, QuestProgress> quest in dailyQuestProgress)
            {
                QuestProgressSaveData progressData = new QuestProgressSaveData();

                progressData.questId = quest.Key;
                progressData.currentCount = quest.Value.currentCount;
                progressData.isRewardClaimed = quest.Value.isRewardClaimed;

                saveData.dailyQuestSaveData.Add(progressData);
            }

            //반복 퀘스트
            foreach (KeyValuePair<string, QuestProgress> quest in repeatQuestProgress)
            {
                QuestProgressSaveData progressData = new QuestProgressSaveData();

                progressData.questId = quest.Key;
                progressData.currentCount = quest.Value.currentCount;
                progressData.isRewardClaimed = quest.Value.isRewardClaimed;

                saveData.repeatQuestSaveData.Add(progressData);
            }

            //메인 퀘스트
            saveData.mainQuestSaveData.currentQuestId = currentMainQuestId;
            saveData.mainQuestSaveData.currentCount = currentMainQuestCount;
            saveData.mainQuestSaveData.isAllCompleted = isMainQuestAllCompleted;

            return saveData;
        }

        //퀘스트 저장 데이터 불러오기
        public void LoadQuestSaveData(QuestSaveData saveData)
        {
            InitializeQuestProgress(); //먼저 현재 퀘스트데이터 초기화

            if (saveData == null) return;

            lastDailyResetDate = saveData.lastDailyResetDate; //저장된 마지막 일일 초기화 날짜

            //일일 퀘스트
            if (saveData.dailyQuestSaveData != null)
            {
                foreach (QuestProgressSaveData savedQuest in saveData.dailyQuestSaveData)
                {
                    if (!dailyQuestProgress.TryGetValue(savedQuest.questId, out QuestProgress progress)) continue;

                    progress.currentCount = savedQuest.currentCount;
                    progress.isRewardClaimed = savedQuest.isRewardClaimed;
                }
            }
            CheckDailyQuestReset();

            //반복 퀘스트
            if (saveData.repeatQuestSaveData != null)
            {
                foreach (QuestProgressSaveData savedQuest in saveData.repeatQuestSaveData)
                {
                    if (!repeatQuestProgress.TryGetValue(savedQuest.questId, out QuestProgress progress)) continue;

                    progress.currentCount = savedQuest.currentCount;
                    progress.isRewardClaimed = savedQuest.isRewardClaimed;
                }
            }

            //메인 퀘스트
            if (saveData.mainQuestSaveData != null)
            {
                currentMainQuestId = saveData.mainQuestSaveData.currentQuestId;
                currentMainQuestCount = saveData.mainQuestSaveData.currentCount;
                isMainQuestAllCompleted = saveData.mainQuestSaveData.isAllCompleted;
            }
        }

        #endregion

        #region 퀘스트 진행

        //퀘스트 진행도 증가
        public void AddProgress(QuestConditionType conditionType, int amount = 1)
        {
            if (amount <= 0) return;

            //같은행동(conditionType)이 일일 반복 메인에 모두 있을 수 있으니 3개 다 확인
            UpdateDailyQuest(conditionType, amount);  //일일
            UpdateRepeatQuest(conditionType, amount); //반복
            UpdateMainQuest(conditionType, amount);   //메인

            OnQuestChanged?.Invoke(); //진행도 변경 후 UI 갱신
        }

        //일일 퀘스트 진행도 증가
        private void UpdateDailyQuest(QuestConditionType conditionType, int amount)
        {
            foreach (QuestData quest in questDataList)
            {
                if (!quest.IsEnabled) continue;                                                           //비활성화이거나
                if (quest.QuestType != QuestType.Daily) continue;                                         //Daily가 아니거나
                if (quest.ConditionType != conditionType) continue;                                       //현재행동과 조건이 다르거나
                if (!dailyQuestProgress.TryGetValue(quest.QuestId, out QuestProgress progress)) continue; //ID를 찾지 못했거나
                if (progress.isRewardClaimed) continue;                                                   //이미 보상을 받았따면 continue

                progress.currentCount = Mathf.Min(progress.currentCount + amount, quest.TargetCount);     //목표숫자를 넘지않도록 
            }
        }

        //반복 퀘스트 진행도 증가
        private void UpdateRepeatQuest(QuestConditionType conditionType, int amount)
        {
            foreach (QuestData quest in questDataList)
            {
                if (!quest.IsEnabled) continue;                                                              //비활성화이거나
                if (quest.QuestType != QuestType.Repeat) continue;                                           //Repeat가 아니거나
                if (quest.ConditionType != conditionType) continue;                                          //현재행동과 조건이 다르거나
                if (!repeatQuestProgress.TryGetValue(quest.QuestId, out QuestProgress progress)) continue;   //ID를 찾지 못했다면 continue

                progress.currentCount += amount;                                                             //반복은 초과된 숫자도 표시되도록 
            }
        }

        //메인 퀘스트 진행도 증가
        private void UpdateMainQuest(QuestConditionType conditionType, int amount)
        {
            if (isMainQuestAllCompleted) return;                        //메인퀘스트가 모두 끝났다면

            QuestData currentQuest = GetQuestData(currentMainQuestId); //현재 진행중인 퀘스트

            if (currentQuest == null) return;
            if (!currentQuest.IsEnabled) return;
            if (currentQuest.ConditionType != conditionType) return;

            currentMainQuestCount = Mathf.Min(currentMainQuestCount + amount, currentQuest.TargetCount);     
        }

        //스테이지 클리어 퀘스트 (스테이지와 섹션 비교를 위해 별도처리)
        //스테이지 클리어
        public void OnStageClear(int stageNumber, int sectionNumber)
        {
            if (isMainQuestAllCompleted) return;

            QuestData currentQuest = GetQuestData(currentMainQuestId);

            if (currentQuest == null || !currentQuest.IsEnabled) return;
            if (currentQuest.ConditionType != QuestConditionType.StageClear) return;

            //현재 메인 퀘스트의 목표 스테이지가 아니라면 종료
            if (currentQuest.TargetStageNumber != stageNumber) return;
            if (currentQuest.TargetSectionNumber != sectionNumber) return;

            currentMainQuestCount = currentQuest.TargetCount; //메인 퀘스트 완료

            OnQuestChanged?.Invoke(); //퀘스트 UI 갱신
        }

        //다음 메인 퀘스트로 이동
        private void MoveToNextMainQuest(int currentOrder)
        {
            QuestData nextMainQuest = null;

            foreach (QuestData quest in questDataList)
            {
                if (quest == null || !quest.IsEnabled) continue;
                if (quest.QuestType != QuestType.Main) continue;
                if (quest.Order <= currentOrder) continue;

                //현재 퀘스트보다 Order가 높으면서 가장 가까운 퀘스트 찾기
                if (nextMainQuest == null || quest.Order < nextMainQuest.Order)
                {
                    nextMainQuest = quest;
                }
            }

            //다음 메인 퀘스트가 존재
            if (nextMainQuest != null)
            {
                currentMainQuestId = nextMainQuest.QuestId; //다음 메인 퀘스트
                currentMainQuestCount = 0;                  //진행도 초기화

                CheckCurrentMainQuestProgress();            //이미 달성한 조건인지 확인

                OnQuestChanged?.Invoke();                   //변경된 메인 퀘스트 UI 갱신
                return;
            }

            //다음 퀘스트가 없다면 메인 퀘스트 전체 완료
            currentMainQuestId = "";
            currentMainQuestCount = 0;
            isMainQuestAllCompleted = true;
        }

        //현재 메인 퀘스트가 이미 달성된 조건인지 확인
        private void CheckCurrentMainQuestProgress()
        {
            if (isMainQuestAllCompleted) return;

            QuestData currentQuest = GetQuestData(currentMainQuestId);

            if (currentQuest == null || !currentQuest.IsEnabled) return;

            switch (currentQuest.ConditionType)
            {
                case QuestConditionType.StageClear:
                    if (StageManager.Instance == null) return;

                    int lastStage = StageManager.Instance.LastStageNumber;
                    int lastSection = StageManager.Instance.LastSectionNumber;

                    bool isCleared =
                        lastStage > currentQuest.TargetStageNumber ||
                        (lastStage == currentQuest.TargetStageNumber &&
                         lastSection >= currentQuest.TargetSectionNumber);

                    if (isCleared)
                    {
                        currentMainQuestCount = currentQuest.TargetCount; //이미 클리어했다면 완료 처리
                    }

                    break;
            }
        }

        #endregion

        #region 퀘스트 조회

        //ID로 퀘스트 데이터 찾기
        private QuestData GetQuestData(string questId)
        {
            foreach (QuestData quest in questDataList)
            {
                if (quest.QuestId == questId)
                {
                    return quest;
                }
            }

            return null;
        }

        //타입에 맞는 퀘스트 목록 반환 : 일일/반복 
        public List<QuestData> GetQuestList(QuestType questType)
        {
            List<QuestData> quests = new List<QuestData>();

            foreach (QuestData quest in questDataList)
            {
                if (quest == null || !quest.IsEnabled) continue;
                if (quest.QuestType != questType) continue;

                quests.Add(quest);
            }

            return quests;
        }

        //현재 진행중인 메인 퀘스트 반환
        public QuestData GetCurrentMainQuest()
        {
            if (isMainQuestAllCompleted) return null;

            return GetQuestData(currentMainQuestId);
        }

        //현재 진행도 반환 (slider, 텍스트 갱신)
        public int GetCurrentCount(QuestData questData)
        {
            if (questData == null) return 0;

            switch (questData.QuestType)
            {
                //일일
                case QuestType.Daily:
                    if (dailyQuestProgress.TryGetValue(questData.QuestId, out QuestProgress dailyProgress))
                    {
                        return dailyProgress.currentCount;
                    }
                    break;
                //반복
                case QuestType.Repeat:
                    if (repeatQuestProgress.TryGetValue(questData.QuestId, out QuestProgress repeatProgress))
                    {
                        return repeatProgress.currentCount;
                    }
                    break;
                //메인
                case QuestType.Main:
                    if (currentMainQuestId == questData.QuestId)
                    {
                        return currentMainQuestCount;
                    }
                    break;
            }

            return 0;
        }

        //일일퀘스트 완료 여부
        public bool IsQuestCompleted(QuestData questData)
        {
            if (questData == null) return false;

            if (questData.QuestType == QuestType.Daily)
            {
                if (!dailyQuestProgress.TryGetValue(questData.QuestId, out QuestProgress progress)) return false;

                return progress.isRewardClaimed; //일일퀘스트의 경우는 보상수령까지 모두 완료해야 완료상태로 반환
            }

            return false;
        }

        //퀘스트 보상 수령 가능 여부
        public bool CanClaimReward(QuestData questData)
        {
            if (questData == null) return false;

            switch (questData.QuestType)
            {
                case QuestType.Daily:
                    if (!dailyQuestProgress.TryGetValue(questData.QuestId, out QuestProgress dailyProgress)) return false;

                    return dailyProgress.currentCount >= questData.TargetCount &&
                           !dailyProgress.isRewardClaimed;

                case QuestType.Repeat:
                    if (!repeatQuestProgress.TryGetValue(questData.QuestId, out QuestProgress repeatProgress)) return false;

                    return repeatProgress.currentCount >= questData.TargetCount;

                case QuestType.Main:
                    if (isMainQuestAllCompleted) return false;
                    if (currentMainQuestId != questData.QuestId) return false;

                    return currentMainQuestCount >= questData.TargetCount;
            }

            return false;
        }

        #endregion

        #region 퀘스트 보상

        //일일 / 반복 퀘스트 보상 수령
        public void ClaimReward(QuestData questData)
        {
            if (!CanClaimReward(questData)) return; //보상 받을 수 있는 상태인지 확인

            switch (questData.QuestType)
            {
                //일일
                case QuestType.Daily:
                    ClaimDailyReward(questData);
                    break;
                //반복
                case QuestType.Repeat:
                    ClaimRepeatReward(questData);
                    break;
                //메인
                case QuestType.Main:
                    ClaimMainQuestReward(questData);
                    break;
            }

            OnQuestChanged?.Invoke();  //퀘스트 변경 이벤트
        }

        //퀘스트 보상 지급
        private void GiveQuestReward(QuestData questData)
        {
            if (AFKHeroPlayerManager.Instance == null) return;

            foreach (QuestReward reward in questData.Rewards)
            {
                switch (reward.RewardType)
                {
                    case RewardType.Gold:
                        AFKHeroPlayerManager.Instance.AddGold(reward.Amount);
                        break;

                    case RewardType.Dia:
                        AFKHeroPlayerManager.Instance.AddDia(reward.Amount);
                        break;

                    case RewardType.FreeTicket:
                        AFKHeroPlayerManager.Instance.AddFreeTicket(reward.Amount);
                        break;
                }
            }
        }

        //일일 퀘스트 보상 수령
        private void ClaimDailyReward(QuestData questData)
        {
            if (!dailyQuestProgress.TryGetValue(questData.QuestId, out QuestProgress progress)) return;
            if (progress.currentCount < questData.TargetCount) return;
            if (progress.isRewardClaimed) return;

            GiveQuestReward(questData);

            progress.isRewardClaimed = true;

        }

        //반복 퀘스트 보상 수령
        private void ClaimRepeatReward(QuestData questData)
        {
            if (!repeatQuestProgress.TryGetValue(questData.QuestId, out QuestProgress progress)) return;
            if (progress.currentCount < questData.TargetCount) return;

            GiveQuestReward(questData);
            
            progress.currentCount -= questData.TargetCount; //목표 수치만큼 차감하고 초과 진행도 유지
        }

        //메인 퀘스트 보상 수령
        private void ClaimMainQuestReward(QuestData questData)
        {
            GiveQuestReward(questData); //현재 메인 퀘스트 보상 지급
            MoveToNextMainQuest(questData.Order); //다음 메인 퀘스트로 이동
        }

        //현재 타입에서 받을 수 있는 퀘스트 보상 모두 수령
        public void ClaimAllRewards(QuestType questType)
        {
            List<QuestData> quests = GetQuestList(questType);

            foreach (QuestData quest in quests)
            {
                if (!CanClaimReward(quest)) continue;

                switch (quest.QuestType)
                {
                    case QuestType.Daily:
                        ClaimDailyReward(quest);
                        break;

                    case QuestType.Repeat:
                        ClaimRepeatReward(quest);
                        break;
                }
            }

            OnQuestChanged?.Invoke(); //모든 보상 수령 후 UI 갱신
        }


        #endregion
    }
}