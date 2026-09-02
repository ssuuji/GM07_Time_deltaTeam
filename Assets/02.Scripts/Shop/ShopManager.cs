using AFKHeroPlayerManager = AFKHero.Player.PlayerManager;
using AFKHero.UI;
using System.Collections.Generic;
using UnityEngine;
using AFKHero.Quest;

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

        [Header("Guide")]
        [SerializeField] private RectTransform freeGuideTarget; //가이드 위치
        [SerializeField] private RectTransform oneGuideTarget; //가이드 위치
        [SerializeField] private RectTransform tenGuideTarget; //가이드 위치

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

        public void ShowGuide()
        {
            if (GuideManager.Instance == null) return;
            if (!GuideManager.Instance.IsTarget(GuideTarget.HeroSummon)) return;
            if (!GuideManager.Instance.IsStep(GuideStep.ClickSummon)) return;

            if (playerManager.FreeTicket > 0)
            {
                GuideManager.Instance.ShowGuide(freeGuideTarget);
                return;
            }

            if (playerManager.Dia >= tenCost)
            {
                GuideManager.Instance.ShowGuide(tenGuideTarget);
                return;
            }

            if (playerManager.Dia >= oneCost)
            {
                GuideManager.Instance.ShowGuide(oneGuideTarget);
                return;
            }

            GuideManager.Instance.HideGuide();
        }

        #region 소환 버튼연결
        //1회 소환
        public void OnClickedSummon1()
        {
            if (!playerManager.TryUseDia(oneCost))
            {
                UINoticePopup.Instance.ShowTime("다이아가 부족합니다.");
                return;
            }


            Summon(1);

            if (GuideManager.Instance != null && GuideManager.Instance.IsTarget(GuideTarget.HeroSummon) && GuideManager.Instance.IsStep(GuideStep.ClickSummon))
            {
                GuideManager.Instance.EndGuide();
            }
        }
        //10회 소환
        public void OnClickedSummon10()
        {
            if (!playerManager.TryUseDia(tenCost))
            {
                UINoticePopup.Instance.ShowTime("다이아가 부족합니다.");
                return;
            }


            Summon(10);

            if (GuideManager.Instance != null && GuideManager.Instance.IsTarget(GuideTarget.HeroSummon) && GuideManager.Instance.IsStep(GuideStep.ClickSummon))
            {
                GuideManager.Instance.EndGuide();
            }
        }
        //무료 뽑기권
        public void OnClickedFreeTicket()
        {
            if (!playerManager.TryUseFreeTicket())
            {
                UINoticePopup.Instance.ShowTime("무료 뽑기권이 없습니다.");
                return;
            }

            Summon(1);

            if (GuideManager.Instance != null && GuideManager.Instance.IsTarget(GuideTarget.HeroSummon) && GuideManager.Instance.IsStep(GuideStep.ClickSummon))
            {
                GuideManager.Instance.EndGuide();
            }
        }
        #endregion
    }
}
