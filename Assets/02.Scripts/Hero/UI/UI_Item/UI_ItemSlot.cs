using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ItemSlot : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Button slotButton;

    public GameObject plusIconObject;

    // 슬롯이 보관하는 장비 타입 변경
    private EquipmentInstance myEquipment;
    private HeroInstance targetHero;
    private UI_EquipmentPanel mainPanel;
    private bool isEquippedSlot;

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
                // 아이콘과 이름은 원본 설계도(BaseData) 안에서 꺼내옵니다.
                iconImage.sprite = myEquipment.BaseData.equipmentIcon;
                iconImage.gameObject.SetActive(true);
            }
            if (nameText != null)
            {
                // 등급에 따라 이름 텍스트 색깔을 바꾸기
                nameText.text = myEquipment.BaseData.equipmentName;
            }

            // 장비가 장착되면 [+] 마크 끄기
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

        // 장비를 클릭하면 장착/해제와 동시에 팝업창 띄우기
        if (mainPanel != null && mainPanel.itemPopup != null)
        {
            mainPanel.itemPopup.OpenPopup(myEquipment);
        }

        if (isEquippedSlot)
        {
            EquipmentManager.Instance.equipmentInventory.Add(myEquipment);
            // 부위 정보를 기반으로 영웅의 장비 해제
            targetHero.UnequipItem(myEquipment.BaseData.type);
        }
        else
        {
            EquipmentManager.Instance.EquipToHero(targetHero, myEquipment);
        }

        if (mainPanel != null)
        {
            mainPanel.RefreshUI();
        }
    }
}