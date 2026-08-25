using UnityEngine;

[System.Serializable]
public class EquipmentInstance
{
    public EquipmentData BaseData; // 원본 설계도 참조

    public EquipmentGrade Grade;   // 획득 시 결정된 등급
    public int Attack;             // 확정된 공격력
    public int Defense;            // 확정된 방어력
    public int HP;                 // 확정된 체력

    // 생성자: 몬스터를 잡거나 상자를 열어 아이템이 드롭될 때 실행
    public EquipmentInstance(EquipmentData data)
    {
        BaseData = data;

        // 설계도의 최소~최대 범위 안에서 주사위를 굴려 스탯 확정
        Attack = Random.Range(data.minAttack, data.maxAttack + 1);
        Defense = Random.Range(data.minDefense, data.maxDefense + 1);
        HP = Random.Range(data.minHP, data.maxHP + 1);

        // 굴려진 스탯을 바탕으로 등급(Normal, Rare, Epic) 판정
        DetermineGrade(data);
    }

    private void DetermineGrade(EquipmentData data)
    {
        // 내가 뽑은 스탯의 총합
        float totalStats = Attack + Defense + HP;

        // 설계도에서 뽑을 수 있는 이론상 최대 스탯의 총합
        float maxPossibleStats = data.maxAttack + data.maxDefense + data.maxHP;

        // 0으로 나누는 에러방지
        if (maxPossibleStats <= 0)
        {
            Grade = EquipmentGrade.Normal;
            return;
        }

        // 내가 뽑은 스탯이 최대치 대비 몇 퍼센트인지(0.0 ~ 1.0) 계산
        float statPercent = totalStats / maxPossibleStats;

        // 기준: 하위 50%는 노말, 50~80%는 레어, 상위 20%는 에픽
        if (statPercent <= 0.5f)
        {
            Grade = EquipmentGrade.Normal;
        }
        else if (statPercent <= 0.8f)
        {
            Grade = EquipmentGrade.Rare;
        }
        else
        {
            Grade = EquipmentGrade.Epic;
            Debug.Log($"<color=magenta>🎉 [에픽 획득!] {data.equipmentName}이(가) 최고 스탯으로 떴습니다!</color>");
        }
    }
}