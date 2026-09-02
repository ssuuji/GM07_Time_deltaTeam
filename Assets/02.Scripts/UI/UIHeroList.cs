using System.Collections.Generic;
using UnityEngine;

namespace AFKHero.UI
{
    public class UIHeroList : MonoBehaviour
    {
        [Header("영웅 슬롯")]
        [SerializeField] private UIHeroSlot heroSlotPrefab;      //영웅슬롯 프리팹

        [Header("영웅 정보 팝업")]
        [SerializeField] private UIHeroInfoPopup heroInfoPopup;  //영웅정보 팝업창

        //영웅 리스트 갱신 (리스트 생성위치, 타입, 선택된 영웅, 모드) : 영웅 카드 소모 기능을 위해 만든 구조이며 추후 필요 없으면 단순화 예정
        public void UpdateList(Transform content, UIHeroSlotType type,  HeroInstance selectHero = null, UIHeroSlotMode mode = UIHeroSlotMode.Party)
        {
            if (content == null) return;
            if (heroSlotPrefab == null) return;

            ClearList(content);                  //기존에 생성되어 있던 슬롯 제거
                                                 
            switch (type)                        
            {                                    
                case UIHeroSlotType.All:         //모든 보유 영웅 표시 
                    CreateAll(content, mode);
                    break;
                
            }
        }

        //슬롯 생성
        private void CreateSlot(Transform content, HeroInstance hero, UIHeroSlotMode mode)
        {
            UIHeroSlot slot = Instantiate(heroSlotPrefab, content);

            slot.SetHero(hero, heroInfoPopup);  //영웅 정보 적용
            slot.SetMode(mode);                 //슬롯 모드 적용
        }

        //슬롯 제거
        private void ClearList(Transform content)
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                content.GetChild(i).gameObject.SetActive(false);
                Destroy(content.GetChild(i).gameObject);
            }
        }

        //모든 보유 영웅 표시
        private void CreateAll(Transform content, UIHeroSlotMode mode)
        {
            List<HeroInstance> heroes = HeroManager.Instance.GetAllHeroes();

            foreach (HeroInstance hero in heroes)
            {
                if (hero == null || hero.data == null) continue;
                if (!hero.isUnlocked) continue;

                CreateSlot(content, hero, mode);
            }
        }

    }
}