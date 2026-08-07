using System.Collections.Generic;
using UnityEngine;

namespace AFKHero.UI
{
    //소환된 영웅의 결과 카드를 보여주는 UI
    public class UISummonResult : MonoBehaviour
    {
        [Header("1회 소환")]
        [SerializeField] private GameObject summon1Panel;  //1회 소환 결과 영역
        [SerializeField] private UISummonCard summon1Card; //1회 소환 카드

        [Header("10회 소환")]
        [SerializeField] private GameObject summon10Panel;         //10회 소환 결과 영역
        [SerializeField] private List<UISummonCard> summon10Cards; //10회 소환 카드

        //소환 결과
        public void ShowResult(List<HeroData> heroes)
        {
            //1회
            if (heroes.Count == 1)
            {
                ShowOneResult(heroes[0]);
            }
            //10회
            else if (heroes.Count == 10)
            {
                ShowTenResult(heroes);
            }
        }

        //1회 소환 결과
        private void ShowOneResult(HeroData hero)
        {
            summon1Panel.SetActive(true);   //1회 영역만 활성화
            summon10Panel.SetActive(false);

            summon1Card.SetHeroData(hero); //카드에 영웅 데이터 적용
        }

        //10회 소환 결과
        private void ShowTenResult(List<HeroData> heroes)
        {
            summon1Panel.SetActive(false);
            summon10Panel.SetActive(true);  //10회 영역만 활성화

            for (int i = 0; i < heroes.Count; i++)
            {
                summon10Cards[i].SetHeroData(heroes[i]); //카드에 영웅 데이터 적용
            }
        }
    }

}
