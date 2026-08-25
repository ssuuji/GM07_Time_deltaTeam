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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 장비 획득 (가방에 추가)
    public void AddEquipment(EquipmentInstance newEquipment)
    {
        if (newEquipment == null) return;

        equipmentInventory.Add(newEquipment);

        Debug.Log($"[EquipmentManager] {newEquipment.BaseData.equipmentName} 획득! (등급: {newEquipment.Grade})");
    }

    // 영웅에게 장비 착용 시도
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

        // 이미 영웅이 해당 부위에 장비를 끼고 있다면, 가방으로 반환
        if (existingEquip != null)
        {
            equipmentInventory.Add(existingEquip);
            Debug.Log($"[EquipmentManager] 기존 장비 {existingEquip.BaseData.equipmentName} 탈착 후 가방으로 반환");
        }

        // 새 장비 장착 후 인벤토리에서 제거
        hero.EquipItem(equipmentToEquip);
        equipmentInventory.Remove(equipmentToEquip);
    }

    // =========================
    // 간편 장착 시스템
    // =========================
    public void AutoEquip(HeroInstance hero)
    {
        if (hero == null) return;

        // 4가지 부위를 순회하며 가장 좋은 장비를 찾아 장착
        EquipBestItemForType(hero, EquipmentType.Weapon);
        EquipBestItemForType(hero, EquipmentType.Armor);
        EquipBestItemForType(hero, EquipmentType.Pants);
        EquipBestItemForType(hero, EquipmentType.Helmet);

        string heroName = (hero.data != null) ? hero.data.HeroName : "알 수 없는 영웅";
        Debug.Log($"[{heroName}] 간편 장착 완료!");
    }

    private void EquipBestItemForType(HeroInstance hero, EquipmentType type)
    {
        EquipmentInstance bestItem = null;
        int maxStat = -1;

        // 인벤토리에서 해당 부위 장비 중 합산 스탯이 가장 높은 장비 찾기
        foreach (var item in equipmentInventory)
        {
            if (item.BaseData.type == type)
            {
                int itemStat = item.Attack + item.Defense + item.HP;

                if (itemStat > maxStat)
                {
                    maxStat = itemStat;
                    bestItem = item;
                }
            }
        }

        // 현재 끼고 있는 장비의 스탯 확인
        EquipmentInstance currentEquip = null;
        switch (type)
        {
            case EquipmentType.Weapon: currentEquip = hero.equippedWeapon; break;
            case EquipmentType.Armor: currentEquip = hero.equippedArmor; break;
            case EquipmentType.Pants: currentEquip = hero.equippedPants; break;
            case EquipmentType.Helmet: currentEquip = hero.equippedHelmet; break;
        }

        int currentStat = currentEquip != null ? (currentEquip.Attack + currentEquip.Defense + currentEquip.HP) : -1;

        // 더 좋은 장비가 인벤토리에 있다면 교체 장착
        if (bestItem != null && maxStat > currentStat)
        {
            EquipToHero(hero, bestItem);
        }
    }

    // =========================
    // 일괄 판매 시스템
    // =========================
    public void BulkSell()
    {
        // 가방에 팔 장비가 있는지 확인
        if (equipmentInventory.Count == 0)
        {
            Debug.Log("가방이 비어있어 판매할 장비가 없습니다!");
            return;
        }

        int totalEarnedGold = 0;
        // 장비 등급에 따라 골드 계산
        foreach (var item in equipmentInventory)
        {
            if (item.Grade == EquipmentGrade.Epic) totalEarnedGold += 1000;
            else if (item.Grade == EquipmentGrade.Rare) totalEarnedGold += 300;
            else totalEarnedGold += 100;
        }

        // PlayerManager를 호출해서 실제로 골드를 추가
        if (AFKHero.Player.PlayerManager.Instance != null)
        {
            AFKHero.Player.PlayerManager.Instance.AddGold(totalEarnedGold);
        }

        // 가방에 있는 모든 장비 데이터를 제거
        equipmentInventory.Clear();

        Debug.Log($"[EquipmentManager] 장비 일괄 판매 완료! 총 {totalEarnedGold} 골드 획득.");
    }
}