using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    //카드 1장에 대한 셋팅
    public class UISummonCard : MonoBehaviour
    {
        [Header("Card")]
        [SerializeField] private Image cardImage;       //등급별 카드 테두리
        [SerializeField] private GameObject cardClose;  //카드 뒷면

        [Header("Hero")]
        [SerializeField] private Image heroImage;       //영웅 이미지
        [SerializeField] private TMP_Text heroNameText; //영웅 이름
        [SerializeField] private TMP_Text gradeText;    //영웅 등급

        [Header("Grade")]
        [SerializeField] private Sprite normalImage;     //노멀
        [SerializeField] private Sprite rareImage;       //레어
        [SerializeField] private Sprite epicImage;       //에픽

        //영웅 데이터 적용
        public void SetHeroData(HeroData hero)
        {
            heroImage.sprite = hero.HeroIcon;           //영웅 이미지 적용
            heroNameText.text = hero.HeroName;          //영웅 이름 적용
            gradeText.text = hero.HeroGrade.ToString(); //영웅 등급 표시
            SetGrade(hero.HeroGrade);                   //등급 테두리 적용
            ShowBack();                                 //카드 뒷면 먼저 보여주기
        }

        //등급 테두리 설정
        private void SetGrade(HeroGrade grade)
        {
            switch (grade)
            {
                //노멀
                case HeroGrade.Normal:
                    cardImage.sprite = normalImage;
                    break;
                //레어
                case HeroGrade.Rare:
                    cardImage.sprite = rareImage;
                    break;
                //에픽
                case HeroGrade.Epic:
                    cardImage.sprite = epicImage;
                    break;
            }
        }

        public void ShowBack() { cardClose.SetActive(true); }   //카드 뒷면 표시
        public void ShowFront() { cardClose.SetActive(false); } //카드 앞면 표시

    }
}

