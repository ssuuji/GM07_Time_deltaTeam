using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [Header("파티 슬롯 (0~1: 전열 / 2~4: 후열)")]
    public HeroInstance[] partySlots = new HeroInstance[5];

    [Header("활성화된 시너지 보너스")]
    public float totalBonusAttackRate = 0f;
    public float totalBonusHpRate = 0f;
    public List<string> activeSynergyDescriptions = new List<string>();

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

    // ============================
    // 파티 슬롯 배치 로직
    // ============================
    public void PlaceHero(int slotIndex, HeroInstance hero)
    {
        if (slotIndex < 0 || slotIndex >= 5) return;

        // 중복 배치 방지 규칙 구현
        for (int i = 0; i < 5; i++)
        {
            if (partySlots[i] == hero) partySlots[i] = null;
        }

        partySlots[slotIndex] = hero;
        UpdateSynergy();
    }

    // ==========================
    // 종족 시너지 계산 로직
    // ==========================
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
            if (raceCounts.ContainsKey(race)) raceCounts[race]++;
            else raceCounts[race] = 1;
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

    // =========================================
    // 세이브 데이터를 위한 데이터 연동 함수
    // =========================================

    public int[] GetPartySaveData()
    {
        int[] savedIDs = new int[5];
        for (int i = 0; i < 5; i++)
        {
            savedIDs[i] = (partySlots[i] != null) ? partySlots[i].data.HeroID : -1;
        }
        return savedIDs;
    }

    public void LoadPartyFromData(int[] savedIDs)
    {
        if (savedIDs == null || savedIDs.Length != 5) return;

        for (int i = 0; i < 5; i++)
        {
            int heroID = savedIDs[i];

            if (heroID != -1)
            {
                HeroInstance hero = HeroManager.Instance.GetHeroByID(heroID);
                if (hero != null && hero.isUnlocked)
                {
                    partySlots[i] = hero;
                }
            }
            else
            {
                partySlots[i] = null;
            }
        }
        UpdateSynergy();
    }
}