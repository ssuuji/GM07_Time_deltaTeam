using AFKHero.Quest;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using AFKHeroPlayerManager = AFKHero.Player.PlayerManager;

namespace AFKHero.UI
{
    public class UIUpgradeManager : MonoBehaviour
    {
        public static UIUpgradeManager Instance { get; private set; }

        [Header("영웅 리스트")]
        [SerializeField] private Transform upgradeContent;       //영웅리스트 생성위치
        [SerializeField] private UIHeroList heroList;            //영웅리스트
                                                                 
        [Header("선택 영웅")]                                     
        [SerializeField] private Transform heroPrefabs;          //영웅 프리펩 생성위치
        [SerializeField] private TMP_Text heroLevelText;         //영웅 레벨
        [SerializeField] private TMP_Text heroAttackText;        //영웅 공격력
        [SerializeField] private TMP_Text heroDefenseText;       //영웅 방어력
        [SerializeField] private TMP_Text heroHpText;            //영웅 체력

        [Header("레벨업")]
        [SerializeField] private Button levelUpButton;           //레벨업 버튼
        [SerializeField] private TMP_Text levelUpCostText;       //레벨업 비용
        private float levelUpHoldDelay = 0.4f;                   //연속 레벨업 시작 대기시간
        private float levelUpRepeatInterval = 0.08f;             //연속 레벨업 간격
        private Coroutine levelUpHoldCoroutine; //연속 레벨업 코루틴
        private bool levelUpHoldTriggered;      //길게 누르기 실행 여부

        [Header("영웅 합성")]
        [SerializeField] private Button gradeUpButton;           //영웅 합성 버튼
        [SerializeField] private TMP_Text gradeUpShardCountText; //(보유 조각 / 필요 조각)

        private HeroInstance selectedHero;                       //선택한 영웅
        private GameObject heroPrefab;                           //영웅 프리펩

        private readonly Color32 levelYellow = new Color32(219, 216, 77, 255); //노란색 (레벨업 가능 색)
        private readonly Color32 levelRed    = new Color32(224, 90, 90, 255);  //빨간색 (레벨업 불가능 색)

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            selectedHero = null;
            SetSelectedHeroUI(false);

            if (AFKHeroPlayerManager.Instance != null)
            {
                AFKHeroPlayerManager.Instance.OnPlayerInfoChanged += UpdateLevelUpCost;
            }
        }

        private void OnDisable()
        {
            if (AFKHeroPlayerManager.Instance != null)
            {
                AFKHeroPlayerManager.Instance.OnPlayerInfoChanged -= UpdateLevelUpCost;
            }
        }

        private void Start()
        {
            SetLevelUpHoldEvent();
            UpdateHeroList();
        }

        //성장 UI 전체 갱신
        public void UpdateUpgradeUI()
        {
            UpdateHeroList();

            if (selectedHero != null)
            {
                UpdateSelectedHero();
            }
        }

        //선택 영웅 UI 표시
        private void SetSelectedHeroUI(bool active)
        {
            heroPrefabs.gameObject.SetActive(active);            //영웅 프리펩
            heroLevelText.gameObject.SetActive(active);          //레벨
            heroAttackText.gameObject.SetActive(active);         //공격력
            heroDefenseText.gameObject.SetActive(active);        //방어력
            if (heroHpText != null) heroHpText.gameObject.SetActive(active); //체력
            levelUpCostText.gameObject.SetActive(active);        //레벨업 비용
            gradeUpShardCountText.gameObject.SetActive(active);  //영웅 합성 재료

            //선택된 영웅이 없으면 버튼 비활성화
            if (!active)
            {
                levelUpButton.interactable = false;
                gradeUpButton.interactable = false;
            }
        }

        #region 영웅 리스트

        //영웅 리스트 갱신
        public void UpdateHeroList()
        {
            if (heroList == null) return;
            if (upgradeContent == null) return;

            heroList.UpdateList(upgradeContent, UIHeroSlotType.All, null, UIHeroSlotMode.Upgrade); //전체 영웅 표시
            ShowSelectHeroGuide();
        }

        //영웅 선택
        public void SelectHero(HeroInstance hero)
        {
            if (hero == null || hero.data == null) return;

            selectedHero = hero;                                    //선택한 영웅 저장
            SetSelectedHeroUI(true);                                //상단에 영웅UI 표시
            UpdateSelectedHero();                                   //선택한 영웅정보 표시

            ShowLevelUpGuide();
        }

