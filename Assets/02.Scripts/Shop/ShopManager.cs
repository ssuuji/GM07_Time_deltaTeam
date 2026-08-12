using AFKHeroPlayerManager = AFKHero.Player.PlayerManager;
using AFKHero.UI;
using System.Collections.Generic;
using UnityEngine;

namespace AFKHero.Shop
{
    public class ShopManager : MonoBehaviour
    {
        [Header("Dia Cost")]
        [SerializeField] private int oneCost = 100;                   //1회  소환 다이아비용
        [SerializeField] private int tenCost = 1000;                  //10회 소환 다이아비용

        [Header("Hero Summon")]
        [SerializeField] private HeroSummonManager heroSummonManager; //영웅 소환
        [SerializeField] private UISummonResult summonResult;         //소환 결과 UI

        [Header("Player")]
        [SerializeField] private AFKHeroPlayerManager playerManager;  //플레이어 정보

        //영웅 소환
        private void Summon(int count)
        {
            List<HeroData> result = heroSummonManager.Summon(count); //소환요청 보내기

            foreach (HeroData hero in result)
            {
                if (hero == null) continue;

                HeroManager.Instance.UnlockHero(hero.HeroID);        //소환된 영웅 해금
            }

            summonResult.ShowResult(result);                         //결과 UI에게 전달
        }
        
        #region 소환 버튼연결
        //1회 소환
        public void OnClickedSummon1()
        {
            if (!playerManager.TryUseDia(oneCost)) return;

            Summon(1);
        }
        //10회 소환
        public void OnClickedSummon10()
        {
            if (!playerManager.TryUseDia(tenCost)) return;

            Summon(10);
        }
        //무료 뽑기권
        public void OnClickedFreeTicket()
        {
            if (!playerManager.TryUseFreeTicket()) return;

            Summon(1);
        }
        #endregion
    }
}
