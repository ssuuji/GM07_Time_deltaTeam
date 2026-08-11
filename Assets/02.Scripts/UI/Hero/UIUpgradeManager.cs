using AFKHeroPlayerManager = AFKHero.Player.PlayerManager;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class UIUpgradeManager : MonoBehaviour
    {
        public static UIUpgradeManager Instance { get; private set; }

        [Header("영웅 리스트")]
        [SerializeField] private Transform upgradeContent; //viewport 에 있는 content 연결
        [SerializeField] private UIHeroList heroList;

        [Header("선택 영웅")]
        [SerializeField] private Image heroImage;
        [SerializeField] private TMP_Text heroLevelText;
        [SerializeField] private TMP_Text heroAttackText;
        [SerializeField] private TMP_Text heroDefenseText;

        [Header("레벨업")]
        [SerializeField] private TMP_Text levelUpCostText;     //레벨업 비용

        [Header("영웅 합성 팝업")]
        [SerializeField] private GameObject backButton;         //뒤로가기 버튼 : 배경클릭
        [SerializeField] private GameObject upgradePopup;       //영웅합성 팝업
        [SerializeField] private Transform materialContent;     //영웅합성 팝업 Content

        [SerializeField] private Image upgradeHeroBefore;       //승급전 영웅
        [SerializeField] private Image upgradeGradeBefore;      //승급전 배경
        [SerializeField] private Image upgradeHeroAfter;        //승급후 영웅
        [SerializeField] private Image upgradeGradeAfter;       //승급후 배경

        [SerializeField] private Image[] materialImages;        //선택한 재료 3칸
        [SerializeField] private TMP_Text materialCountText;    //0 / 3 텍스트 표시
        private int requiredMaterialCount = 3;                  //필요한 재료 수
        private readonly List<HeroInstance> selectedMaterials = new List<HeroInstance>();

        private HeroInstance selectedHero; //선택한 영웅

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            UpdateHeroList();
        }

        //선택 영웅 UI 표시
        private void SetSelectedHeroUI(bool active)
        {
            heroImage.gameObject.SetActive(active);
            heroLevelText.gameObject.SetActive(active);
            heroAttackText.gameObject.SetActive(active);
            heroDefenseText.gameObject.SetActive(active);
        }

        #region 영웅 리스트
        //영웅 리스트 갱신
        public void UpdateHeroList()
        {
            if (heroList == null) return;
            if (upgradeContent == null) return;

            heroList.UpdateList(upgradeContent, UIHeroSlotType.All, null, UIHeroSlotMode.Upgrade);
        }

        //영웅 선택
        public void SelectHero(HeroInstance hero)
        {
            if (hero == null || hero.data == null) return;

            selectedHero = hero;

            levelUpCostText.color = new Color32(219, 216, 77, 255); //기본색(노란색)
            SetSelectedHeroUI(true);
            UpdateSelectedHero();
        }

        //선택한 영웅 UI 갱신
        private void UpdateSelectedHero()
        {
            if (selectedHero == null || selectedHero.data == null) return;

            heroImage.sprite = selectedHero.data.HeroIcon;
            heroLevelText.text = $"LV. {selectedHero.level}";
            heroAttackText.text = selectedHero.Attack.ToString();
            heroDefenseText.text = selectedHero.Defense.ToString();

            UpdateLevelUpCost();
        }
        #endregion

        #region 레벨업
        //레벨업 비용 UI 갱신
        private void UpdateLevelUpCost()
        {
            if (selectedHero == null) return;

            //최고 레벨
            if (selectedHero.level >= 50)
            {
                levelUpCostText.text = "MAX";
                levelUpCostText.color = new Color32(219, 216, 77, 255);
                return;
            }

            int cost = selectedHero.LevelUpCost;

            levelUpCostText.text = cost.ToString();

            //현재 골드로 다음 레벨업이 가능한지 확인
            if (AFKHeroPlayerManager.Instance.Gold < cost)
            {
                levelUpCostText.color =
                    new Color32(224, 90, 90, 255); //부족
            }
            else
            {
                levelUpCostText.color =
                    new Color32(219, 216, 77, 255); //충분
            }
        }

        //레벨업 버튼
        public void OnClickedLevelUp()
        {
            if (selectedHero == null || selectedHero.data == null) return;

            HeroManager.Instance.LevelUpHero(selectedHero.data.HeroID);
            UpdateSelectedHero();
        }
        #endregion

        #region 영웅 합성

        //영웅 합성 버튼
        public void OnClickedUpgrade()
        {
            if (selectedHero == null || selectedHero.data == null) return;

            switch (selectedHero.data.HeroGrade)
            {
                //등급 → 등급+      : 같은등급 표시
                case HeroGrade.Normal:
                case HeroGrade.Rare:
                case HeroGrade.Epic:
                    heroList.UpdateList(materialContent, UIHeroSlotType.SameGrade, selectedHero, UIHeroSlotMode.UpgradeMaterial);
                    break;

                //등급+ → 다음 등급 : 같은영웅 표시
                case HeroGrade.NormalPlus:
                case HeroGrade.RarePlus:
                    heroList.UpdateList(materialContent, UIHeroSlotType.SameHero, selectedHero, UIHeroSlotMode.UpgradeMaterial);
                    break;

                //최고등급 (에픽+)
                case HeroGrade.EpicPlus:
                    UINoticePopup.Instance.ShowTime("현재 등급은 최고 등급 입니다.", 3f);
                    return;
            }

            ResetUpgradePopup();

            backButton.SetActive(true);
            upgradePopup.SetActive(true);
        }

        //영웅 합성 팝업 닫기
        public void OnClickedCloseUpgradePopup()
        {
            backButton.SetActive(false);
            upgradePopup.SetActive(false);
        }

        //합성창 초기화
        private void ResetUpgradePopup()
        {
            selectedMaterials.Clear();

            HeroGrade currentGrade = selectedHero.data.HeroGrade;
            HeroGrade nextGrade = GetNextGrade(currentGrade);

            //현재 영웅
            upgradeHeroBefore.sprite = selectedHero.data.HeroIcon;
            upgradeGradeBefore.color = HeroGradeColor.GetColor(currentGrade);

            //승급 후 영웅
            upgradeHeroAfter.sprite = selectedHero.data.HeroIcon;
            upgradeGradeAfter.color = HeroGradeColor.GetColor(nextGrade);

            //선택 재료 초기화
            for (int i = 0; i < materialImages.Length; i++)
            {
                materialImages[i].gameObject.SetActive(false);
            }

            UpdateMaterialUI();
        }

        //선택한 합성 재료 UI 갱신
        private void UpdateMaterialUI()
        {
            string materialType;

            switch (selectedHero.data.HeroGrade)
            {
                case HeroGrade.Normal:
                case HeroGrade.Rare:
                case HeroGrade.Epic:
                    materialType = "동일 등급 재료";
                    break;

                default:
                    materialType = "동일 영웅 재료";
                    break;
            }

            materialCountText.text = $"{materialType} ({selectedMaterials.Count}/{requiredMaterialCount})";

            for (int i = 0; i < materialImages.Length; i++)
            {
                if (i < selectedMaterials.Count)
                {
                    materialImages[i].gameObject.SetActive(true);
                    materialImages[i].sprite = selectedMaterials[i].data.HeroIcon;
                }
                else
                {
                    materialImages[i].gameObject.SetActive(false);
                }
            }
        }

        //합성 재료 선택
        public void SelectMaterial(HeroInstance hero)
        {
            if (hero == null || hero.data == null) return;

            //이미 선택한 재료면 선택 해제
            if (selectedMaterials.Contains(hero))
            {
                selectedMaterials.Remove(hero);
                UpdateMaterialUI();
                return;
            }

            //필요 개수 이상 선택 불가
            if (selectedMaterials.Count >= requiredMaterialCount)
            {
                UINoticePopup.Instance.ShowTime("필요한 재료를 모두 선택했습니다.", 2f);
                return;
            }

            selectedMaterials.Add(hero);

            UpdateMaterialUI();
        }

        //다음 등급 확인
        private HeroGrade GetNextGrade(HeroGrade grade)
        {
            switch (grade)
            {
                case HeroGrade.Normal:
                    return HeroGrade.NormalPlus;

                case HeroGrade.NormalPlus:
                    return HeroGrade.Rare;

                case HeroGrade.Rare:
                    return HeroGrade.RarePlus;

                case HeroGrade.RarePlus:
                    return HeroGrade.Epic;

                case HeroGrade.Epic:
                    return HeroGrade.EpicPlus;

                default:
                    return grade;
            }
        }

        #endregion
    }
}

