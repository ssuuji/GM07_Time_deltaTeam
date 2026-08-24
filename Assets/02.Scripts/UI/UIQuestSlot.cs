using AFKHero.Quest;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class UIQuestSlot : MonoBehaviour
    {
        [Header("퀘스트")]
        [SerializeField] private TMP_Text questText;       //퀘스트 이름
        [SerializeField] private Slider progressSlider;    //퀘스트 진행도
        [SerializeField] private TMP_Text progressText;    //현재 진행도 / 목표 진행도

        [Header("보상")]
        [SerializeField] private Image rewardImage;         //보상 아이콘
        [SerializeField] private TMP_Text rewardAmountText; //보상 수량

        [Header("보상 아이콘")]
        [SerializeField] private Sprite goldSprite;         //골드
        [SerializeField] private Sprite diaSprite;          //다이아
        [SerializeField] private Sprite freeTicketSprite;   //무료뽑기권

        [Header("보상 수령")]
        [SerializeField] private Button getButton;          //보상 받기 버튼

        [Header("완료 표시")]
        [SerializeField] private GameObject completeImage; //보상 수령 완료 표시

        private QuestData questData; //현재 슬롯에 표시 중인 퀘스트 데이터

        //실제 게임에서 사용할 퀘스트 슬롯 설정
        public void SetQuest(QuestData questData, int currentCount)
        {
            bool canClaim = QuestManager.Instance != null && QuestManager.Instance.CanClaimReward(questData);      //현재 퀘스트가 보상 수령 가능한 상태인지 확인
            bool isCompleted = QuestManager.Instance != null && QuestManager.Instance.IsQuestCompleted(questData); //현재 퀘스트가 완료 상태인지 확인

            SetQuestView(questData, currentCount, canClaim, isCompleted);
        }

        //퀘스트 데이터 UI에 적용
        private void SetQuestView(QuestData questData, int currentCount, bool canClaim, bool isCompleted)
        {
            this.questData = questData;                                                 //보상 버튼 클릭 시 사용할 현재 퀘스트 데이터 저장.
            UpdateReward();                                                             //보상 아이콘 / 수량 표시

            questText.text = questData.QuestName;                                       //퀘스트 이름

            progressSlider.maxValue = questData.TargetCount;                            //진행도 
            progressSlider.value = Mathf.Clamp(currentCount, 0, questData.TargetCount); //진행도 값
            progressText.text = $"{currentCount} / {questData.TargetCount}";            //진행도 텍스트

            getButton.interactable = canClaim && !isCompleted;                          //보상 수령 가능 상태이고 완료 상태가 아닐 때만 받기버튼 활성화
            
            completeImage.SetActive(isCompleted);                                       //일일 퀘스트 보상 수령 완료 시 완료 이미지 표시
        }

        //Quest Editor - 미리보기에서 사용할 슬롯 설정
        public void SetPreview(QuestData questData, int currentCount, bool isCompleted)
        {
            bool canClaim = !isCompleted && currentCount >= questData.TargetCount; //완료 상태가 아니면서 현재 진행도가 목표 이상이면 보상 수령 가능

            SetQuestView(questData, currentCount, canClaim, isCompleted);
        }

        //퀘스트 보상 정보 표시
        private void UpdateReward()
        {
            if (questData.Rewards == null || questData.Rewards.Count == 0) return;

            QuestReward reward = questData.Rewards[0];        //현재는 첫 번째 보상만 표시,,
            rewardAmountText.text = reward.Amount.ToString(); //보상 수량 표시

            //보상 아이콘 적용
            switch (reward.RewardType)
            {
                //골드
                case RewardType.Gold:
                    rewardImage.sprite = goldSprite;
                    break;
                //다이아
                case RewardType.Dia:
                    rewardImage.sprite = diaSprite;
                    break;
                //무료뽑기권
                case RewardType.FreeTicket:
                    rewardImage.sprite = freeTicketSprite;
                    break;
            }
        }

        //보상 받기 버튼
        public void OnClickedGetReward()
        {
            if (questData == null || QuestManager.Instance == null) return;

            QuestManager.Instance.ClaimReward(questData); //보상 수령요청
        }
    }
}