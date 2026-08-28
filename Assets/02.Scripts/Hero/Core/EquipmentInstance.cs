using UnityEngine;

[System.Serializable]
public class EquipmentInstance
{
    public EquipmentData BaseData;

    public EquipmentGrade Grade;
    public int Attack;
    public int Defense;
    public int HP;

    // 현재 강화 수치
    public int EnhanceLevel = 0;

    public EquipmentInstance(EquipmentData data)
    {
        BaseData = data;

        Attack = Random.Range(data.minAttack, data.maxAttack + 1);
        Defense = Random.Range(data.minDefense, data.maxDefense + 1);
        HP = Random.Range(data.minHP, data.maxHP + 1);

        DetermineGrade(data);
    }

    private void DetermineGrade(EquipmentData data)
    {
        float totalStats = Attack + Defense + HP;
        float maxPossibleStats = data.maxAttack + data.maxDefense + data.maxHP;

        if (maxPossibleStats <= 0)
        {
            Grade = EquipmentGrade.Normal;
            return;
        }

        float statPercent = totalStats / maxPossibleStats;

        if (statPercent <= 0.5f) Grade = EquipmentGrade.Normal;
        else if (statPercent <= 0.8f) Grade = EquipmentGrade.Rare;
        else Grade = EquipmentGrade.Epic;
    }

    // 확률 기반 강화 로직
    public bool EnhanceItem()
    {
        // 최대 강화 수치 체크
        if (EnhanceLevel >= 10) return false;

        // 현재 강화 수치에 따른 성공 확률 설정 (예: 0강->100%, 1강->90% ... 9강->10%)
        int[] successChances = { 100, 90, 80, 70, 60, 50, 40, 30, 20, 10 };
        int currentChance = successChances[EnhanceLevel];

        // 1 ~ 100 사이의 랜덤 주사위 굴리기
        int roll = Random.Range(1, 101);

        // 주사위 값이 성공 확률보다 작거나 같으면 성공
        if (roll <= currentChance)
        {
            EnhanceLevel++;
            Attack += 5;
            Defense += 5;
            HP += 20;
            return true; // 성공 반환
        }
        else
        {
            return false; // 실패 반환
        }
    }
}
