using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    //영웅을 표시하는 슬롯 UI
    public class UIHeroSlot : MonoBehaviour
    {
        [Header("영웅 아이콘")]
        [SerializeField] private Image heroIcon;

        [Header("등급 배경색")]
        [SerializeField] private Image gradeImage;
        
        private UIHeroInfoPopup heroInfoPopup;
        private UIHeroSlotMode slotMode;

        public HeroInstance Hero { get; private set; } //영웅

        public void SetMode(UIHeroSlotMode mode)
        {
            slotMode = mode;
        }

        //슬롯 설정
        public void SetHero(HeroInstance hero, UIHeroInfoPopup info)
        {
            Hero = hero; //영웅 저장
            heroInfoPopup = info; //영웅정보창
            
            if (Hero == null)
            {
                gameObject.SetActive(false); //영웅값이 없다면 슬롯표시X
                return;
            }

            gameObject.SetActive(true);
            heroIcon.sprite = Hero.data.HeroIcon; //영웅아이콘
            gradeImage.color = HeroGradeColor.GetColor(Hero.data.HeroGrade); //등급별 배경색 설정
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
                    break;

                //성장탭 - 영웅합성
                case UIHeroSlotMode.UpgradeMaterial:
                    if (UIUpgradeManager.Instance == null) return;
                    UIUpgradeManager.Instance.SelectMaterial(Hero); //재료로 사용할 영웅카드 선택
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
