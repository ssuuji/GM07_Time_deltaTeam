using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
public class HeroInstance
{
    public HeroData data;
    public int level;
    public bool isUnlocked;
    public bool isResonanced;

    // 승급 재료로 사용하던 중복 카드 duplicateCount 삭제
    public HeroGrade currentGrade;

    // =========================================
    // 장비 슬롯 및 세트 효과
    // =========================================
    public EquipmentInstance equippedWeapon { get; private set; }
    public EquipmentInstance equippedArmor { get; private set; }
    public EquipmentInstance equippedPants { get; private set; }
    public EquipmentInstance equippedHelmet { get; private set; }

    // 장비 장착 함수
    public void EquipItem(EquipmentInstance equipment)
    {
        if (equipment == null) return;

        switch (equipment.BaseData.type)
        {
            case EquipmentType.Weapon: equippedWeapon = equipment; break;
            case EquipmentType.Armor: equippedArmor = equipment; break;
            case EquipmentType.Pants: equippedPants = equipment; break;
            case EquipmentType.Helmet: equippedHelmet = equipment; break;
        }
        string heroName = data != null ? data.HeroName : "알수없는 영웅";
        Debug.Log($"[{heroName}] {equipment.BaseData.equipmentName} 장착 완료!");
    }

    // 장비 해제 함수
    public void UnequipItem(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Weapon: equippedWeapon = null; break;
            case EquipmentType.Armor: equippedArmor = null; break;
            case EquipmentType.Pants: equippedPants = null; break;
            case EquipmentType.Helmet: equippedHelmet = null; break;
        }
    }

    // 고정 스탯 대신 랜덤으로 확정된 진짜 스탯 합산
    private int EquipmentBonusAttack =>
        (equippedWeapon?.Attack ?? 0) + (equippedArmor?.Attack ?? 0) +
        (equippedPants?.Attack ?? 0) + (equippedHelmet?.Attack ?? 0);

    private int EquipmentBonusHP =>
        (equippedWeapon?.HP ?? 0) + (equippedArmor?.HP ?? 0) +
        (equippedPants?.HP ?? 0) + (equippedHelmet?.HP ?? 0);

    private int EquipmentBonusDefense =>
        (equippedWeapon?.Defense ?? 0) + (equippedArmor?.Defense ?? 0) +
        (equippedPants?.Defense ?? 0) + (equippedHelmet?.Defense ?? 0);

    // 장비 고유 ID를 기반으로 4개 부위의 세트 개수 판별
    public int GetSetCount(string setPrefix)
    {
        int count = 0;
        if (equippedWeapon != null && equippedWeapon.BaseData.equipmentID.StartsWith(setPrefix)) count++;
        if (equippedArmor != null && equippedArmor.BaseData.equipmentID.StartsWith(setPrefix)) count++;
        if (equippedPants != null && equippedPants.BaseData.equipmentID.StartsWith(setPrefix)) count++;
        if (equippedHelmet != null && equippedHelmet.BaseData.equipmentID.StartsWith(setPrefix)) count++;
        return count;
    }

    // =========================================
    // 장비 스탯이 포함된 최종 전투 스탯
    // =========================================
    public int FinalMaxHP
    {
        get
        {
            int baseWithEquip = MaxHP + EquipmentBonusHP;
            float multiplier = 1f;

            if (PartyManager.Instance != null && PartyManager.Instance.IsHeroInParty(this))
            {
                multiplier += PartyManager.Instance.totalBonusHpRate;
            }
            return Mathf.RoundToInt(baseWithEquip * multiplier);
        }
    }

    public int FinalAttack
    {
        get
        {
            int baseWithEquip = Attack + EquipmentBonusAttack;

            if (PartyManager.Instance == null || !PartyManager.Instance.IsHeroInParty(this)) return baseWithEquip;

            float multiplier = 1f + PartyManager.Instance.totalBonusAttackRate;
            return Mathf.RoundToInt(baseWithEquip * multiplier);
        }
    }

    public int FinalDefense => Defense + EquipmentBonusDefense;


    public HeroInstance(HeroData heroData, bool defaultUnlocked = false)
    {
        data = heroData;
        level = 1;
        isUnlocked = defaultUnlocked;
        isResonanced = false;

        currentGrade = data.HeroGrade;
    }

    // ==========================
    // 영웅 자체 기본 스탯
    // ==========================

    private float GradeMultiplier
    {
        get
        {
            if (data == null) return 1f;

            int upgradeCount = (int)currentGrade - (int)data.HeroGrade;
            if (upgradeCount <= 0) return 1f;

            float ratePerUpgrade = 0.15f;

            switch (data.HeroGrade)
            {
                case HeroGrade.Normal: ratePerUpgrade = 0.10f; break;
                case HeroGrade.Rare: ratePerUpgrade = 0.15f; break;
                case HeroGrade.Epic: ratePerUpgrade = 0.20f; break;
            }

            return 1f + (upgradeCount * ratePerUpgrade);
        }
    }

    public int MaxHP
    {
        get
        {
            if (data == null) return 0;
            int baseHp = data.GetJobStats().hp + (level - 1) * 20;
            return Mathf.RoundToInt(baseHp * GradeMultiplier);
        }
    }

    public int Attack
    {
        get
        {
            if (data == null) return 0;
            int baseAttack = data.GetJobStats().attack + (level - 1) * 5;
            return Mathf.RoundToInt(baseAttack * GradeMultiplier);
        }
    }

    public int Defense
    {
        get
        {
            if (data == null) return 0;
            int baseDefense = data.GetJobStats().defense + (level - 1) * 2;
            return Mathf.RoundToInt(baseDefense * GradeMultiplier);
        }
    }

    public float AttackSpeed => (data != null) ? data.GetJobStats().attackSpeed : 0f;
    public float AttackRange => (data != null) ? data.GetJobStats().attackRange : 0f;

    // ===================
    // 레벨업 및 승급
    // ===================

    public int LevelUpCost => level * 100;

    public bool LevelUp()
    {
        if (level >= 50)
        {
            Debug.LogWarning($"{data.HeroName}은(는) 이미 최고 레벨(50)입니다.");
            return false;
        }

        if (isResonanced)
        {
            Debug.LogWarning($"{data.HeroName}은(는) 현재 공명중입니다.");
            return false;
        }

        level++;
        return true;
    }

    public HeroGrade GetRequiredShardGrade()
    {
        if (currentGrade == HeroGrade.Normal || currentGrade == HeroGrade.NormalPlus) return HeroGrade.Normal;
        if (currentGrade == HeroGrade.Rare || currentGrade == HeroGrade.RarePlus) return HeroGrade.Rare;
        return HeroGrade.Epic;
    }

    public int GetRequiredShardCount()
    {
        switch (currentGrade)
        {
            case HeroGrade.Normal: return 5;
            case HeroGrade.NormalPlus: return 10;
            case HeroGrade.Rare: return 5;
            case HeroGrade.RarePlus: return 10;
            case HeroGrade.Epic: return 10;
            default: return 9999;
        }
    }

    public void UpgradeGrade()
    {
        if (currentGrade < HeroGrade.EpicPlus)
        {
            currentGrade++;
            Debug.Log($"{data.HeroName}영웅이 {currentGrade}등급으로 승급했습니다!");
        }
        else
        {
            Debug.LogWarning("이미 최고 등급입니다.");
        }
    }
}