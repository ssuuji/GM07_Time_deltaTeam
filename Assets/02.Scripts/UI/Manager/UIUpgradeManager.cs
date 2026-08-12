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
        [SerializeField] private Transform upgradeContent;       //영웅리스트 생성위치
        [SerializeField] private UIHeroList heroList;            //영웅리스트
                                                                 
        [Header("선택 영웅")]                                     
        [SerializeField] private Image heroImage;                //영웅 아이콘
        [SerializeField] private TMP_Text heroLevelText;         //영웅 레벨
        [SerializeField] private TMP_Text heroAttackText;        //영웅 공격력
        [SerializeField] private TMP_Text heroDefenseText;       //영웅 방어력
                                                                 
        [Header("레벨업")]                                        
        [SerializeField] private TMP_Text levelUpCostText;       //레벨업 비용
                                                                 
        private HeroInstance selectedHero;                       //선택한 영웅

        private readonly Color32 levelYellow = new Color32(219, 216, 77, 255); //노란색 (레벨업 가능 색)
        private readonly Color32 levelRed    = new Color32(224, 90, 90, 255);  //빨간색 (레벨업 불가능 색)

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            SetSelectedHeroUI(false);
        }

        private void Start()
        {
            UpdateHeroList();
        }

        //선택 영웅 UI 표시
        private void SetSelectedHeroUI(bool active)
        {
            heroImage.gameObject.SetActive(active);       //아이콘
            heroLevelText.gameObject.SetActive(active);   //레벨
            heroAttackText.gameObject.SetActive(active);  //공격력
            heroDefenseText.gameObject.SetActive(active); //방어력
        }

        #region 영웅 리스트

        //영웅 리스트 갱신
        public void UpdateHeroList()
        {
            if (heroList == null) return;
            if (upgradeContent == null) return;

            heroList.UpdateList(upgradeContent, UIHeroSlotType.All, null, UIHeroSlotMode.Upgrade); //전체 영웅 표시
        }

        //영웅 선택
        public void SelectHero(HeroInstance hero)
        {
            if (hero == null || hero.data == null) return;

            selectedHero = hero;                                    //선택한 영웅 저장
            SetSelectedHeroUI(true);                                //상단에 영웅UI 표시
            UpdateSelectedHero();                                   //선택한 영웅정보 표시
        }

        //선택한 영웅 UI 갱신
        private void UpdateSelectedHero()
        {
            if (selectedHero == null || selectedHero.data == null) return;

            heroImage.sprite = selectedHero.data.HeroIcon;           //아이콘
            heroLevelText.text = $"LV. {selectedHero.level}";        //레벨
            heroAttackText.text = selectedHero.Attack.ToString();    //공격력
            heroDefenseText.text = selectedHero.Defense.ToString();  //방어력

            UpdateLevelUpCost();                                     //레벨업 비용계산
        }
        #endregion

        #region 레벨업

        //레벨업 비용 UI 갱신
        private void UpdateLevelUpCost()
        {
            if (selectedHero == null) return;

            
            if (selectedHero.level >= 50)
            {
                levelUpCostText.text = "MAX";        //최고레벨이면 MAX 표시
                levelUpCostText.color = levelRed;    //빨간색 표시
                return;
            }

            int cost = selectedHero.LevelUpCost;     //레벨업 비용계산
            levelUpCostText.text = cost.ToString();  //레벨업 비용표시
            levelUpCostText.color = AFKHeroPlayerManager.Instance.Gold < cost ? levelRed : levelYellow; 
        }

        //레벨업 버튼
        public void OnClickedLevelUp()
        {
            if (selectedHero == null || selectedHero.data == null) return;
            if (selectedHero.level >= 50) return;                                            //최고레벨이면 return
            if (!AFKHeroPlayerManager.Instance.TryUseGold(selectedHero.LevelUpCost)) return; //비용이 부족하면 return

            selectedHero.LevelUp(); //레벨업
            UpdateSelectedHero();   //영웅UI 갱신
        }
        #endregion

        #region 영웅 등급업
        #endregion

    }
}

