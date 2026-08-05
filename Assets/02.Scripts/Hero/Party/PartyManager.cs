using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [Header("파티 슬롯 (0~1: 전열 / 2~4: 후열)")]
    // 5칸짜리 고정 배열로 변경 비어있는 슬롯은 null 상태가 됩니다.
    public HeroInstance[] partySlots = new HeroInstance[5];

    [Header("활성화된 시너지 보너스")]
    public float totalBonusAttackRate = 0f;
    public float totalBonusHpRate = 0f;
    public List<string> activeSynergyDescriptions = new List<string>();

    // 로컬 저장을 위한 키값
    private const string PARTY_SAVE_KEY = "PartySaveData";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 게임 시작 시 이전에 저장해둔 파티를 자동으로 불러오기
        LoadParty();
    }

    // ==========================
    // 파티 슬롯 배치 로직
    // ==========================

    // 특정 슬롯에 영웅을 배치합니다.
    public void PlaceHero(int slotIndex, HeroInstance hero)
    {
        if (slotIndex < 0 || slotIndex >= 5) return;

        // 중복 배치 방지 - 이미 다른 슬롯에 이 영웅이 있다면 그 슬롯을 비우기
        for (int i = 0; i < 5; i++)
        {
            if (partySlots[i] == hero)
            {
                partySlots[i] = null;
            }
        }

        // 지정한 슬롯에 영웅 배치
        partySlots[slotIndex] = hero;
        UpdateSynergy();
        SaveParty(); // 파티가 변경될 때마다 자동 저장
    }

    // 특정 슬롯의 영웅을 해제합니다.
    public void RemoveHeroFromSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < 5)
        {
            partySlots[slotIndex] = null;
            UpdateSynergy();
            SaveParty();
        }
    }

    // 두 슬롯의 영웅 위치를 교환
    public void SwapHero(int slotIndex1, int slotIndex2)
    {
        if (slotIndex1 < 0 || slotIndex1 >= 5 || slotIndex2 < 0 || slotIndex2 >= 5) return;

        HeroInstance temp = partySlots[slotIndex1];
        partySlots[slotIndex1] = partySlots[slotIndex2];
        partySlots[slotIndex2] = temp;

        // 시너지 수치는 같겠지만 데이터가 변했으므로 저장
        SaveParty();
    }

    // ========================
    // 저장 및 불러오기
    // ========================

    public void SaveParty()
    {
        // 각 슬롯에 있는 영웅의 ID를 콤마(,)로 연결하여 문자열로 만듭니다. (예: "1,3,-1,5,2")
        // 비어있는 슬롯은 -1로 저장
        string saveData = "";
        for (int i = 0; i < 5; i++)
        {
            if (partySlots[i] != null && partySlots[i].data != null)
                saveData += partySlots[i].data.HeroID.ToString();
            else
                saveData += "-1";

            if (i < 4) saveData += ",";
        }

        PlayerPrefs.SetString(PARTY_SAVE_KEY, saveData);
        PlayerPrefs.Save();
        Debug.Log($"[PartyManager] 파티 저장 완료: {saveData}");
    }

    public void LoadParty()
    {
        string saveData = PlayerPrefs.GetString(PARTY_SAVE_KEY, "");
        if (string.IsNullOrEmpty(saveData)) return; // 저장된 데이터가 없으면 패스

        string[] heroIDs = saveData.Split(',');
        for (int i = 0; i < heroIDs.Length; i++)
        {
            if (i >= 5) break;

            int id = int.Parse(heroIDs[i]);
            if (id != -1)
            {
                // HeroManager에서 ID로 영웅 정보를 찾아와서 슬롯에 넣기
                HeroInstance hero = HeroManager.Instance.GetHeroByID(id);
                if (hero != null && hero.isUnlocked)
                {
                    partySlots[i] = hero;
                }
            }
        }
        UpdateSynergy(); // 불러오고 나서 시너지 갱신
        Debug.Log("[PartyManager] 파티 불러오기 완료!");
    }

    // ==============================
    // 종족 기반 파티 시너지 계산
    // ==============================
    public void UpdateSynergy()
    {
        totalBonusAttackRate = 0f;
        totalBonusHpRate = 0f;
        activeSynergyDescriptions.Clear();

        Dictionary<RaceType, int> raceCounts = new Dictionary<RaceType, int>();

        foreach (var hero in partySlots)
        {
            if (hero == null || hero.data == null) continue;

            RaceType race = hero.data.RaceType;
            if (raceCounts.ContainsKey(race))
                raceCounts[race]++;
            else
                raceCounts[race] = 1;
        }

        foreach (var kvp in raceCounts)
        {
            RaceType race = kvp.Key;
            int count = kvp.Value;

            if (count >= 3)
            {
                totalBonusAttackRate += 0.15f;
                totalBonusHpRate += 0.15f;
                activeSynergyDescriptions.Add($"{race} 3명: 파티 전체 공격력 15%, 체력 15% 증가");
            }
            else if (count == 2)
            {
                totalBonusAttackRate += 0.10f;
                activeSynergyDescriptions.Add($"{race} 2명: 파티 전체 공격력 10% 증가");
            }
        }
    }
}