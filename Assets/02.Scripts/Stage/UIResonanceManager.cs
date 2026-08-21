using AFKHero.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

//ResonanceManager 내부에 존재하는 PlaceHero, RemoveHero를 실제로 실행시킬 버튼에 연결할 메서드를 갖는 스크립트
//UIPartyManager를 참고하여 제작하도록 한다.

public class UIResonanceManager : MonoBehaviour
{
    public static UIResonanceManager Instance { get; private set; }

    [Header("영웅 리스트")]
    [SerializeField] private Transform resonanceContent; //공명 쪽 Viewport 연결
    [SerializeField] private UIHeroList heroList; //

    [Header("공명 배치 슬롯")]
    [SerializeField] private Image[] resonancePlaceImages;
    [SerializeField] private Transform[] resonanceHeroPrefabs;
    [SerializeField] private Image resonancePlaceBackImage_t;
    [SerializeField] private Image resonancePlaceBackImage_b;
    private HeroInstance selectedHero;
    private GameObject[] heroPrefabs;

    [Header("공명 배치 삭제")]
    [SerializeField] private GameObject[] removeMarks;
    private int selectedRemoveSlotIndex = -1;


    //흠... 문제는 이 파티클은 공명 창이 열려있을 때만 활성화되어야 할 텐데?
    [Header("공명 시 연출")]
    [SerializeField] private ParticleSystem resonanceParticle;



