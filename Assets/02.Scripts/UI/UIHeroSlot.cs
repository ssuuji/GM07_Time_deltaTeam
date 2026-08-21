using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    //영웅을 표시하는 슬롯 UI
    public class UIHeroSlot : MonoBehaviour
    {
        [Header("영웅 아이콘")]
        [SerializeField] private Image heroIcon;    //아이콘

        [Header("등급 배경색")]
        [SerializeField] private Image gradeImage;  //등급 배경
        
        private UIHeroInfoPopup heroInfoPopup;      //영웅 정보 팝업
        private UIHeroSlotMode slotMode;            //현재 슬롯이 사용되고있는 화면모드

        public HeroInstance Hero { get; private set; } //영웅

        //슬롯 모드 설정
        public void SetMode(UIHeroSlotMode mode)
        {
            slotMode = mode;
        }

        //슬롯 영웅 설정
        public void SetHero(HeroInstance hero, UIHeroInfoPopup info)
        {
            Hero = hero;                                                    //슬롯에 표시할 영웅 저장
            heroInfoPopup = info;                                           //영웅정보 팝업창 연결

            if (Hero == null)
            {
                gameObject.SetActive(false);                                //표시할 슬롯이 없다면 숨기기
                return;
            }

            gameObject.SetActive(true);
            heroIcon.sprite = Hero.data.HeroIcon;                           //아이콘
            gradeImage.color = HeroGradeColor.GetColor(Hero.currentGrade);  //등급(색) 설정
        }

        //영웅슬롯 클릭버튼
        public void OnClickedSlot()
        {
            if (Hero == null) return;
            
            switch (slotMode)
            {
                //파티탭
                case UIHeroSlotMode.Party:
                    if (heroInfoPopup == null) return;
                    heroInfoPopup.InfoOpen(Hero, UIHeroSlotMode.Party); //정보 팝업창 열기 (배치하기 버튼)
                    break;

                //성장탭 
                case UIHeroSlotMode.Upgrade:
                    if (UIUpgradeManager.Instance == null) return;
                    UIUpgradeManager.Instance.SelectHero(Hero); //상단 영웅 표시

                    UI_EquipmentPanel equipPanel = FindFirstObjectByType<UI_EquipmentPanel>();
                    if (equipPanel != null && equipPanel.gameObject.activeInHierarchy)
                    {
                        equipPanel.OpenPanel(Hero); //장비창 열기
                    }
                    break;

                //성장탭 - 영웅합성
                case UIHeroSlotMode.UpgradeMaterial:
                    break;

                //공명탭
                case UIHeroSlotMode.Share:
                    if (heroInfoPopup == null) return;
                    heroInfoPopup.InfoOpen(Hero, UIHeroSlotMode.Share); //정보 팝업창 열기 (레벨업 버튼)
                    break;
            }
            
        }
    }

}
