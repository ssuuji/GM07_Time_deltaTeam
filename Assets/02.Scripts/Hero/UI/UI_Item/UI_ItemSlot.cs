using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ItemSlot : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Button slotButton;
    public GameObject plusIconObject;

    // 슬롯이 보관하는 장비를 'EquipmentInstance'로 변경했습니다.
    private EquipmentInstance myEquipment;
    private HeroInstance targetHero;
    private UI_EquipmentPanel mainPanel;
    private bool isEquippedSlot;

    // 매개변수 타입도 EquipmentInstance로 변경
    public void Setup(EquipmentInstance equipData, HeroInstance hero, UI_EquipmentPanel panel, bool isEquipped)
    {
        myEquipment = equipData;
        targetHero = hero;
        mainPanel = panel;
        isEquippedSlot = isEquipped;

        if (myEquipment != null)
        {
            if (iconImage != null)
            {
                // 아이콘과 이름은 원본 설계도 안에서 꺼내옵니다.
                iconImage.sprite = myEquipment.BaseData.equipmentIcon;
                iconImage.gameObject.SetActive(true);
            }
            if (nameText != null) nameText.text = myEquipment.BaseData.equipmentName;

            if (plusIconObject != null) plusIconObject.SetActive(false);
            slotButton.interactable = true;
        }
        else
        {
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.gameObject.SetActive(false);
            }
            if (nameText != null) nameText.text = "";
            if (plusIconObject != null) plusIconObject.SetActive(true);
            slotButton.interactable = true;
        }

        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(OnClickSlot);
    }

    private void OnClickSlot()
    {
        if (myEquipment == null) return;

        // 클릭 시 즉시 장착하지 않고, 필요한 정보를 담아서 팝업창만 열기
        if (mainPanel != null && mainPanel.itemPopup != null)
        {
            mainPanel.itemPopup.OpenPopup(myEquipment, targetHero, mainPanel, isEquippedSlot);
        }
    }
}