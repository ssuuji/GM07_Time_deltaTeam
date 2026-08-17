using AFKHero.Battle;
using TMPro;
using UnityEngine;

namespace AFKHero.UI
{
    //전투탭 UI 매니저
    public class UIBattleManager : MonoBehaviour
    {
        public static UIBattleManager Instance { get; private set; }

        [Header("하단 영웅 UI")]
        [SerializeField] private Transform heroSlotTransform;           //영웅슬롯 생성위치
        [SerializeField] private UIBattleHeroSlot heroSlotPrefab;       //영웅슬롯 프리펩

        [Header("궁극기 모드")]
        [SerializeField] private BattleManager battleManager;       //전투 매니저
        [SerializeField] private TMP_Text ultimateModeText;         //AUTO 표시

        [Header("현재 스테이지")]                                        
        [SerializeField] private TMP_Text currentStageText;             //현재 스테이지 표시

        [Header("전투 보상")]
        [SerializeField] private GameObject rewardGoldPanel;            //골드 보상 패널
        [SerializeField] private GameObject rewardDiaPanel;             //다이아 보상 패널
        [SerializeField] private GameObject rewardTicketPanel;          //무료 뽑기권 보상 패널
        [SerializeField] private TMP_Text rewardGoldText;               //골드 보상
        [SerializeField] private TMP_Text rewardDiaText;                //다이아 보상
        [SerializeField] private TMP_Text rewardTicketText;             //무료 뽑기권 보상

        private UIBattleHeroSlot[] heroSlots = new UIBattleHeroSlot[5]; //영웅슬롯
        private readonly Color32 autoYellow = new Color32(255, 220, 60, 255);    //AUTO 색상
        private readonly Color32 ManualWhite = new Color32(255, 255, 255, 255);  //Manual 색상

        private void Awake()
        {
            Instance = this;

            CreateHeroSlots();
        }

        private void Start()
        {
            if (battleManager == null) return;

            battleManager.UltimateUseModeChanged += UpdateUltimateModeUI;
            
            UpdateUltimateModeUI(battleManager.UltimateMode); //현재 설정으로 UI 갱신
        }

        private void OnDestroy()
        {
            if (battleManager != null)
            {
                battleManager.UltimateUseModeChanged -= UpdateUltimateModeUI;
            }
        }

        //전투탭 UI 갱신
        public void UpdateBattleUI()
        {
            UpdateStageUI(); //현재 스테이지 갱신
            UpdatePartyUI(); //현재 파티 갱신
        }

        //현재 진행 스테이지 UI 갱신
        public void UpdateStageUI()
        {
            if (StageManager.Instance == null) return;
            if (currentStageText == null) return;

            currentStageText.text = $"STAGE {StageManager.Instance.CurrentStageNumber}-{StageManager.Instance.CurrentSectionNumber}";
        }


        //하단 영웅 슬롯 생성
        private void CreateHeroSlots()
        {
            for (int i = 0; i < heroSlots.Length; i++)
            {
                heroSlots[i] = Instantiate(heroSlotPrefab, heroSlotTransform);
            }
        }

        //현재 파티 기준으로 하단 영웅 UI 갱신
        public void UpdatePartyUI()
        {
            if (PartyManager.Instance == null) return;

            for (int i = 0; i < heroSlots.Length; i++)
            {
                HeroInstance hero = PartyManager.Instance.partySlots[i]; //현재 저장된 파티자리 가져오기
                
                if (hero == null || hero.data == null) 
                {
                    heroSlots[i].Hide();                                 //빈 파티 자리는 숨기기
                    continue;
                }

                heroSlots[i].SetHero(hero);                              //배치된 영웅 표시
            }
        }

        //전투 유닛연결
        public void SetBattleUnit(int slotIndex, BattleUnit unit)
        {
            if (unit == null) return;
            if (unit.Team != TeamType.Ally) return;

            heroSlots[slotIndex].SetBattleUnit(unit);
        }

        //승리 보상 UI 갱신
        public void UpdateRewardUI(StageInfo stageInfo)
        {
            if (stageInfo == null) return;

            //골드
            bool hasGold = stageInfo.ClearGold > 0;
            rewardGoldPanel.SetActive(hasGold);
            if (hasGold) rewardGoldText.text = $"+ {stageInfo.ClearGold}";

            //다이아
            bool hasDia = stageInfo.ClearDia > 0;
            rewardDiaPanel.SetActive(hasDia);
            if (hasDia) rewardDiaText.text = $"+ {stageInfo.ClearDia}";

            //무료 뽑기권
            bool hasTicket = stageInfo.ClearTicket > 0;
            rewardTicketPanel.SetActive(hasTicket);
            if (hasTicket) rewardTicketText.text = $"+ {stageInfo.ClearTicket}";
        }

        //궁극기 자동 / 수동 변경 버튼
        public void OnClickedUltimateMode()
        {
            if (battleManager == null) return;

            battleManager.ToggleUltimateUseMode();
        }

        //궁극기 모드 UI 갱신
        private void UpdateUltimateModeUI(UltimateUseMode mode)
        {
            if (ultimateModeText == null) return;

            bool isAuto = mode == UltimateUseMode.Auto;
            ultimateModeText.color = isAuto ? autoYellow : ManualWhite;
        }
    }

}
