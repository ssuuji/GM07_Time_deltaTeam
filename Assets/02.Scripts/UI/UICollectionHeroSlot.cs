using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class UICollectionHeroSlot : MonoBehaviour
    {
        [SerializeField] private Image heroIconImage; //영웅 이미지
        [SerializeField] private TMP_Text heroNameText; //영웅 이름
        [SerializeField] private GameObject unlock; //미획득 표시

        private HeroData heroData;

        public JobType JobType => heroData.JobType;

        //영웅 카드 설정
        public void SetSlot(HeroData data)
        {
            heroData = data;

            heroNameText.text = heroData.HeroName;
            heroIconImage.sprite = heroData.HeroIcon;

            RefreshSlot();
        }

        //영웅 획득 상태 갱신
        public void RefreshSlot()
        {
            HeroInstance hero = HeroManager.Instance.GetHeroByID(heroData.HeroID);

            if (hero == null) return;

            unlock.SetActive(!hero.isUnlocked);
        }
    }
}