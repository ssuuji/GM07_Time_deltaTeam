//using System.Collections.Generic;
//using UnityEngine;

//public class EquipmentManager : MonoBehaviour
//{
//    public static EquipmentManager Instance { get; private set; }

//    //플레이어가 획득한 장비 가방
//    public List<EquipmentData> equipmentInventory = new List<EquipmentData>();

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    //장비 획득
//    public void AddEquipment(EquipmentData newEquipment)
//    {
//        equipmentInventory.Add(newEquipment);
//        Debug.Log($"[EquipmentManager] {newEquipment.equipmentName} 획득!");
//    }

//    //영웅에게 장비 착용 시도
//    public void EquipToHero(HeroInstance hero, EquipmentData equipmentToEquip)
//    {
//        if (!equipmentInventory.Contains(equipmentToEquip)) return;

//    //이미 영웅이 해당 부위에 장비를 끼고 있다면, 가방으로 반환
//        EquipmentData existingEquip = null;
//        switch (equipmentToEquip.type)
//        {
//            case EquipmentType.Weapon: existingEquip = hero.equippedWeapon; break;
//            case EquipmentType.Armor: existingEquip = hero.equippedArmor; break;
//            case EquipmentType.Accessory: existingEquip = hero.equippedAccessory; break;
//        }

//        if (existingEquip != null)
//        {
//            equipmentInventory.Add(existingEquip); // 벗은 장비는 인벤토리로
//        }

//    // 새 장비 장착 후 인벤토리에서 제거
//        hero.EquipItem(equipmentToEquip);
//        equipmentInventory.Remove(equipmentToEquip);
//    }
//}
