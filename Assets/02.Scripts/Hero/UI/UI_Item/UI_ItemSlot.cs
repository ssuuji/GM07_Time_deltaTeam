using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ItemSlot : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Button slotButton;

    public GameObject plusIconObject;

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
            if (iconImage != null)
            {
                iconImage.sprite = myEquipment.equipmentIcon;
                iconImage.gameObject.SetActive(true); // 혹시 꺼져있을까봐 켜주기
            }
            if (nameText != null) nameText.text = myEquipment.equipmentName;

            // 장비가 장착되면 [+] 마크 끄기
            if (plusIconObject != null) plusIconObject.SetActive(false);

            slotButton.interactable = true;
        }
        else
        {
            if (iconImage != null)
            {
                iconImage.sprite = null;
                // 투명한 흰색 네모가 보이지 않게 아예 꺼버립니다.
                iconImage.gameObject.SetActive(false);
            }

            // "New Text" 같은 글씨가 빈 슬롯에 뜨지 않게 깔끔하게 지우기
            if (nameText != null) nameText.text = "";

            // 빈 슬롯이면 다시 [+] 마크 켜기
            if (plusIconObject != null) plusIconObject.SetActive(true);

            slotButton.interactable = true;
        }

        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(OnClickSlot);
    }

    private void OnClickSlot()
    {
        if (myEquipment == null)
        {
            // 만약 빈 슬롯을 눌렀을 때 가방 창이 열리게 하고 싶다면 아래 두 줄의 주석을 지우기
            // if (mainPanel != null && !mainPanel.gameObject.activeSelf) 
            //     mainPanel.gameObject.SetActive(true);

            return;
        }

        if (isEquippedSlot)
        {
            EquipmentManager.Instance.equipmentInventory.Add(myEquipment);
            targetHero.UnequipItem(myEquipment.type);
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