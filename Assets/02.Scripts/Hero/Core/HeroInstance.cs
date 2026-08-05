using UnityEngine;

[System.Serializable]
public class HeroInstance
{
    public HeroData data;
    public int level;
    public bool isUnlocked;

    public HeroInstance(HeroData heroData, bool defaultUnlocked = false)
    {
        data = heroData;
        level = 1;
        isUnlocked = defaultUnlocked;
    }

    // ==========================
    // 영웅 자체의 기본 스탯
    // ==========================
    public int MaxHP => data.GetJobStats().hp + (level - 1) * 20;
    public int Attack => data.GetJobStats().attack + (level - 1) * 5;
    public int Defense => data.GetJobStats().defense + (level - 1) * 2;
    public float AttackSpeed => data.GetJobStats().attackSpeed;
    public float AttackRange => data.GetJobStats().attackRange;

    // =========================================
    // 시너지가 적용된 인게임 최종 전투 스탯
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
    // 영웅 성장 기능
    // ===================
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