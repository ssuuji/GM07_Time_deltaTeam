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
        [SerializeField] private Transform heroSlotTransform;
        [SerializeField] private UIBattleHeroSlot heroSlotPrefab;

        [Header("궁극기 모드")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private TMP_Text ultimateModeText;

        [Header("현재 스테이지")]
        [SerializeField] private TMP_Text currentStageText;

        [Header("전투 보상")]
        [SerializeField] private GameObject rewardGoldPanel;
        [SerializeField] private GameObject rewardDiaPanel;
        [SerializeField] private GameObject rewardTicketPanel;
        [SerializeField] private TMP_Text rewardGoldText;
        [SerializeField] private TMP_Text rewardDiaText;
        [SerializeField] private TMP_Text rewardTicketText;

        [Header("전투 보상 (장비)")]
        [SerializeField] private GameObject rewardEquipPanel;
        [SerializeField] private TMP_Text rewardEquipText;

        private UIBattleHeroSlot[] heroSlots = new UIBattleHeroSlot[5];
        private readonly Color32 autoYellow = new Color32(255, 220, 60, 255);
        private readonly Color32 ManualWhite = new Color32(255, 255, 255, 255);

        // 추가된 부분: 이번 판에 장비가 떨어졌는지 기억
        private bool isEquipDroppedThisStage = false;

        private void Awake()
        {
            Instance = this;
            CreateHeroSlots();
        }

        private void Start()
        {
            if (battleManager == null) return;
            battleManager.UltimateUseModeChanged += UpdateUltimateModeUI;
            UpdateUltimateModeUI(battleManager.UltimateMode);
        }

        private void OnDestroy()
        {
            if (battleManager != null)
            {
                battleManager.UltimateUseModeChanged -= UpdateUltimateModeUI;
            }
        }

        public void UpdateBattleUI()
        {
            UpdateStageUI();
            UpdatePartyUI();
        }

        public void UpdateStageUI()
        {
            if (StageManager.Instance == null) return;
            if (currentStageText == null) return;

            currentStageText.text = $"STAGE {StageManager.Instance.CurrentStageNumber}-{StageManager.Instance.CurrentSectionNumber}";
        }

        private void CreateHeroSlots()
        {
            for (int i = 0; i < heroSlots.Length; i++)
            {
                heroSlots[i] = Instantiate(heroSlotPrefab, heroSlotTransform);
            }
        }

        public void UpdatePartyUI()
        {
            if (PartyManager.Instance == null) return;

            for (int i = 0; i < heroSlots.Length; i++)
            {
                HeroInstance hero = PartyManager.Instance.partySlots[i];
                if (hero == null || hero.data == null)
                {
                    heroSlots[i].Hide();
                    continue;
                }
                heroSlots[i].SetHero(hero);
            }
        }

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

            bool hasGold = stageInfo.ClearGold > 0;
            rewardGoldPanel.SetActive(hasGold);
            if (hasGold) rewardGoldText.text = $"+ {stageInfo.ClearGold}";

            bool hasDia = stageInfo.ClearDia > 0;
            rewardDiaPanel.SetActive(hasDia);
            if (hasDia) rewardDiaText.text = $"+ {stageInfo.ClearDia}";

            bool hasTicket = stageInfo.ClearTicket > 0;
            rewardTicketPanel.SetActive(hasTicket);
            if (hasTicket) rewardTicketText.text = $"+ {stageInfo.ClearTicket}";

            if (rewardEquipPanel != null)
            {
                rewardEquipPanel.SetActive(isEquipDroppedThisStage);
            }

            // 다음 판을 위해 다시 초기화
            isEquipDroppedThisStage = false;
        }

        //장비 드롭 시 호출
        public void ShowDroppedEquipmentUI(EquipmentData equip)
        {
            if (equip == null || rewardEquipPanel == null) return;

            isEquipDroppedThisStage = true; // 장비 당첨 기록
            rewardEquipPanel.SetActive(true);

            if (rewardEquipText != null)
            {
                rewardEquipText.text = $"+ {equip.equipmentName}";
            }
        }

        public void OnClickedUltimateMode()
        {
            if (battleManager == null) return;
            battleManager.ToggleUltimateUseMode();
        }

        private void UpdateUltimateModeUI(UltimateUseMode mode)
        {
            if (ultimateModeText == null) return;

            bool isAuto = mode == UltimateUseMode.Auto;
            ultimateModeText.color = isAuto ? autoYellow : ManualWhite;
        }
    }
}