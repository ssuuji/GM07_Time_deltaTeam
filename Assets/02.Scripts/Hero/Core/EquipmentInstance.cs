using UnityEngine;

[System.Serializable]
public class EquipmentInstance
{
    public EquipmentData BaseData;

    public EquipmentGrade Grade;
    public int Attack;
    public int Defense;
    public int HP;

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
}