        //선택한 영웅 UI 갱신
        private void UpdateSelectedHero()
        {
            if (selectedHero == null || selectedHero.data == null) return;

            SetHeroPrefab(selectedHero);                             //영웅 프리펩
            heroLevelText.text = $"LV. {selectedHero.level}";        //레벨
            heroAttackText.text = selectedHero.Attack.ToString();    //공격력
            heroDefenseText.text = selectedHero.Defense.ToString();  //방어력
            if (heroHpText != null) heroHpText.text = selectedHero.MaxHP.ToString(); //체력

            UpdateLevelUpCost();                                     //레벨업 비용계산
            UpdateGradeUpUI();                                       //영웅 승급 UI

            UI_EquipmentPanel equipPanel = FindFirstObjectByType<UI_EquipmentPanel>(FindObjectsInactive.Include);
            if (equipPanel != null)
            {
                equipPanel.currentHero = selectedHero;
                equipPanel.RefreshUI();
            }
        }

        //영웅 프리펩 표시
        private void SetHeroPrefab(HeroInstance hero)
        {
            if (heroPrefab != null)
            {
                Destroy(heroPrefab);
                heroPrefab = null;
            }
            if (hero == null || hero.data == null || hero.data.HeroPrefab == null) return;

            //영웅 프리펩 생성
            heroPrefab = Instantiate(hero.data.HeroPrefab, heroPrefabs);

            //위치 및 크기
            heroPrefab.transform.localPosition = Vector3.zero;
            heroPrefab.transform.localRotation = Quaternion.identity;
            heroPrefab.transform.localScale = Vector3.one * 300f;

            //UI 위에 표시
            SortingGroup sortingGroup = heroPrefab.GetComponentInChildren<SortingGroup>();

            if (sortingGroup != null)
            {
                sortingGroup.sortingLayerName = "UI";
                sortingGroup.sortingOrder = 10;
            }
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
                levelUpButton.interactable = false;  //레벨업 버튼 비활성화
                return;
            }

            int cost = selectedHero.LevelUpCost;     //레벨업 비용계산
            bool canLevelUp = AFKHeroPlayerManager.Instance.Gold >= cost && !selectedHero.isResonanced;

            if (selectedHero.isResonanced)
            {
                levelUpCostText.text = "공명 시 레벨업 불가";
            }
            else
            {
                levelUpCostText.text = cost.ToString();
            }
            
