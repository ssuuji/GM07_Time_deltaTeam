using AFKHero.Save;
using TMPro;
using UnityEngine;

namespace AFKHero.UI
{
    public class UIOfflineReward : MonoBehaviour
    {
        public static UIOfflineReward Instance { get; private set; }

        [Header("방치 보상")]
        [SerializeField] private GameObject rewardPanel; //방치 보상 패널
        [SerializeField] private TMP_Text offlineTimeText; //방치 시간
        [SerializeField] private TMP_Text rewardGoldText; //방치 골드
        [SerializeField] private GameObject backImg;

        private void Awake()
        {
            Instance = this;

            rewardPanel.SetActive(false);
        }

        //방치 보상 팝업 표시
        public void ShowOfflineReward()
        {
            int offlineMinutes = OfflineRewardManager.Instance.OfflineMinutes;
            int rewardGold = OfflineRewardManager.Instance.RewardGold;

            if (rewardGold <= 0) return;

            int hour = offlineMinutes / 60;
            int minute = offlineMinutes % 60;

            offlineTimeText.text = $"{hour}시간 {minute}분";
            rewardGoldText.text = $"{rewardGold:N0}";         //:N0  30000 -> 30,000

            rewardPanel.SetActive(true);
            backImg.SetActive(true);
        }

        //방치 보상 수령
        public void OnClickedClaimReward()
        {
            OfflineRewardManager.Instance.ClaimReward();

            rewardPanel.SetActive(false);
            backImg.SetActive(false);
        }
    }
}