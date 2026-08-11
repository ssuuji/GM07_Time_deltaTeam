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

        public HeroInstance Hero { get; private set; } //영웅

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

        //영웅 정보창 여는 버튼
        public void OnClickedHeroInfo()
        {
            if (Hero == null) return;
            if (heroInfoPopup == null) return;

            heroInfoPopup.InfoOpen(Hero); //열기
        }
    }

}
