using TMPro;
using UnityEngine;

namespace AFKHero.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("메인 탭 Panel")]
        [SerializeField] private GameObject heroPanel; //영웅탭 패널
        [SerializeField] private GameObject shopPanel; //상점탭 패널     //전투탭은 기본배경(?) 이므로 제외

        [Header("영웅 탭 내부 Panel")]
        [SerializeField] private GameObject partyPanel;   //파티
        [SerializeField] private GameObject upgradePanel; //성장
        [SerializeField] private GameObject sharePanel;   //공명
        [SerializeField] private TMP_Text heroTitletxt;   //영웅탭 타이틀

        private UIMainTab defaultView = UIMainTab.Battle;    //게임 시작 시 "전투" 화면을 기본으로 함
        private UIHeroTab defaultHeroView = UIHeroTab.Party; //게임 시작 시 영웅 내부 탭의 "파티"를 기본으로 함.
        public UIMainTab CurrentView { get; private set; }     //현재 화면
        public UIHeroTab CurrentHeroView { get; private set; } //현재 영웅 내부 탭 화면

        private void Start()
        {
            CurrentHeroView = defaultHeroView; //영웅탭
            OpenView(defaultView);             //메인탭
        }

        #region 메인탭
        //현재 상태에 맞게 패널을 활성화
        public void OpenView(UIMainTab view)
        {                                         
            SetPanelActive(heroPanel, view == UIMainTab.Hero); //영웅탭이면 true
            SetPanelActive(shopPanel, view == UIMainTab.Shop); //상점탭이면 true
            //전투탭의 경우 두 조건이 맞지 않아 자동으로 false

            CurrentView = view;      //현재 화면 상태저장
            UpdateView(CurrentView); //현재 화면의 데이터 갱신
        }

        #region 메인 탭 화면 갱신
        //현재 열려있는 화면의 데이터 갱신
        private void UpdateView(UIMainTab view)
        {
            switch (view)
            {
                //영웅
                case UIMainTab.Hero:
                    UpdateHeroView();
                    break;
                //전투
                case UIMainTab.Battle:
                    UpdateBattleView();
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

        //전투탭 화면 갱신
        private void UpdateBattleView()
        {

        }

        //상점탭 화면 갱신
        private void UpdateShopView()
        {

        }
        #endregion

        #region 메인 탭 버튼연결
        //영웅탭 버튼연결
        public void OnClickedOpenHero()
        {
            OpenView(UIMainTab.Hero);
        }
        //전투탭 버튼연결
        public void OnClickedOpenBattle()
        {
            OpenView(UIMainTab.Battle);
        }
        //상점탭 버튼연결
        public void OnClickedOpenShop()
        {
            OpenView(UIMainTab.Shop);
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

            CurrentHeroView = view;          //현재 화면 상태저장
            UpdateHeroView(CurrentHeroView); //현재 화면의 데이터 갱신
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
            UIPartyManager.Instance.UpdateHeroList(); //영웅리스트 조회
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

        //패널 활성화 상태 변경용
        private void SetPanelActive(GameObject panel, bool isActive)
        {
            if (panel == null) return;

            panel.SetActive(isActive);
        }
    }
}

