using System.Linq;
using AFKHero.Quest;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class UIQuestManager : MonoBehaviour
    {
        [Header("퀘스트 창")]
        [SerializeField] private GameObject questPanel; //퀘스트 전체 패널
        [SerializeField] private Button backButton;     //퀘스트창 닫기

        [Header("퀘스트 탭")]
        [SerializeField] private RectTransform selector;                             //현재 선택된 탭 표시 이미지
        [SerializeField] private TMP_Text dailyText;                                 //일일탭 텍스트
        [SerializeField] private TMP_Text repeatText;                                //반복탭 텍스트
        private const float DailySelectorX = -165f;                                  //일일  탭 선택시 셀렉터 X위치
        private const float RepeatSelectorX = 165f;                                  //반복 탭 선택시 셀렉터 X위치
        private readonly Color selectedTextColor = new Color32(74, 40, 26, 255);     //선택
        private readonly Color unselectedTextColor = new Color32(114, 87, 71, 255);  //미선택
        private QuestType currentQuestType = QuestType.Daily;                        //현재 선택되어있는 퀘스트 탭

        [Header("퀘스트 목록")]
        [SerializeField] private Transform content;           //퀘스트목록 content
        [SerializeField] private UIQuestSlot questSlotPrefab; //퀘스트 슬롯 프리펩


        private void Start()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestChanged += RefreshQuestList; //퀘스트 상태 변경 이벤트
            }

        }

        private void OnDestroy()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestChanged -= RefreshQuestList;
            }

        }

        #region 퀘스트 목록

        //현재 열려있는 퀘스트 목록 갱신
        private void RefreshQuestList()
        {
            if (!questPanel.activeSelf) return;

            ShowQuestList(currentQuestType);
        }

        //선택한 타입의 퀘스트 목록 표시
        private void ShowQuestList(QuestType questType)
        {
            
            ClearQuestList(); //기존에 생성되어 있던 퀘스트 슬롯 제거

            if (QuestManager.Instance == null) return;

            List<QuestData> quests = QuestManager.Instance.GetQuestList(questType);                   //퀘스트 목록 가져오기
            quests = quests.OrderBy(quest => QuestManager.Instance.IsQuestCompleted(quest)).ToList(); //완료된 퀘스트를 목록 가장 아래로 정렬(미완료->완료 순서)

            foreach (QuestData quest in quests)
            {
                UIQuestSlot questSlot = Instantiate(questSlotPrefab, content);   //퀘스트 슬롯 생성
                int currentCount = QuestManager.Instance.GetCurrentCount(quest); //현재 퀘스트 진행도 가져오기
                questSlot.SetQuest(quest, currentCount);                         //퀘스트 데이터와 진행도를 슬롯 UI에 적용
            }
        }

        //현재 생성되어 있는 퀘스트 슬롯 제거
        private void ClearQuestList()
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }
        }

        #endregion

        #region 퀘스트 창 열기/닫기

        //퀘스트창 열기버튼
        public void OnClickedOpenQuest()
        {
            questPanel.SetActive(true);
            backButton.gameObject.SetActive(true);

            ChangeQuestTab(QuestType.Daily, false); //일일탭을 우선 보여줌
        }

        //퀘스트창 닫기버튼
        public void OnClickedCloseQuest()
        {
            questPanel.SetActive(false);
            backButton.gameObject.SetActive(false);
        }

        #endregion

        #region 퀘스트 탭

        //일일 퀘스트 탭 클릭
        public void OnClickedDailyQuest()
        {
            ChangeQuestTab(QuestType.Daily, true);
        }

        //반복 퀘스트 탭 클릭
        public void OnClickedRepeatQuest()
        {
            ChangeQuestTab(QuestType.Repeat, true);
        }

        //선택한 퀘스트 탭으로 변경
        private void ChangeQuestTab(QuestType questType, bool isAnimation)
        {
            currentQuestType = questType; //현재 선택된 탭 저장
            ShowQuestList(questType); //선택한 타입의 퀘스트 목록 표시
            UpdateTabTextColor(); //선택된 탭에 맞게 글씨 색상 변경
            float targetX = questType == QuestType.Daily ? DailySelectorX : RepeatSelectorX; //선택한 탭에 맞는 Selector 위치 결정

            //탭 이동할때 셀렉터가 좌우로 움직이게끔 보여주고 현재 어떤탭을 눌렀는지 확실하게 표시해줌
            if (isAnimation)
            {
                selector.DOKill();                                           //기존에 실행 중인 Selector Tween이 있다면 중지
                selector.DOAnchorPosX(targetX, 0.15f).SetEase(Ease.OutQuad); //현재 위치에서 목표 X 위치까지 이동
            }
            else
            {
                selector.DOKill();
                selector.anchoredPosition = new Vector2(targetX, selector.anchoredPosition.y); //두트윈없이 바로 일일탭 보여주기
            }
        }

        //현재 선택된 탭에 맞게 텍스트 색상 변경
        private void UpdateTabTextColor()
        {
            dailyText.color = currentQuestType == QuestType.Daily ? selectedTextColor : unselectedTextColor;
            repeatText.color = currentQuestType == QuestType.Repeat ? selectedTextColor : unselectedTextColor;
        }
        #endregion
    }
}