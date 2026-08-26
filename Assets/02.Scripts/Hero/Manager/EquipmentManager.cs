using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    public List<EquipmentInstance> equipmentInventory = new List<EquipmentInstance>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 넘어가도 가방이 파괴되지 않게 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 몬스터를 잡고 만들어진 '진짜 장비'를 가방에 넣기
    public void AddEquipment(EquipmentInstance newEquipment)
    {
        if (newEquipment == null) return;
        equipmentInventory.Add(newEquipment);
    }

    //  영웅에게 장비를 입히는 로직
    public void EquipToHero(HeroInstance hero, EquipmentInstance equipmentToEquip)
    {
        if (hero == null || equipmentToEquip == null) return;
        if (!equipmentInventory.Contains(equipmentToEquip)) return;

        EquipmentInstance existingEquip = null;

        switch (equipmentToEquip.BaseData.type)
        {
            case EquipmentType.Weapon: existingEquip = hero.equippedWeapon; break;
            case EquipmentType.Armor: existingEquip = hero.equippedArmor; break;
            case EquipmentType.Pants: existingEquip = hero.equippedPants; break;
            case EquipmentType.Helmet: existingEquip = hero.equippedHelmet; break;
        }

        // 이미 끼고 있는 장비가 있다면 가방으로 다시 돌려보내기
        if (existingEquip != null) equipmentInventory.Add(existingEquip);

        // 새 장비를 영웅에게 입히고, 가방에서는 삭제
        hero.EquipItem(equipmentToEquip);
        equipmentInventory.Remove(equipmentToEquip);
    }

    // 4개 부위를 순사하며 가장 좋은 장비를 자동으로 찾아 장착
    public void AutoEquip(HeroInstance hero)
    {
        if (hero == null) return;
        EquipBestItemForType(hero, EquipmentType.Weapon);
        EquipBestItemForType(hero, EquipmentType.Armor);
        EquipBestItemForType(hero, EquipmentType.Pants);
        EquipBestItemForType(hero, EquipmentType.Helmet);
    }

    // 특정 부위에서 스탯 총합이 가장 높은 장비를 찾는 함수
    private void EquipBestItemForType(HeroInstance hero, EquipmentType type)
    {
        EquipmentInstance bestItem = null;
        int maxStat = -1;

        // 가방을 다 뒤져서 스탯 총합(공+방+체)이 가장 높은 녀석을 찾습니다.
        foreach (var item in equipmentInventory)
        {
            if (item.BaseData.type == type)
            {
                int itemStat = item.Attack + item.Defense + item.HP; // 랜덤으로 확정된 진짜 스탯 사용
                if (itemStat > maxStat)
                {
                    maxStat = itemStat;
                    bestItem = item;
                }
            }
        }

        EquipmentInstance currentEquip = null;
        switch (type)
        {
            case EquipmentType.Weapon: currentEquip = hero.equippedWeapon; break;
            case EquipmentType.Armor: currentEquip = hero.equippedArmor; break;
            case EquipmentType.Pants: currentEquip = hero.equippedPants; break;
            case EquipmentType.Helmet: currentEquip = hero.equippedHelmet; break;
        }

        int currentStat = currentEquip != null ? (currentEquip.Attack + currentEquip.Defense + currentEquip.HP) : -1;

        // 찾은 장비가 지금 끼고 있는 것보다 좋으면 교체
        if (bestItem != null && maxStat > currentStat) EquipToHero(hero, bestItem);
    }

    // 가방에 있는 모든 장비를 팔고 등급에 따라 골드 지급
    public void BulkSell()
    {
        if (equipmentInventory.Count == 0) return;

        int totalEarnedGold = 0;
        foreach (var item in equipmentInventory)
        {
            // 에픽은 1000골드, 레어는 300골드, 노말은 100골드로 차등 지급
            if (item.Grade == EquipmentGrade.Epic) totalEarnedGold += 1000;
            else if (item.Grade == EquipmentGrade.Rare) totalEarnedGold += 300;
            else totalEarnedGold += 100;
        }

        if (AFKHero.Player.PlayerManager.Instance != null)
        {
            AFKHero.Player.PlayerManager.Instance.AddGold(totalEarnedGold);
        }

        equipmentInventory.Clear(); // 판 뒤에는 가방을 싹 비웁니다.
    }
    // ===============================
    // 스마트 일괄 장착 시스템
    // ===============================

    public void AutoEquipHero(HeroInstance hero)
    {
        if (hero == null) return;

        EquipmentType[] slotTypes = { EquipmentType.Weapon, EquipmentType.Armor, EquipmentType.Helmet, EquipmentType.Pants };
        bool isEquippedAnything = false;

        // 영웅이 낄 수 있는 모든 장비 풀을 하나로 모으기
        List<EquipmentInstance> allAvailable = new List<EquipmentInstance>(equipmentInventory);
        if (hero.equippedWeapon != null) allAvailable.Add(hero.equippedWeapon);
        if (hero.equippedArmor != null) allAvailable.Add(hero.equippedArmor);
        if (hero.equippedHelmet != null) allAvailable.Add(hero.equippedHelmet);
        if (hero.equippedPants != null) allAvailable.Add(hero.equippedPants);

        // 가방을 스캔해서, 어떤 세트를 몇 부위나 맞출 수 있는지 미리 파악
        Dictionary<string, HashSet<EquipmentType>> possibleSetParts = new Dictionary<string, HashSet<EquipmentType>>();

        foreach (var eq in allAvailable)
        {
            string setName = GetSetName(eq.BaseData.equipmentID);
            if (string.IsNullOrEmpty(setName)) continue;

            if (!possibleSetParts.ContainsKey(setName))
                possibleSetParts[setName] = new HashSet<EquipmentType>();

            // HashSet을 사용하여 중복 부위(예: 무기만 2개)는 1부위로 취급
            possibleSetParts[setName].Add(eq.BaseData.type);
        }

        // 부위별로 가장 점수가 높은 최고의 장비를 찾아 장착
        foreach (var type in slotTypes)
        {
            List<EquipmentInstance> availableForSlot = allAvailable.FindAll(e => e.BaseData.type == type);
            if (availableForSlot.Count == 0) continue;

            EquipmentInstance currentEquip = hero.GetEquippedItem(type);
            EquipmentInstance bestEquip = null;
            float highestScore = -1f;

            foreach (var equip in availableForSlot)
            {
                // 점수 계산 시, 이 장비의 세트가 총 몇 부위까지 완성될 수 있는지 정보를 넘김
                string setName = GetSetName(equip.BaseData.equipmentID);
                int maxPossibleSetCount = string.IsNullOrEmpty(setName) ? 0 : possibleSetParts[setName].Count;

                float score = CalculateEquipScore(equip, maxPossibleSetCount);
                if (score > highestScore)
                {
                    highestScore = score;
                    bestEquip = equip;
                }
            }

            // 가장 좋은 장비가 지금 끼고 있는 것과 다르다면 새것으로 교체
            if (bestEquip != null && bestEquip != currentEquip)
            {
                EquipToHero(hero, bestEquip);
                isEquippedAnything = true;
            }
        }

        if (isEquippedAnything) Debug.Log($"<color=green>[일괄 장착]</color> {hero.data.HeroName}에게 최적의 세팅을 장착했습니다!");
        else Debug.Log($"<color=yellow>[일괄 장착]</color> {hero.data.HeroName}은(는) 이미 최적의 세팅입니다.");
    }

    // 장비 ID에서 세트 이름을 쉽게 뽑아내는 헬퍼 함수
    private string GetSetName(string equipmentID)
    {
        if (equipmentID.Contains("VampSet")) return "VampSet";
        if (equipmentID.Contains("ComboSet")) return "ComboSet";
        if (equipmentID.Contains("EvadeSet")) return "EvadeSet";
        if (equipmentID.Contains("ExecuteSet")) return "ExecuteSet";
        if (equipmentID.Contains("ReviveSet")) return "ReviveSet";
        return "";
    }

    // 2세트 이상 맞춰질 때만 세트 점수
    private float CalculateEquipScore(EquipmentInstance equip, int maxPossibleSetCount)
    {
        float score = 0;

        // [1순위] 장비의 순수 등급 점수
        if (equip.Grade == EquipmentGrade.Epic) score += 50000;
        else if (equip.Grade == EquipmentGrade.Rare) score += 20000;
        else score += 5000;

        // [2순위] 세트 효과 점수 (이 세트가 2부위 이상 맞춰질 수 있을 때만 점수)
        if (maxPossibleSetCount >= 2)
        {
            string equipName = equip.BaseData.equipmentID;
            float setBonus = 0;

            if (equipName.Contains("VampSet")) setBonus = 5000;
            else if (equipName.Contains("ComboSet")) setBonus = 4000;
            else if (equipName.Contains("EvadeSet")) setBonus = 3000;
            else if (equipName.Contains("ExecuteSet")) setBonus = 2000;
            else if (equipName.Contains("ReviveSet")) setBonus = 1000;

            // 만약 4세트 풀셋이 가능하다면? 세트 가중치를 2배로 줘서 풀셋을 우선적으로 장착하도록 유도
            if (maxPossibleSetCount >= 4) setBonus *= 2;

            score += setBonus;
        }

        // [3순위] 깡스탯 점수 (세트를 못 맞추면 위 2순위 점수는 0점이 되고, 오직 이 스탯 점수로만 비교)
        score += equip.HP * 1f;
        score += equip.Attack * 5f;
        score += equip.Defense * 4f;

        return score;
    }
}