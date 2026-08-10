using UnityEngine;

// 장비 부위 지정
public enum EquipmentType
{
    Weapon,    // 무기
    Armor,    // 방어구
    Accessory   // 장신구
}

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Game Data / Equipment Data")]
public class EquipmentData : ScriptableObject
{
    [Header("장비 기본 정보")]
    public string equipmentID;     // 장비 고유 ID
    public string equipmentName;    // 장비 이름
    public EquipmentType type;     // 장비 부위
    public Sprite equipmentIcon;    // UI 표시용 아이콘

    [Header("장비 스탯 보너스")]
    public int bonusAttack;         // 올려주는 공격력
    public int bonusHP;           // 올려주는 체력
    public int bonusDefense;        // 올려주는 방어력
}
