using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ItemSlot : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Button slotButton;

    private EquipmentData myEquipment;
    private HeroInstance targetHero;
    private UI_EquipmentPanel mainPanel;
    private bool isEquippedSlot;

    public void Setup(EquipmentData equipData, HeroInstance hero, UI_EquipmentPanel panel, bool isEquipped)
    {
        myEquipment = equipData;
        targetHero = hero;
        mainPanel = panel;
        isEquippedSlot = isEquipped;

        if (myEquipment != null)
        {
            if (iconImage != null) iconImage.sprite = myEquipment.equipmentIcon;
            if (nameText != null) nameText.text = myEquipment.equipmentName;
            slotButton.interactable = true;
        }
        else
        {
            if (iconImage != null) iconImage.sprite = null;
            if (nameText != null) nameText.text = "장비 없음";
            slotButton.interactable = false;
        }

        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(OnClickSlot);
    }

    private void OnClickSlot()
    {
        if (isEquippedSlot)
        {
            EquipmentManager.Instance.equipmentInventory.Add(myEquipment);
            targetHero.UnequipItem(myEquipment.type);
        }
        else
        {
            EquipmentManager.Instance.EquipToHero(targetHero, myEquipment);
        }
        mainPanel.RefreshUI();
    }
}