using UnityEngine;
using UnityEngine.Rendering;
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
        [SerializeField] private Image[] partyPlaceImages;     //배치자리 이미지
        [SerializeField] private Transform[] partyHeroPrefabs; //영웅 프리펩 배치할 위치
        [SerializeField] private Image partyPlaceBackImage_t;  //배치중 표시할 이미지
        [SerializeField] private Image partyPlaceBackImage_b;  //배치중 표시할 이미지

        [Header("시너지")]
        [SerializeField] private UISynergy synergyUI;      //시너지 UI

        private HeroInstance selectedHero;                 //배치할 영웅
        private GameObject[] heroPrefabs;                  //영웅 프리펩


        private void Awake()
        {
            Instance = this;

            heroPrefabs = new GameObject[partyHeroPrefabs.Length];
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

            SetPlaceBack(true);                                         //주변 어둡게 표시
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
            if (selectedHero != null)
            {
                PartyManager.Instance.PlaceHero(slotIndex, selectedHero); //선택한 자리에 영웅 배치
                selectedHero = null;                                      //배치 완료 후 선택 해제

                UINoticePopup.Instance.Hide();                            //알림창 닫기
                SetPlaceBack(false);                                      //배치중 배경 제거
                SetSlotAlpha(false);                                      //자리 이미지 원래 상태로
            }
            else
            {
                HeroInstance hero = PartyManager.Instance.partySlots[slotIndex]; //임시로 배치되어있는 파티영웅 클릭하면 바로 해제
                PartyManager.Instance.RemoveHero(hero);
            }

            UpdatePartySet();                                         //파티 UI 갱신
            synergyUI.UpdateUI();                                     //시너지 UI갱신
        }

        //파티 배치 UI 갱신
        public void UpdatePartySet()
        {
            for (int i = 0; i < partyHeroPrefabs.Length; i++)
            {
                HeroInstance hero = PartyManager.Instance.partySlots[i];

                SetHeroPrefab(i, hero);
            }
        }

        //영웅 프리펩 표시
        private void SetHeroPrefab(int slotIndex, HeroInstance hero)
        {
            
            if (heroPrefabs[slotIndex] != null) //기존 프리펩 제거
            {
                Destroy(heroPrefabs[slotIndex]);
                heroPrefabs[slotIndex] = null;
            }
            if (hero == null || hero.data == null || hero.data.HeroPrefab == null) return; //빈자리

            GameObject heroPrefab = Instantiate(hero.data.HeroPrefab, partyHeroPrefabs[slotIndex]); //프리팹 생성
            heroPrefabs[slotIndex] = heroPrefab;

            //위치 및 크기
            heroPrefab.transform.localPosition = Vector3.zero;
            heroPrefab.transform.localRotation = Quaternion.identity;
            heroPrefab.transform.localScale = Vector3.one * 150f;

            //UI 위에 표시
            SortingGroup sortingGroup = heroPrefab.GetComponentInChildren<SortingGroup>();

            if (sortingGroup != null)
            {
                sortingGroup.sortingLayerName = "UI";
                sortingGroup.sortingOrder = 10;
            }

        }

        //배치중 배경 표시
        private void SetPlaceBack(bool active)
        {
            partyPlaceBackImage_t.gameObject.SetActive(active);
            partyPlaceBackImage_b.gameObject.SetActive(active);
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

