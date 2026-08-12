using TMPro;
using UnityEngine;

namespace AFKHero.UI
{
    //전투탭 UI 매니저
    public class UIBattleManager : MonoBehaviour
    {
        public static UIBattleManager Instance { get; private set; }

        [Header("하단 영웅 UI")]
        [SerializeField] private Transform heroSlotTransform;           //영웅슬롯 생성위치
        [SerializeField] private UIBattleHeroSlot heroSlotPrefab;       //영웅슬롯 프리펩
                                                                        
        [Header("현재 스테이지")]                                        
        [SerializeField] private TMP_Text currentStageText;             //현재 스테이지 표시

        private UIBattleHeroSlot[] heroSlots = new UIBattleHeroSlot[5]; //영웅슬롯

        private void Awake()
        {
            Instance = this;

            CreateHeroSlots();
        }

        //하단 영웅 슬롯 생성
        private void CreateHeroSlots()
        {
            for (int i = 0; i < heroSlots.Length; i++)
            {
                heroSlots[i] = Instantiate(heroSlotPrefab, heroSlotTransform);
            }
        }

        //현재 파티 기준으로 하단 영웅 UI 갱신
        public void UpdatePartyUI()
        {
            if (PartyManager.Instance == null) return;

            for (int i = 0; i < heroSlots.Length; i++)
            {
                HeroInstance hero = PartyManager.Instance.partySlots[i]; //현재 저장된 파티자리 가져오기
                
                if (hero == null || hero.data == null) 
                {
                    heroSlots[i].Hide();                                 //빈 파티 자리는 숨기기
                    continue;
                }

                heroSlots[i].SetHero(hero);                              //배치된 영웅 표시
            }
        }
    }

}
