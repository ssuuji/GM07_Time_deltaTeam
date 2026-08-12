using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class UIPartyManager : MonoBehaviour
    {
        public static UIPartyManager Instance { get; private set; }

        [Header("영웅 리스트")]
        [SerializeField] private Transform partyContent;   //영웅리스트 생성위치
        [SerializeField] private UIHeroList heroList;      //영웅리스트

        [Header("파티 배치 슬롯")]
        [SerializeField] private Image[] partyPlaceImages; //배치자리 이미지
        [SerializeField] private Image[] partyHeroImages;  //배치된 영웅 이미지

        [Header("시너지")]
        [SerializeField] private UISynergy synergyUI;      //시너지 UI

        private HeroInstance selectedHero;                 //배치할 영웅


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            UpdatePartySet();     //파티 UI 갱신
            synergyUI.UpdateUI(); //시너지 UI 갱신
        }

        //영웅리스트 갱신
        public void UpdateHeroList()
        {
            if (heroList == null) return;
            if (partyContent == null) return;

            heroList.UpdateList(partyContent, UIHeroSlotType.All, null, UIHeroSlotMode.Party); //현재 보유 중인 모든 영웅 표시
        }

        #region 파티 배치

        //영웅 배치 시작
        public void StartPlaceHero(HeroInstance hero)
        {
            if (hero == null) return;

            selectedHero = hero;                                        //배치할 영웅 저장

            UINoticePopup.Instance.Show("배치할 위치를 선택해주세요.");

            SetSlotAlpha(true);                                         //빈자리 UI표시용
        }

        //영웅 배치 해제
        public void RemoveHero(HeroInstance hero)
        {
            if (hero == null) return;

            PartyManager.Instance.RemoveHero(hero); //배치 해제

            UpdatePartySet();                       //파티 UI 갱신
            synergyUI.UpdateUI();                   //시너지 UI 갱신
        }

        //파티 슬롯 선택
        public void OnClickedPartySlot(int slotIndex)
        {
            if (selectedHero == null) return;

            PartyManager.Instance.PlaceHero(slotIndex, selectedHero); //선택한 자리에 영웅 배치
            selectedHero = null;                                      //배치 완료 후 선택 해제

            UINoticePopup.Instance.Hide();                            //알림창 닫기
            SetSlotAlpha(false);                                      //자리 이미지 원래 상태로
            UpdatePartySet();                                         //파티 UI 갱신
            synergyUI.UpdateUI();                                     //시너지 UI갱신
        }

        //파티 배치 UI 갱신
        public void UpdatePartySet()
        {
            for (int i = 0; i < partyHeroImages.Length; i++)
            {
                HeroInstance hero = PartyManager.Instance.partySlots[i];

                if (hero == null || hero.data == null)
                {
                    partyHeroImages[i].gameObject.SetActive(false);       //빈자리 숨기기
                    continue;
                }

                partyHeroImages[i].gameObject.SetActive(true);
                partyHeroImages[i].sprite = hero.data.HeroIcon;           //배치된 영웅 표시
            }
        }

        #endregion

        //빈자리 UI 표시용
        private void SetSlotAlpha(bool placing)
        {
            for (int i = 0; i < partyPlaceImages.Length; i++)
            {
                if (partyPlaceImages[i] == null) continue;

                Color32 color = partyPlaceImages[i].color;

                HeroInstance hero = PartyManager.Instance.partySlots[i];

                //영웅이 없거나 HeroData가 없으면 빈 자리
                bool isEmpty = hero == null || hero.data == null;
                if (placing && isEmpty)
                {
                    color.a = 225; //알파값 높여서 진하게 (나중에 빈자리 반짝이게하는,, 느낌으로 ? 변경하면 좋을듯)
                }
                else
                {
                    color.a = 100;
                }

                partyPlaceImages[i].color = color;
            }
        }
    }
}

