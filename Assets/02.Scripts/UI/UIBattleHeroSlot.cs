using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    //전투화면 하단 영웅 슬롯 UI
    public class UIBattleHeroSlot : MonoBehaviour
    {
        [Header("영웅 정보")]
        [SerializeField] private Image heroIcon;         //영웅 아이콘
        [SerializeField] private TMP_Text heroNameText;  //영웅 이름
        [SerializeField] private Slider hpSlider;        //HP 게이지
        [SerializeField] private Slider ultimateSlider;  //궁극기 게이지

        [Header("등급")]
        [SerializeField] private Image gradeImage;       //둥급 테두리
        [SerializeField] private Sprite normal;          //노멀
        [SerializeField] private Sprite rare;            //레어
        [SerializeField] private Sprite epic;            //에픽

        //영웅 정보 적용 
        public void SetHero(HeroInstance hero)
        {
            if (hero == null || hero.data == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);             

            heroIcon.sprite = hero.data.HeroIcon;    //아이콘
            heroNameText.text = hero.data.HeroName;  //이름
            SetHP(1f, 1f);                           //HP 초기화
            SetUltimate(0f, 1f);                     //궁극기게이지 초기화
            SetHeroGrade(hero.currentGrade);         //등급 테두리 적용
        }

        //등급 테두리 설정
        private void SetHeroGrade(HeroGrade grade)
        {
            switch (grade)
            {
                case HeroGrade.Normal:
                case HeroGrade.NormalPlus:
                    gradeImage.sprite = normal; //노멀
                    break;

                case HeroGrade.Rare:
                case HeroGrade.RarePlus:
                    gradeImage.sprite = rare;   //레어
                    break;

                case HeroGrade.Epic:
                case HeroGrade.EpicPlus:
                    gradeImage.sprite = epic;   //에픽
                    break;
            }
        }

        //HP 갱신
        public void SetHP(float currentHP, float maxHP)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        //궁극기 게이지 갱신
        public void SetUltimate(float currentEnergy, float maxEnergy)
        {
            ultimateSlider.maxValue = maxEnergy;
            ultimateSlider.value = currentEnergy;
        }

        //빈자리 표시
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

