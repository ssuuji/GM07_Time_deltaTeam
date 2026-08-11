using System.Collections.Generic;
using UnityEngine;

namespace AFKHero.UI
{
    public class UIHeroList : MonoBehaviour
    {
        [Header("영웅 슬롯")]
        [SerializeField] private UIHeroSlot heroSlotPrefab;

        [Header("영웅 정보 팝업")]
        [SerializeField] private UIHeroInfoPopup heroInfoPopup;

        //영웅 리스트 갱신
        public void UpdateList(Transform content, UIHeroSlotType type,  HeroInstance selectHero = null, UIHeroSlotMode mode = UIHeroSlotMode.Party)
        {
            if (content == null) return;
            if (heroSlotPrefab == null) return;

            ClearList(content); //기존에 생성되어 있던 슬롯 제거

            switch (type)
            {
                //모든 보유 영웅 표시
                case UIHeroSlotType.All:
                    CreateAll(content, mode);
                    break;
                //선택한 영웅과 같은 등급 표시
                case UIHeroSlotType.SameGrade:
                    if (selectHero == null || selectHero.data == null) return;
                    CreateSameGrade(content, selectHero, mode);
                    break;
                //선택한 영웅과 같은 영웅 표시
                case UIHeroSlotType.SameHero:
                    if (selectHero == null || selectHero.data == null) return;
                    CreateSameHero(content, selectHero, mode);
                    break;
            }
        }

        //슬롯 생성
        private void CreateSlot(Transform content, HeroInstance hero, UIHeroSlotMode mode)
        {
            UIHeroSlot slot = Instantiate(heroSlotPrefab, content);
            slot.SetHero(hero, heroInfoPopup);
            slot.SetMode(mode);
        }

        //슬롯 제거
        private void ClearList(Transform content)
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }

        //모든 보유 영웅 표시
        private void CreateAll(Transform content, UIHeroSlotMode mode)
        {
            List<HeroInstance> heroes = HeroManager.Instance.GetAllHeroes(); //전체 영웅 목록

            foreach (HeroInstance hero in heroes)
            {
                if (hero == null || hero.data == null) continue; //영웅값이 없으면 제외
                if (!hero.isUnlocked) continue;                  //획득하지 않은 영웅 제외
                CreateSlot(content, hero, mode);                 //슬롯생성
            }

        }

        //선택한 영웅과 같은 등급 표시
        private void CreateSameGrade(Transform content, HeroInstance selectHero, UIHeroSlotMode mode)
        {
            List<HeroInstance> heroes = HeroManager.Instance.GetAllHeroes(); //전체 영웅 목록

            foreach (HeroInstance hero in heroes)
            {
                if (hero == null || hero.data == null) continue;                //영웅값이 없으면 제외
                if (!hero.isUnlocked) continue;       //획득하지 않은 영웅 제외
                if (hero == selectHero) continue;     //본인 제외
                if (hero.data.HeroGrade != selectHero.data.HeroGrade) continue; //HeroGrade가 같은지 체크

                CreateSlot(content, hero, mode);      //슬롯생성
            }

        }

        //선택한 영웅과 같은 영웅 표시
        private void CreateSameHero(Transform content, HeroInstance selectHero, UIHeroSlotMode mode)
        {
            List<HeroInstance> heroes = HeroManager.Instance.GetAllHeroes(); //전체 영웅 목록

            foreach (HeroInstance hero in heroes)
            {
                if (hero == null || hero.data == null) continue;                //영웅값이 없으면 제외
                if (!hero.isUnlocked) continue;                                 //획득하지 않은 영웅 제외
                if (hero == selectHero) continue;                               //본인 제외
                if (hero.data.HeroID != selectHero.data.HeroID) continue;       //HeroID가 같은지 체크
                if (hero.data.HeroGrade != selectHero.data.HeroGrade) continue; //등급확인

                CreateSlot(content, hero, mode);      //슬롯생성
            }
        }
        
    }
}