            //레벨업 비용표시
            levelUpCostText.color = canLevelUp ? levelYellow : levelRed; //가능 여부에 따라 색상
            levelUpButton.interactable = canLevelUp;                     //골드가 충분하면 버튼 활성화
        }

        //레벨업 버튼
        public void OnClickedLevelUp()
        {
            //길게 눌러서 이미 연속 레벨업이 실행된 경우
            //버튼을 뗄 때 발생하는 OnClick은 무시
            if (levelUpHoldTriggered)
            {
                levelUpHoldTriggered = false;
                return;
            }

            LevelUpOnce();
        }

        //레벨업 1회
        private void LevelUpOnce()
        {
            if (selectedHero == null || selectedHero.data == null) return;
            if (selectedHero.level >= 50) return;                                            //최고레벨이면 return
            if (!AFKHeroPlayerManager.Instance.TryUseGold(selectedHero.LevelUpCost)) return; //비용이 부족하면 return

            HeroManager.Instance.LevelUpHero(selectedHero.data.HeroID);

            QuestManager.Instance?.AddProgress(QuestConditionType.HeroLevelUp);              //영웅 레벨업 퀘스트 진행도 증가
            UpdateSelectedHero();   //영웅UI 갱신
            UpdateHeroList();

            if (GuideManager.Instance != null && GuideManager.Instance.IsTarget(GuideTarget.HeroUpgrade) && GuideManager.Instance.IsStep(GuideStep.ClickLevelUpButton))
            {
                GuideManager.Instance.EndGuide();
            }
        }

        //레벨업 버튼 길게 누르기 이벤트 설정
        private void SetLevelUpHoldEvent()
        {
            if (levelUpButton == null) return;

            EventTrigger trigger = levelUpButton.GetComponent<EventTrigger>();

            if (trigger == null)
            {
                trigger = levelUpButton.gameObject.AddComponent<EventTrigger>();
            }

            //버튼 누름
            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => StartLevelUpHold());
            trigger.triggers.Add(pointerDown);

            //버튼 뗌
            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => StopLevelUpHold());
            trigger.triggers.Add(pointerUp);

            //버튼 영역에서 벗어남
            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => CancelLevelUpHold());
            trigger.triggers.Add(pointerExit);
        }

        //레벨업 버튼 누르기 시작
        private void StartLevelUpHold()
        {
            if (!levelUpButton.interactable) return;

            levelUpHoldTriggered = false;

            if (levelUpHoldCoroutine != null)
            {
                StopCoroutine(levelUpHoldCoroutine);
            }

            levelUpHoldCoroutine = StartCoroutine(LevelUpHoldRoutine());
        }

        //레벨업 버튼 길게 누르기
        private IEnumerator LevelUpHoldRoutine()
        {
            yield return new WaitForSecondsRealtime(levelUpHoldDelay);

            levelUpHoldTriggered = true;

            while (levelUpButton.interactable)
            {
                LevelUpOnce();

                yield return new WaitForSecondsRealtime(levelUpRepeatInterval);
            }

            levelUpHoldCoroutine = null;
        }

        //레벨업 버튼 뗌
        private void StopLevelUpHold()
        {
            if (levelUpHoldCoroutine != null)
            {
                StopCoroutine(levelUpHoldCoroutine);
                levelUpHoldCoroutine = null;
            }
        }

        //버튼 영역에서 벗어남
        private void CancelLevelUpHold()
        {
            StopLevelUpHold();
            levelUpHoldTriggered = false;
        }
        #endregion

        #region 영웅 등급업

        //영웅 승급 UI 갱신
        private void UpdateGradeUpUI()
        {
            if (selectedHero == null || selectedHero.data == null) return;
            if (HeroManager.Instance == null) return;

            if (selectedHero.currentGrade >= HeroGrade.EpicPlus) //최고 등급
            {
                gradeUpShardCountText.text = "MAX";
                gradeUpShardCountText.color = levelRed;
                gradeUpButton.interactable = false;
                return;
            }

            HeroGrade requiredGrade = selectedHero.GetRequiredShardGrade(); //현재 승급에 필요한 조각 종류
            
            int requiredCount = selectedHero.GetRequiredShardCount();       //현재 승급에 필요한 조각 개수
            int currentCount = 0;                                           //현재 가지고 있는 조각 개수

            switch (requiredGrade)
            {
                case HeroGrade.Normal:
                    currentCount = HeroManager.Instance.normalShards;
                    break;

                case HeroGrade.Rare:
                    currentCount = HeroManager.Instance.rareShards;
                    break;

                case HeroGrade.Epic:
                    currentCount = HeroManager.Instance.epicShards;
                    break;
            }

            gradeUpShardCountText.text = $"({currentCount} / {requiredCount})";                   //(보유 개수 / 필요 개수)
            gradeUpShardCountText.color = currentCount >= requiredCount ? levelYellow : levelRed; //가능하면 노란색 표시 아니라면 빨간색 표시
            gradeUpButton.interactable = currentCount >= requiredCount;                           //조각이 충분하면 버튼 활성화
        }


        //영웅 합성 버튼
        public void OnClickedGradeUp()
        {
            if (selectedHero == null || selectedHero.data == null) return;
            if (HeroManager.Instance == null) return;
            if (selectedHero.currentGrade >= HeroGrade.EpicPlus) return;

            
            bool success = HeroManager.Instance.UpgradeHeroGrade(selectedHero.data.HeroID); //승급 요청
            if (!success) return;

            UpdateSelectedHero(); //선택 영웅 정보 갱신
            UpdateHeroList();     //영웅 리스트 갱신
        }

        #endregion

        #region 장비 일괄장착

        //장비 일괄장착
        public void OnClickedAutoEquip()
        {
            if (selectedHero == null || selectedHero.data == null) return;

            // 장비 매니저를 호출해서 일괄 장착 실행
            EquipmentManager.Instance.AutoEquip(selectedHero);

            // 장착 완료 후 현재 창의 스탯 및 장비 슬롯 UI를 갱신
            UpdateSelectedHero();
        }
        #endregion

        #region 가이드

        //영웅 선택 가이드 표시
        private void ShowSelectHeroGuide()
        {
            if (GuideManager.Instance == null) return;
            if (!GuideManager.Instance.IsTarget(GuideTarget.HeroUpgrade)) return;
            if (!GuideManager.Instance.IsStep(GuideStep.SelectHero)) return;

            UIHeroSlot[] heroSlots = upgradeContent.GetComponentsInChildren<UIHeroSlot>();

            foreach (UIHeroSlot heroSlot in heroSlots)
            {
                HeroInstance hero = heroSlot.Hero;

                if (hero == null) continue;
                if (hero.level >= 50) continue;
                if (hero.isResonanced) continue;
                if (AFKHeroPlayerManager.Instance.Gold < hero.LevelUpCost) continue;

                GuideManager.Instance.ShowGuide(heroSlot.GetComponent<RectTransform>());
                return;
            }
        }

        //레벨업 버튼 가이드 표시
        private void ShowLevelUpGuide()
        {
            if (GuideManager.Instance == null) return;
            if (!GuideManager.Instance.IsTarget(GuideTarget.HeroUpgrade)) return;
            if (!GuideManager.Instance.IsStep(GuideStep.SelectHero)) return;
            if (!levelUpButton.interactable) return;

            GuideManager.Instance.ChangeStep(GuideStep.ClickLevelUpButton);
            GuideManager.Instance.ShowGuide(levelUpButton.GetComponent<RectTransform>());
        }
        #endregion
    }
}