    //싱글톤과는 다른 구조인 것 같은데, 뭘까.
    private void Awake()
    {
        Instance = this;
        heroPrefabs = new GameObject[resonanceHeroPrefabs.Length];

        //시작 시에는 우선 꺼두는 걸로 할 건데, 저장 데이터 로드 시 일치하지 않는지 확인하기.
        if (resonanceParticle != null)
        {
            resonanceParticle.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        ClearRemoveSelection();
        UpdateResonanceUI();
    }

    //현재 보유한 영웅 리스트를 갱신한다.
    //UIHeroList클래스, UIHeroSlot의 수정이 필요한가?
    //보유한 영웅을 특정 UI에 전부 출력하는 건 공통기능이므로
    //추후에 기능통합을 고려한다.
    public void UpdateHeroList()
    {
        if (heroList == null) return;
        if (resonanceContent == null) return;

        heroList.UpdateList(resonanceContent, UIHeroSlotType.All, null, UIHeroSlotMode.Share);
    }


    public void UpdateResonanceUI()
    {
        UpdateResonanceSet();

        ShowResonanceEffect(ResonanceManager.Instance.IsResonanceOn);
    }

    //공명 상태가 활성화됐을 경우, 어디선가에서 빤짝이 이펙트를 켜고 끄게 할 메서드
    //Play와 Stop이 제대로 사용된 건지 모르겠네.
    //공명 패널을 닫을 때 비활성화되고, 열 때 현재 공명 상태인지를 판별하여 활성화하게끔 해야 함.
    private void ShowResonanceEffect(bool isResonanceOn)
    {
        if (resonanceParticle == null)
        {
            Debug.LogWarning("[UIResonanceManager] : 공명 연출 컴포넌트가 없습니다.");
            return;
        }
        

        if (isResonanceOn)
        {
            Debug.Log("[UIResonanceManager] : 공명이 활성화되어 파티클이 재생됩니다.");
            resonanceParticle.gameObject.SetActive(true);
            resonanceParticle.Play();
            //파티클 켜기

            //어차피 공명이 활성화 되었을 때 표시할 거잖아? 그러면... 여기서 그런 식으로 하는게?
            //resonanceText.text = "현재 공명 레벨 : " + {ResonanceManager.Instance.ResonanceLevel;
        }
        else
        {
            Debug.Log("[UIResonanceManager] : 공명이 비활성화되어 파티클이 꺼집니다.");
            resonanceParticle.gameObject.SetActive(false);
            resonanceParticle.Stop();
            //파티클 끄기

            //resonanceText.text = "현재 공명이 활성화되지 않았습니다."
        }
    }

    public void UpdateResonanceSet()
    {
        for (int i = 0; i < resonanceHeroPrefabs.Length; i++)
        {
            HeroInstance hero = ResonanceManager.Instance.ResonanceSlots[i];

            SetHeroPrefab(i, hero);
        }

        UpdateRemoveMarks();
    }

    //전달받은 영웅 프리펩의 이미지를 슬롯 위치에 표시하게 할 메서드
    private void SetHeroPrefab(int slotIndex, HeroInstance hero)
    {
        if (heroPrefabs[slotIndex] != null) //이미 무언가 배치된 칸이라면
        {
            Destroy(heroPrefabs[slotIndex]); //해당 캐릭터 프리팹을 제거하고
            heroPrefabs[slotIndex] = null; //null로 처리해둔다.
        }

        //인스턴스가 없거나, 데이터를 포함하고 있지 않거나, 데이터 안에 프리팹이 없다면 종료
        if (hero == null || hero.data == null || hero.data.HeroPrefab == null) return;

        //지정된 위치에 영웅을 생성하고
        GameObject heroPrefab = Instantiate(hero.data.HeroPrefab, resonanceHeroPrefabs[slotIndex]);
        //슬롯에 생성한 영웅을 설정한다.
        heroPrefabs[slotIndex] = heroPrefab;

        //위치, 크기, 회전값을 이 곳에서 설정한다.
        //원본 프리팹과 다른 크기로 배치하시려고 하셔서 이와 같은 방식으로 구현하신 듯
        heroPrefab.transform.localPosition = Vector3.zero;
        heroPrefab.transform.localRotation = Quaternion.identity;
        heroPrefab.transform.localScale = Vector3.one * 150f;

        //UI에 가려지는 걸 방지하기 위해 SortingGroup을 사용하신 듯
        SortingGroup sortingGroup = heroPrefab.GetComponentInChildren<SortingGroup>();

        
        if (sortingGroup != null)
        {
            //레이어 이름과 정렬 순서를 정하는 것 같은데 정확히 확인 필요
            sortingGroup.sortingLayerName = "UI";
            sortingGroup.sortingOrder = 10;
        }

    }

    private void SetPlaceBack(bool active)
    {
        resonancePlaceBackImage_t.gameObject.SetActive(active);
        resonancePlaceBackImage_b.gameObject.SetActive(active);

    }

    private void SetSlotAlpha(bool placing)
    {
        for (int i = 0; i < resonancePlaceImages.Length; i++)
        {
            if (resonancePlaceImages[i] == null) continue;

            Color32 color = resonancePlaceImages[i].color;

            HeroInstance hero = ResonanceManager.Instance.ResonanceSlots[i];

            bool isEmpty = hero == null || hero.data == null;

            if (placing & isEmpty)
            {
                color.a = 225;
            }
            else
            {
                color.a = 100;
            }

            resonancePlaceImages[i].color = color;
        }
    }

    //영웅 배치
    public void StartPlaceHero(HeroInstance hero)
    {
        if (hero == null) return;

        ClearRemoveSelection(); 
        selectedHero = hero;

        UINoticePopup.Instance.Show("배치할 위치를 선택해주세요.");

        SetPlaceBack(true);
        SetSlotAlpha(true);
        
    }

    private void PlaceHeroToSlot(int slotIndex)
    {
        if (selectedHero == null) return;

        ResonanceManager.Instance.PlaceHero(slotIndex, selectedHero);
        selectedHero = null;

        UINoticePopup.Instance.Hide();
        SetPlaceBack(false);
        SetSlotAlpha(false);
        UpdateResonanceUI();
    }

    public void RemoveHero(HeroInstance hero)
    {
        if (hero == null) return;

        ResonanceManager.Instance.RemoveHero(hero);

        ClearRemoveSelection();
        UpdateResonanceUI();
    }

    //참조 0이던데, 버튼에 연결하신 듯?
    //각 슬롯의 버튼에 연결되어, 영웅이 슬롯에 들어있다면 빼게 할 메서드.
    public void OnClickedResonanceSlot(int slotIndex)
    {
        if (selectedHero != null)
        {
            PlaceHeroToSlot(slotIndex);
            return;
        }

        HeroInstance hero = ResonanceManager.Instance.ResonanceSlots[slotIndex];

        if (selectedRemoveSlotIndex != -1)
        {
            if (selectedRemoveSlotIndex == slotIndex)
            {
                if (hero == null || hero.data == null)
                {
                    ClearRemoveSelection();
                    return;
                }

                ResonanceManager.Instance.RemoveHero(hero);

                ClearRemoveSelection();
                UpdateResonanceUI();
                return;
            }

            ClearRemoveSelection();
            return;
        }

        if (hero == null || hero.data == null) return;

        selectedRemoveSlotIndex = slotIndex;
        UpdateRemoveMarks();

    }

    private void UpdateRemoveMarks()
    {
        for (int i = 0; i < removeMarks.Length; i++)
        {
            if (removeMarks[i] == null) continue;

            HeroInstance hero = ResonanceManager.Instance.ResonanceSlots[i];

            bool hasHero = hero != null && hero.data != null;

            removeMarks[i].SetActive(hasHero && i == selectedRemoveSlotIndex);
        }
    }

    public void ClearRemoveSelection()
    {
        selectedRemoveSlotIndex = -1;

        UpdateRemoveMarks();
    }
}
