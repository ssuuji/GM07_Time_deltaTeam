using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    public List<EquipmentInstance> equipmentInventory = new List<EquipmentInstance>();

    //0901 추가부분
    //EquipmentManager의 내부 배열에 만드신 장비들을 전부 등록하셔야 합니다.
    //LoadAll 메서드는 Resources 폴더 내부에 있는 것들만 찾아오기에 사용할 수 없습니다.
    public EquipmentData[] allEquipments;


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

    // ==========================
    // 개별 장비 판매
    // ==========================
    public void SellEquipment(EquipmentInstance itemToSell)
    {
        // 가방에 없는 아이템이거나 비어있으면 취소
        if (itemToSell == null || !equipmentInventory.Contains(itemToSell)) return;

        int earnedGold = 0;

        // 등급에 따라 골드 차등 지급 (에픽 1000, 레어 300, 노말 100)
        if (itemToSell.Grade == EquipmentGrade.Epic) earnedGold = 1000;
        else if (itemToSell.Grade == EquipmentGrade.Rare) earnedGold = 300;
        else earnedGold = 100;

        // 플레이어에게 골드 지급
        if (AFKHero.Player.PlayerManager.Instance != null)
        {
            AFKHero.Player.PlayerManager.Instance.AddGold(earnedGold);
        }

        // 가방에서 해당 아이템만 빼서 삭제
        equipmentInventory.Remove(itemToSell);

        Debug.Log($"<color=orange>[장비 판매]</color> {itemToSell.BaseData.equipmentName}을(를) 팔고 {earnedGold}골드를 얻었습니다!");
    }

    // ==========================
    // 다중 선택 장비 판매 
    // ==========================
    public void SellSelectedEquipments(List<EquipmentInstance> itemsToSell)
    {
        if (itemsToSell == null || itemsToSell.Count == 0) return;

        int totalEarnedGold = 0;
        int sellCount = 0;

        foreach (var item in itemsToSell)
        {
            if (equipmentInventory.Contains(item))
            {
                // 등급에 따른 골드 합산
                if (item.Grade == EquipmentGrade.Epic) totalEarnedGold += 1000;
                else if (item.Grade == EquipmentGrade.Rare) totalEarnedGold += 300;
                else totalEarnedGold += 100;

                equipmentInventory.Remove(item);
                sellCount++;
            }
        }

        // 플레이어에게 총 골드 지급
        if (AFKHero.Player.PlayerManager.Instance != null)
        {
            AFKHero.Player.PlayerManager.Instance.AddGold(totalEarnedGold);
        }

        Debug.Log($"<color=orange>[선택 판매]</color> 장비 {sellCount}개를 팔고 {totalEarnedGold}골드를 얻었습니다!");
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
        if (equipmentID.Contains("ImmortalSet")) return "ImmortalSet";
        return "";
    }

    // 2세트 이상 맞춰질 때만 세트 점수
    private float CalculateEquipScore(EquipmentInstance equip, int maxPossibleSetCount)
    {
        float score = 0;

        // 1순위 : 장비의 순수 등급 점수
        if (equip.Grade == EquipmentGrade.Epic) score += 50000;
        else if (equip.Grade == EquipmentGrade.Rare) score += 20000;
        else score += 5000;

        // 2순위 : 세트 효과 점수
        if (maxPossibleSetCount >= 2)
        {
            string equipName = equip.BaseData.equipmentID;
            float setBonus = 0;

            if (equipName.Contains("VampSet")) setBonus = 5000;
            else if (equipName.Contains("ComboSet")) setBonus = 4000;
            else if (equipName.Contains("EvadeSet")) setBonus = 3000;
            else if (equipName.Contains("ExecuteSet")) setBonus = 2000;
            else if (equipName.Contains("ReviveSet")) setBonus = 1000;
            else if (equipName.Contains("ImmortalSet")) setBonus = 1500;

            // 만약 4세트 풀셋이 가능하다면? 세트 가중치를 2배로 줘서 풀셋을 우선적으로 장착하도록 유도
            if (maxPossibleSetCount >= 4) setBonus *= 2;

            score += setBonus;
        }

        // 3순위 : 깡스탯 점수
        score += equip.HP * 1f;
        score += equip.Attack * 5f;
        score += equip.Defense * 4f;

        return score;
    }

    // ===============================
    // 장비 세이브 / 로드 시스템
    // ===============================
    public EquipmentSaveData CreateEquipmentSaveData()
    {
        EquipmentSaveData saveData = new EquipmentSaveData();

        foreach (var equip in equipmentInventory)
        {
            EquipItemSaveData itemData = new EquipItemSaveData();
            itemData.equipmentID = equip.BaseData.equipmentID;
            itemData.grade = (int)equip.Grade;
            itemData.enhanceLevel = equip.EnhanceLevel;
            itemData.attack = equip.Attack;
            itemData.defense = equip.Defense;
            itemData.hp = equip.HP;

            saveData.inventoryEquips.Add(itemData);
        }

        return saveData;
    }

    public void LoadEquipmentSaveData(EquipmentSaveData saveData)
    {
        if (saveData == null) return;

        equipmentInventory.Clear();

        foreach (var itemData in saveData.inventoryEquips)
        {
            // 수정 : Resources.Load 대신, 미리 등록된 allEquipments 배열에서 장비찾기
            EquipmentData baseData = null;

            foreach (var equipData in allEquipments)
            {
                if (equipData.equipmentID == itemData.equipmentID)
                {
                    baseData = equipData;
                    break;
                }
            }

            // 원본 데이터를 찾지 못했다면 에러를 띄우고 건너뜐다.
            if (baseData == null)
            {
                Debug.LogWarning($"[장비 로드 실패] 배열에서 '{itemData.equipmentID}'(을)를 찾을 수 없습니다. EquipmentManager 프리팹의 allEquipments 배열에 잘 등록되었는지 확인해주세요!");
                continue;
            }

            // 찾은 원본 데이터를 넣어서 장비를 생성
            EquipmentInstance newEquip = new EquipmentInstance(baseData);

            // 새로 생성되면서 랜덤 부여된 스탯들을, 저장되어 있던 기존 스탯으로 덮기
            newEquip.Grade = (EquipmentGrade)itemData.grade;
            newEquip.EnhanceLevel = itemData.enhanceLevel;
            newEquip.Attack = itemData.attack;
            newEquip.Defense = itemData.defense;
            newEquip.HP = itemData.hp;

            equipmentInventory.Add(newEquip);
        }
    }

    // ===============================
    // 랜덤장비 지급하는 메서드
    // ===============================
    public void GiveRandomEquipment()
    {
        //정상적으로 모든 장비가 들어있는 배열이 셋팅됐다면
        if (allEquipments.Length > 0)
        {
            //0부터 배열 길이 중 랜덤 숫자 하나 뽑아서
            int randomIndex = UnityEngine.Random.Range(0, allEquipments.Length);

            //데이터 생성한 다음
            EquipmentData randomData = allEquipments[randomIndex];

            //장비를 지급
            AddEquipment(new EquipmentInstance(randomData));
            Debug.Log($"{randomData.equipmentName} 획득!");
        }
        else
        {
            Debug.LogWarning("EquipmentManager : allEquipments 배열에 장비가 등록되지 않아 지급할 수 없습니다");
            return;
        }
    }
}