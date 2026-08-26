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

        // 장비의 부위 정보는 BaseData 안에 있으므로 참조 경로를 바꿨습니다
        switch (equipment.BaseData.type)
        {
            case EquipmentType.Weapon: equippedWeapon = equipment; break;
            case EquipmentType.Armor: equippedArmor = equipment; break;
            case EquipmentType.Pants: equippedPants = equipment; break;
            case EquipmentType.Helmet: equippedHelmet = equipment; break;
        }
        string heroName = data != null ? data.HeroName : "알수없는 영웅";

        // 장비의 이름도 BaseData 안에서 가져오도록 바꿨습니다
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
    public EquipmentInstance GetEquippedItem(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Weapon: return equippedWeapon;
            case EquipmentType.Armor: return equippedArmor;
            case EquipmentType.Pants: return equippedPants;
            case EquipmentType.Helmet: return equippedHelmet;
            default: return null;
        }
    }

    // // onusAttack 같은 고정 스탯 대신, 확정된 스탯을 더하도록 바꿨습니다.
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
        // 세트 확인을 위한 장비 ID도 BaseData 안에서 꺼내오도록 바꿨습니다.
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

        //  처음 생성 시 처음 등급으로 초기화
        currentGrade = data.HeroGrade;
    }

    // ==========================
    // 영웅 자체 기본 스탯
    // ==========================

    // 현재 등급에 따른 추가 스탯 배율 계산
    // 태생 등급에 따라 승급 시 추가되는 스탯 가중치를 다르게 부여
    // 태생 노말 영웅이 풀승급을 하더라도 태생 에픽의 벽을 넘지 못하도록 제어
    private float GradeMultiplier
    {
        get
        {
            if (data == null) return 1f;

            // 현재 등급과 태생 등급의 차이(승급한 횟수)를 계산
            // Enum 순서: Normal(0), NormalPlus(1), Rare(2), RarePlus(3), Epic(4), EpicPlus(5)
            int upgradeCount = (int)currentGrade - (int)data.HeroGrade;

            // 승급을 한 번도 안 했다면 보너스 없음
            if (upgradeCount <= 0) return 1f;

            float ratePerUpgrade = 0.15f; // 기본적으로 1단계 승급당 15% 증가

            // 태생 등급별 보너스 효율 차등화
            switch (data.HeroGrade)
            {
                case HeroGrade.Normal:
                    ratePerUpgrade = 0.10f; // 태생 노말은 승급 효율을 10%로 낮춤
                    break;
                case HeroGrade.Rare:
                    ratePerUpgrade = 0.15f; // 태생 레어는 승급 효율 15%
                    break;
                case HeroGrade.Epic:
                    ratePerUpgrade = 0.20f; // 태생 에픽은 승급 효율을 20%로 우대
                    break;
            }

            return 1f + (upgradeCount * ratePerUpgrade);
        }
    }

    public int MaxHP
    {
        get
        {
            if (data == null) return 0; // 영웅 데이터가 비어있으면 체력 계산을 스킵하고 0을 반환

            int baseHp = data.GetJobStats().hp + (level - 1) * 20;
            return Mathf.RoundToInt(baseHp * GradeMultiplier);
        }
    }

    public int Attack
    {
        get
        {
            if (data == null) return 0; // 영웅 데이터가 비어있으면 공격력 계산을 스킵하고 0을 반환

            int baseAttack = data.GetJobStats().attack + (level - 1) * 5;
            return Mathf.RoundToInt(baseAttack * GradeMultiplier);
        }
    }

    public int Defense
    {
        get
        {
            if (data == null) return 0; // 영웅 데이터가 비어있으면 방어력 계산을 스킵하고 0을 반환합니다.

            int baseDefense = data.GetJobStats().defense + (level - 1) * 2;
            return Mathf.RoundToInt(baseDefense * GradeMultiplier);
        }
    }

    
    public float AttackSpeed => (data != null) ? data.GetJobStats().attackSpeed : 0f;
    public float AttackRange => (data != null) ? data.GetJobStats().attackRange : 0f;


    // ===================
    // 레벨업 및 승급
    // ===================

    // 현재 레벨 비례 레벨업 필요 골드 계산
    public int LevelUpCost => level * 100;

    public bool LevelUp()
    {
        if (level >= 50)
        {
            Debug.LogWarning($"{data.HeroName}은(는) 이미 최고 레벨(50)입니다.");
            return false;
        }

        if(isResonanced)
        {
            Debug.LogWarning($"{data.HeroName}은(는) 현재 공명중입니다.");
            return false;
        }

        level++;
        return true;
    }

    // 현재 등급에서 다음 등급으로 갈대 필요한 조각 반환
    public HeroGrade GetRequiredShardGrade()
    {
        if (currentGrade == HeroGrade.Normal || currentGrade == HeroGrade.NormalPlus) return HeroGrade.Normal;
        if (currentGrade == HeroGrade.Rare || currentGrade == HeroGrade.RarePlus) return HeroGrade.Rare;
        return HeroGrade.Epic;
    }

    // 승급 필요 조각 개수 반환
    public int GetRequiredShardCount()
    {
        switch (currentGrade)
        {
            case HeroGrade.Normal: return 5;   // Normal -> Normal+ (노말 조각 5개)
            case HeroGrade.NormalPlus: return 10;  // Normal+ -> Rare (노말 조각 10개)
            case HeroGrade.Rare: return 5;   // Rare -> Rare+ (레어 조각 5개)
            case HeroGrade.RarePlus: return 10;  // Rare+ -> Epic (레어 조각 10개)
            case HeroGrade.Epic: return 10;  // Epic -> Epic+ (에픽 조각 10개)
            default: return 9999;
        }
    }

    // 영웅 승급 함수
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