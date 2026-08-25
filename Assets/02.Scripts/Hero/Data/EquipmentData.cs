using UnityEngine;

// 장비 부위 지정
public enum EquipmentType
{
    Weapon, // 무기
    Armor,  // 갑옷(상의)
    Pants,  // 하의
    Helmet, // 헬멧
}

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Game Data / Equipment Data")]
public class EquipmentData : ScriptableObject
{
    [Header("장비 기본 정보")]
    public string equipmentID;     // 장비 고유 ID
    public string equipmentName;   // 장비 이름
    public EquipmentType type;     // 장비 부위
    public Sprite equipmentIcon;   // UI 표시용 아이콘

    // 고정 스탯에서 랜덤 스탯 범위로 수정
    [Header("랜덤 스탯 범위")]
    public int minAttack;
    public int maxAttack;

    public int minHP;
    public int maxHP;

    public int minDefense;
    public int maxDefense;
}