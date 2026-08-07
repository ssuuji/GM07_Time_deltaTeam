using AFKHero.UI;
using System.Collections.Generic;
using UnityEngine;

namespace AFKHero.Shop
{
    public class ShopManager : MonoBehaviour
    {
        [Header("Dia Cost")]
        [SerializeField] private int oneCost = 100;
        [SerializeField] private int tenCost = 1000;

        [Header("Hero Summon")]
        [SerializeField] private HeroSummonManager heroSummonManager; //영웅 소환
        [SerializeField] private UISummonResult summonResult;

        //영웅 소환
        private void Summon(int count)
        {
            List<HeroData> result = heroSummonManager.Summon(count); //소환요청 보내기
            summonResult.ShowResult(result);                         //결과 UI에게 전달
        }

        //다이아 소모
        private bool TryUseDia(int cost)
        {
            //다이아 보유량 확인

            //소모

            return true;
        }
        
        #region 소환 버튼연결
        //1회 소환
        public void OnClickedSummon1()
        {
            if (!TryUseDia(oneCost)) return;

            Summon(1);
        }
        //10회 소환
        public void OnClickedSummon10()
        {
            if (!TryUseDia(tenCost)) return;

            Summon(10);
        }
        //무료 뽑기권
        public void OnClickedFreeTicket()
        {
            //무료는 다이아소모 체크를 하지 않음
            Summon(1);
        }
        #endregion
    }
}
