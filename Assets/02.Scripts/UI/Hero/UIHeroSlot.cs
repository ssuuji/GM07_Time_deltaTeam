using System;
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

        public HeroInstance Hero { get; private set; } //영웅

        //슬롯 설정
        public void SetHero(HeroInstance hero)
        {
            Hero = hero; //영웅 저장
            
            if (Hero == null)
            {
                gameObject.SetActive(false); //영웅값이 없다면 슬롯표시X
                return;
            }

            gameObject.SetActive(true);
            heroIcon.sprite = Hero.data.HeroIcon; //영웅아이콘
            SetGradeColor();                      //등급별 배경색 설정
        }

        private void SetGradeColor()
        {
            switch (Hero.data.HeroGrade)
            {
                //노멀
                case HeroGrade.Normal:
                case HeroGrade.NormalPlus:
                    gradeImage.color = new Color32(160, 160, 160, 255);
                    break;
                //레어
                case HeroGrade.Rare:
                case HeroGrade.RarePlus:
                    gradeImage.color = new Color32(61, 141, 255, 255);
                    break;
                //에픽
                case HeroGrade.Epic:
                case HeroGrade.EpicPlus:
                    gradeImage.color = new Color32(176, 76, 255, 255);
                    break;
            }
        }
    }

}
