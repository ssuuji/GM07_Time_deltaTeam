using AFKHero.Quest;
using AFKHero.Shop;
using TMPro;
using UnityEngine;

namespace AFKHero.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("상점")]
        [SerializeField] private ShopManager shopManager;

        [Header("메인 탭 Panel")]
        [SerializeField] private GameObject battlePanel;       //전투탭 패널
        [SerializeField] private GameObject heroPanel;         //영웅탭 패널
        [SerializeField] private GameObject shopPanel;         //상점탭 패널

        [Header("메인 탭 Icon")]
        [SerializeField] private GameObject battleIcon;       //전투탭 아이콘
        [SerializeField] private GameObject heroIcon;         //영웅탭 아이콘
        [SerializeField] private GameObject shopIcon;         //상점탭 아이콘
        [SerializeField] private RectTransform closeIcon;     //공용 X 아이콘

        [Header("영웅 탭 내부 Panel")]
        [SerializeField] private GameObject partyPanel;        //파티
        [SerializeField] private GameObject upgradePanel;      //성장
        [SerializeField] private GameObject sharePanel;        //공명
        [SerializeField] private TMP_Text heroTitletxt;        //영웅탭 타이틀

        private UIMainTab defaultView = UIMainTab.None;        //게임 시작 시 "방치전투" 화면을 기본으로 함
        private UIHeroTab defaultHeroView = UIHeroTab.Party;   //게임 시작 시 영웅 내부 탭의 "파티"를 기본으로 함.
        public UIMainTab CurrentView { get; private set; }     //현재 화면
        public UIHeroTab CurrentHeroView { get; private set; } //현재 영웅 내부 탭 화면

        public event System.Action<UIMainTab> OnMainViewChanged;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            CurrentHeroView = defaultHeroView; //영웅탭
            OpenView(defaultView);             //메인탭
        }

        #region 메인탭

        //지정한 메인 탭 열기
        public void OpenView(UIMainTab view)
        {
            SetPanelActive(battlePanel, view == UIMainTab.Battle); //전투탭이면 true
            SetPanelActive(heroPanel, view == UIMainTab.Hero);     //영웅탭이면 true
            SetPanelActive(shopPanel, view == UIMainTab.Shop);     //상점탭이면 true

            CurrentView = view;                                    //현재 화면 상태저장
                                                                   
            UpdateMainTabIcon(view);                               //현재 탭에 맞게 아이콘 변경
            UpdateView(CurrentView);                               //현재 화면의 데이터 갱신

            OnMainViewChanged?.Invoke(CurrentView);
        }

        //메인 탭 버튼 클릭 시 화면 열기/닫기
        private void ToggleView(UIMainTab view)
        {
            //현재 열려있는 탭을 다시 눌렀으면 닫기
            if (CurrentView == view)
            {
                CloseCurrentView();
                return;
            }

            //다른 탭을 눌렀으면 해당 탭 열기
            OpenView(view);
        }

        //현재 열려있는 메인 탭 닫기
        private void CloseCurrentView()
        {
            OpenView(UIMainTab.None);
        }

        //현재 열려있는 메인 탭 닫기 (public) .. 그냥 하나로 합칠까,.?
        public void CloseView()
        {
            CloseCurrentView();
        }

        //현재 메인 탭이 열려있다면 닫기
        public bool TryCloseMainTab()
        {
            //열려있는 메인 탭이 없다면
            if (CurrentView == UIMainTab.None)
            {
                return false;
            }

            CloseCurrentView();

            return true;
        }

        #region 메인 탭 아이콘

        //현재 열려있는 메인 탭에 맞게 기본 아이콘 / X 아이콘 변경
        private void UpdateMainTabIcon(UIMainTab view)
        {
            //선택된 탭의 기본 아이콘은 숨기고 선택되지 않은 탭의 기본 아이콘은 표시
            battleIcon.SetActive(view != UIMainTab.Battle);
            heroIcon.SetActive(view != UIMainTab.Hero);
            shopIcon.SetActive(view != UIMainTab.Shop);

            //열려있는 탭이 없으면 X 아이콘 숨김
            if (view == UIMainTab.None)
            {
                closeIcon.gameObject.SetActive(false);
                return;
            }

            RectTransform targetIcon = null;

            //현재 선택된 탭의 아이콘 위치 가져오기
            switch (view)
            {
                case UIMainTab.Battle:
                    targetIcon = battleIcon.GetComponent<RectTransform>();
                    break;

                case UIMainTab.Hero:
                    targetIcon = heroIcon.GetComponent<RectTransform>();
                    break;

                case UIMainTab.Shop:
                    targetIcon = shopIcon.GetComponent<RectTransform>();
                    break;
            }

            if (targetIcon == null) return;

            closeIcon.SetParent(targetIcon.parent, false); //X 아이콘을 현재 선택된 탭 아이콘과 같은 부모로 이동

            //기본 아이콘과 같은 위치에 X 아이콘 배치
            closeIcon.anchoredPosition = targetIcon.anchoredPosition;
            closeIcon.sizeDelta = targetIcon.sizeDelta;
            closeIcon.localScale = targetIcon.localScale;
            closeIcon.localRotation = targetIcon.localRotation;

            closeIcon.gameObject.SetActive(true);
        }

        #endregion

        #region 메인 탭 화면 갱신

        //현재 열려있는 화면의 데이터 갱신
        private void UpdateView(UIMainTab view)
        {
            switch (view)
            {
                //메인 화면(방치전투 화면)
                case UIMainTab.None:
                    UpdateIdleBattleView();
                    break;

                //영웅
                case UIMainTab.Hero:
                    UpdateHeroView();
                    break;
                //전투
                case UIMainTab.Battle:
                    break;
                //상점
                case UIMainTab.Shop:
                    UpdateShopView();
                    break;
            }
        }

        //영웅탭 화면 갱신
        private void UpdateHeroView()
        {
            OpenHeroView(CurrentHeroView);
        }

        //방치전투 화면 갱신
        private void UpdateIdleBattleView()
        {
            if (UIBattleManager.Instance == null) return;

            UIBattleManager.Instance.UpdateBattleUI();
        }

        //상점탭 화면 갱신
        private void UpdateShopView()
        {
            shopManager?.ShowGuide();
        }
        #endregion

        #region 메인 탭 버튼연결

        //영웅탭 버튼연결
        public void OnClickedOpenHero()
        {
            ToggleView(UIMainTab.Hero);
        }
        //전투탭 버튼연결
        public void OnClickedOpenBattle()
        {
            if (StageManager.Instance == null)
            {
                Debug.LogWarning("[UIManager] : StageManager가 없습니다.");
                return;
            }

            //전투 중이거나 결과 화면이 떠있는 경우 전투 프리뷰를 열지 않음
            if (StageManager.Instance.CurrentState != StageState.Idle)
            {
                Debug.Log("[UIManager] : 현재 전투가 진행중이거나 결과 패널이 떠 있는 상태입니다.");
                return;
            }

            ToggleView(UIMainTab.Battle);
        }
        //상점탭 버튼연결
        public void OnClickedOpenShop()
        {
            ToggleView(UIMainTab.Shop);
        }

        #endregion

        #endregion

        #region 영웅 내부 탭 

        //현재 상태에 맞게 패널을 활성화
        public void OpenHeroView(UIHeroTab view)
        {
            SetPanelActive(partyPanel, view == UIHeroTab.Party);     //파티탭 활성화
            SetPanelActive(upgradePanel, view == UIHeroTab.Upgrade); //성장탭 활성화
            SetPanelActive(sharePanel,view == UIHeroTab.Share);      //공명탭 활성화

            CurrentHeroView = view;                                  //현재 화면 상태저장
            UpdateHeroView(CurrentHeroView);                         //현재 화면의 데이터 갱신
        }

        #region 영웅 내부탭 화면갱신

        //현재 열려있는 화면의 데이터 갱신
        private void UpdateHeroView(UIHeroTab view)
        {
            switch (view)
            {
                //파티
                case UIHeroTab.Party:
                    UpdatePartyView();
                    break;
                //성장
                case UIHeroTab.Upgrade:
                    UpdateUpgradeView();
                    break;
                //공명
                case UIHeroTab.Share:
                    UpdateShareView();
                    break;
            }
        }

        //파티탭 화면 갱신
        private void UpdatePartyView()
        {
            heroTitletxt.text = "파티";
            UIPartyManager.Instance.UpdateHeroList();   //영웅리스트 조회
        }

        //성장탭 화면 갱신
        private void UpdateUpgradeView()
        {
            heroTitletxt.text = "성장";
            UIUpgradeManager.Instance.UpdateHeroList(); //영웅리스트 조회
        }

        //공명탭 화면 갱신
        private void UpdateShareView()
        {
            heroTitletxt.text = "공명";

            //배치하지 않았을 때를 위한 예외처리.
            if(UIResonanceManager.Instance == null)
            {
                Debug.LogWarning("[UIManager] : UIResonanceManager가 현재 씬에 없습니다");
                return;
            }
            UIResonanceManager.Instance.UpdateHeroList();
        }
        #endregion

        #region 영웅 내부 버튼연결

        //파티탭 버튼연결
        public void OnClickedOpenParty()
        {
            OpenHeroView(UIHeroTab.Party);
        }

        //성장탭 버튼연결
        public void OnClickedOpenUpgrade()
        {
            OpenHeroView(UIHeroTab.Upgrade);
        }

        //공명탭 버튼연결
        public void OnClickedOpenShare()
        {
            OpenHeroView(UIHeroTab.Share);
        }

        #endregion

        #endregion

        #region 퀘스트 가이드

        //메인 퀘스트의 목적지 화면 열기
        public void OpenGuideTarget(GuideTarget guideTarget)
        {
            switch (guideTarget)
            {
                //파티
                case GuideTarget.Party:
                    CurrentHeroView = UIHeroTab.Party; //영웅 내부 탭을 파티로 설정
                    OpenView(UIMainTab.Hero);          //영웅탭 열기
                    break;

                //영웅 성장
                case GuideTarget.HeroUpgrade:
                    CurrentHeroView = UIHeroTab.Upgrade; //영웅 내부 탭을 성장으로 설정
                    OpenView(UIMainTab.Hero);            //영웅탭 열기
                    break;

                //영웅 소환
                case GuideTarget.HeroSummon:
                    OpenView(UIMainTab.Shop); //상점탭 열기
                    break;

                //전투
                case GuideTarget.Battle:
                    OpenView(UIMainTab.Battle);
                    break;

                //이동할 목적지 없음
                case GuideTarget.None:
                    break;
            }
        }

        #endregion

        //패널 활성화 상태 변경용
        private void SetPanelActive(GameObject panel, bool isActive)
        {
            if (panel == null) return;

            panel.SetActive(isActive);
        }
    }
}

