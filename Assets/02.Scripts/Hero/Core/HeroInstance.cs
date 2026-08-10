using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
public class HeroInstance
{
    public HeroData data;
    public int level;
    public bool isUnlocked;

    // 장비 슬롯 변수 - 장비 할때 추가하기
    //public EquipmentData equippedWeapon { get; private set; }
    //public EquipmentData equippedArmor { get; private set; }
    //public EquipmentData equippedAccessory { get; private set; }

    // 추가한 내용 : 장비 장착 및 해제 함수
    //public void EquipItem(EquipmentData equipment)
    //{
    //    if (equipment == null) return;

    //    switch (equipment.type)
    //    {
    //        case EquipmentType.Weapon: equippedWeapon = equipment; break;
    //        case EquipmentType.Armor: equippedArmor = equipment; break;
    //        case EquipmentType.Accessory: equippedAccessory = equipment; break;
    //    }
    //    Debug.Log($"[{data.HeroName}] {equipment.equipmentName} 장착 완료!");
    //}

    //public void UnequipItem(EquipmentType type)
    //{
    //    switch (type)
    //    {
    //        case EquipmentType.Weapon: equippedWeapon = null; break;
    //        case EquipmentType.Armor: equippedArmor = null; break;
    //        case EquipmentType.Accessory: equippedAccessory = null; break;
    //    }
    //}

    //// 장비가 올려주는 스탯 총합 계산 헬퍼
    //private int EquipmentBonusAttack => (equippedWeapon?.bonusAttack ?? 0) + 
    //    (equippedArmor?.bonusAttack ?? 0) + (equippedAccessory?.bonusAttack ?? 0);
    //private int EquipmentBonusHP => (equippedWeapon?.bonusHP ?? 0) +
    //    (equippedArmor?.bonusHP ?? 0) + (equippedAccessory?.bonusHP ?? 0);
    //private int EquipmentBonusDefense => (equippedWeapon?.bonusDefense ?? 0) + 
    //    (equippedArmor?.bonusDefense ?? 0) + (equippedAccessory?.bonusDefense ?? 0);

    public HeroInstance(HeroData heroData, bool defaultUnlocked = false)
    {
        data = heroData;
        level = 1;
        isUnlocked = defaultUnlocked;
    }

    // ==========================
    // 영웅 자체 기본 스탯
    // ==========================
    public int MaxHP => data.GetJobStats().hp + (level - 1) * 20;
    public int Attack => data.GetJobStats().attack + (level - 1) * 5;
    public int Defense => data.GetJobStats().defense + (level - 1) * 2;
    public float AttackSpeed => data.GetJobStats().attackSpeed;
    public float AttackRange => data.GetJobStats().attackRange;

  
    // 장비를 추가한다면 수정할 내용: 최종 스탯 프로퍼티 덮어쓰기 - 아래 시너지 적용 스탯과 교체
    //// 장비 스탯을 먼저 더한 뒤에 파티 시너지 뻥튀기를 적용
 
    //public int FinalMaxHP
    //{
    //    get
    //    {
    //        int baseWithEquip = MaxHP + EquipmentBonusHP; // 기본 체력 + 장비 체력

    //        if (PartyManager.Instance != null && PartyManager.Instance.IsHeroInParty(this))
    //        {
    //            float multiplier = 1f + PartyManager.Instance.totalBonusHpRate;
    //            return Mathf.RoundToInt(baseWithEquip * multiplier);
    //        }
    //        return baseWithEquip;
    //    }
    //}

    //public int FinalAttack
    //{
    //    get
    //    {
    //        int baseWithEquip = Attack + EquipmentBonusAttack; // 기본 공격력 + 장비 공격력

    //        if (PartyManager.Instance != null && PartyManager.Instance.IsHeroInParty(this))
    //        {
    //            float multiplier = 1f + PartyManager.Instance.totalBonusAttackRate;
    //            return Mathf.RoundToInt(baseWithEquip * multiplier);
    //        }
    //        return baseWithEquip;
    //    }
    //}

    //public int FinalDefense => Defense + EquipmentBonusDefense;



    // =========================================
    // 시너지 적용 스탯
    // =========================================

    // 최종 체력 = 기본 체력 * (1 + 파티 체력 시너지 퍼센트)
    public int FinalMaxHP
    {
        get
        {
            if (PartyManager.Instance == null) return MaxHP;

            float multiplier = 1f + PartyManager.Instance.totalBonusHpRate;
            return Mathf.RoundToInt(MaxHP * multiplier);
        }
    }

    // 최종 공격력 = 기본 공격력 * (1 + 파티 공격력 시너지 퍼센트)
    public int FinalAttack
    {
        get
        {
            if (PartyManager.Instance == null) return Attack;

            float multiplier = 1f + PartyManager.Instance.totalBonusAttackRate;
            return Mathf.RoundToInt(Attack * multiplier);
        }
    }

    // ===================
    // 레벨업
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

        level++;
        return true;
    }
}