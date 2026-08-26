using UnityEngine;

public enum EquipmentType
{
    Weapon, // ¹«±â
    Armor,  // °©¿Ê(»óÀÇ)
    Pants,  // ÇÏÀÇ
    Helmet, // Çï¸ä
}

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Game Data / Equipment Data")]
public class EquipmentData : ScriptableObject
{
    [Header("Àåºñ ±âº» Á¤º¸")]
    public string equipmentID;     
    public string equipmentName;   
    public EquipmentType type;     
    public Sprite equipmentIcon;   

    [Header("·£´ı ½ºÅÈ ¹üÀ§")]
    public int minAttack;         
    public int maxAttack;         

    public int minHP;           
    public int maxHP;           

    public int minDefense;        
    public int maxDefense;        
}
