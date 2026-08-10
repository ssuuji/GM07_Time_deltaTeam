using UnityEngine;

namespace AFKHero.UI
{
    public class UIPartyManager : MonoBehaviour
    {
        public static UIPartyManager Instance { get; private set; }

        [Header("영웅 리스트")]
        [SerializeField] private Transform partyContent; //viewport 에 있는 content 연결
        [SerializeField] private UIHeroList heroList;

        private void Awake()
        {
            Instance = this;
        }

        public void UpdateHeroList()
        {
            if (heroList == null) return;
            if (partyContent == null) return;

            // 현재 보유 중인 모든 영웅 표시
            heroList.UpdateList(partyContent, UIHeroSlotType.All);
        }
    }
}

