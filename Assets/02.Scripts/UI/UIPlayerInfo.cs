using AFKHeroPlayerManager = AFKHero.Player.PlayerManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class UIPlayerInfo : MonoBehaviour
    {
        //화면 상단 플레이어 정보 표시 UI
        [SerializeField] private AFKHeroPlayerManager playerManager;

        [Header("Profile")]
        [SerializeField] private Image playerIconImage;   //아이콘
        [SerializeField] private TMP_Text playerNameText; //이름

        [Header("Currency")]
        [SerializeField] private TMP_Text goldText;       //골드
        [SerializeField] private TMP_Text diaText;        //다이아
        [SerializeField] private TMP_Text freeTicket;     //무료뽑기권

        [Header("Power")]
        [SerializeField] private TMP_Text partyPowerText; //파티 공격력

        private void OnEnable()
        {
            if (playerManager == null) return;

            playerManager.OnPlayerInfoChanged += UpdatePlayerUI; //플레이어 정보변경 이벤트 구독
        }

        private void Start()
        {
            UpdatePlayerUI(); //플레이어 정보 갱신
        }

        private void OnDisable()
        {
            if (playerManager == null) return;

            //구독해제
            playerManager.OnPlayerInfoChanged -= UpdatePlayerUI;
        }

        //플레이어 정보 갱신
        public void UpdatePlayerUI()
        {
            if (playerManager == null) return;

            playerIconImage.sprite = playerManager.PlayerIcon;         //아이콘
            playerNameText.text = playerManager.PlayerName;            //이름
            partyPowerText.text = playerManager.PartyPower.ToString(); //파티공격력
            goldText.text = playerManager.Gold.ToString();             //골드
            diaText.text = playerManager.Dia.ToString();               //다이아
            freeTicket.text = playerManager.FreeTicket.ToString();     //무료뽑기권 ( 이건 상점쪽에 반영 )
        }
    }
}

