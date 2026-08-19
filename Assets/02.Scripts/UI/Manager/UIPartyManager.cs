using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class UIPartyManager : MonoBehaviour
    {
        public static UIPartyManager Instance { get; private set; }

        [Header("영웅 리스트")]
        [SerializeField] private Transform partyContent;       //영웅리스트 생성위치
        [SerializeField] private UIHeroList heroList;          //영웅리스트

        [Header("파티 배치 슬롯")]
        [SerializeField] private Image[] partyPlaceImages;     //배치자리 이미지
        [SerializeField] private Transform[] partyHeroPrefabs; //영웅 프리펩 배치할 위치
        [SerializeField] private Image partyPlaceBackImage_t;  //배치중 표시할 이미지(주변 어둡게) - 위
        [SerializeField] private Image partyPlaceBackImage_b;  //배치중 표시할 이미지(주변 어둡게) - 아래
        private HeroInstance selectedHero;                     //배치할 영웅저장
        private GameObject[] heroPrefabs;                      //영웅 프리펩

        [Header("파티 배치 삭제")]
        [SerializeField] private GameObject[] removeMarks;     //각 슬롯의 (-) 표시 (배치된 영웅 삭제할 때 바로 삭제되지 않고 한 번 표시할 마크)
        private int selectedRemoveSlotIndex = -1;              //삭제될 슬롯번호 저장

        [Header("시너지")]
        [SerializeField] private UISynergy synergyUI;          //시너지 UI


        private void Awake()
        {
            Instance = this;
            heroPrefabs = new GameObject[partyHeroPrefabs.Length];
        }

        private void Start()
        {
            ClearRemoveSelection(); //(-) 초기화
            UpdatePartyUI();        //UI 갱신
        }

        #region  UI 갱신

        //영웅리스트 갱신
        public void UpdateHeroList()
        {
            if (heroList == null) return;
            if (partyContent == null) return;

            heroList.UpdateList(partyContent, UIHeroSlotType.All, null, UIHeroSlotMode.Party); //현재 보유 중인 모든 영웅 표시
        }

        //UI 갱신
        public void UpdatePartyUI()
        {
            UpdatePartySet();     //파티 배치 UI 갱신
            synergyUI.UpdateUI(); //시너지 UI갱신
        }

        //파티 배치 UI 갱신
        public void UpdatePartySet()
        {
            for (int i = 0; i < partyHeroPrefabs.Length; i++)
            {
                HeroInstance hero = PartyManager.Instance.partySlots[i];

                SetHeroPrefab(i, hero); //영웅 프리펩 표시
            }

            UpdateRemoveMarks();
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

            GameObject heroPrefab = Instantiate(hero.data.HeroPrefab, partyHeroPrefabs[slotIndex]); //프리펩 생성
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

        //배치중 배경 표시 (주변을 어둡게 표시)
        private void SetPlaceBack(bool active)
        {
            partyPlaceBackImage_t.gameObject.SetActive(active);
            partyPlaceBackImage_b.gameObject.SetActive(active);
        }

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

        #endregion

        #region 파티 배치

        //영웅 배치 시작
        public void StartPlaceHero(HeroInstance hero)
        {
            if (hero == null) return;

            ClearRemoveSelection(); //(-) 초기화
            selectedHero = hero;    //배치할 영웅 저장

            UINoticePopup.Instance.Show("배치할 위치를 선택해주세요.");

            SetPlaceBack(true); //주변 어둡게 표시
            SetSlotAlpha(true); //빈자리 UI 표시
        }

        //선택한 위치에 영웅 배치
        private void PlaceHeroToSlot(int slotIndex)
        {
            if (selectedHero == null) return;

            PartyManager.Instance.PlaceHero(slotIndex, selectedHero); //선택한 자리에 영웅 배치
            selectedHero = null;                                      //배치 완료 후 선택 해제

            UINoticePopup.Instance.Hide(); //알림창 닫기
            SetPlaceBack(false);           //배치중 배경 제거
            SetSlotAlpha(false);           //자리 이미지 원래 상태로
            UpdatePartyUI();               //UI갱신
        }

        //영웅 배치 해제
        public void RemoveHero(HeroInstance hero)
        {
            if (hero == null) return;

            PartyManager.Instance.RemoveHero(hero);

            ClearRemoveSelection();
            UpdatePartyUI();
        }

        #endregion

        #region 파티 슬롯 선택

        //파티 슬롯 클릭
        public void OnClickedPartySlot(int slotIndex)
        {
            if (selectedHero != null)
            {
                PlaceHeroToSlot(slotIndex); //영웅 배치 중이라면 선택한 위치에 영웅 배치
                return;
            }

            HeroInstance hero = PartyManager.Instance.partySlots[slotIndex];

            if (selectedRemoveSlotIndex != -1) //이미 (-) 표시된 슬롯이 있는 상태
            {
                //(-) 표시된 같은 슬롯을 다시 클릭 
                if (selectedRemoveSlotIndex == slotIndex)
                {
                    if (hero == null || hero.data == null)
                    {
                        ClearRemoveSelection();
                        return;
                    }

                    PartyManager.Instance.RemoveHero(hero); //배치 해제

                    ClearRemoveSelection();
                    UpdatePartyUI();

                    return;
                }

                //다른 슬롯을 클릭
                ClearRemoveSelection(); //(-) 표시만 해제
                return;
            }

            //빈자리 클릭
            if (hero == null || hero.data == null) return;

            selectedRemoveSlotIndex = slotIndex; //삭제 될 슬롯 번호 저장
            UpdateRemoveMarks();                 //(-) 표시
        }

        //(-) 표시 갱신
        private void UpdateRemoveMarks()
        {
            for (int i = 0; i < removeMarks.Length; i++)
            {
                if (removeMarks[i] == null) continue;

                HeroInstance hero = PartyManager.Instance.partySlots[i];
                bool hasHero = hero != null && hero.data != null;

                removeMarks[i].SetActive(hasHero && i == selectedRemoveSlotIndex); //(-) 표시 활성화
            }
        }

        //슬롯 선택 해제
        public void ClearRemoveSelection()
        {
            selectedRemoveSlotIndex = -1;

            UpdateRemoveMarks();
        }

        #endregion
    }
}