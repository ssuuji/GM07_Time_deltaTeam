using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ItemSlot : MonoBehaviour
{
    public Image backgroundImage;

    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Button slotButton;
    public GameObject plusIconObject;

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
                iconImage.sprite = myEquipment.BaseData.equipmentIcon;
                iconImage.gameObject.SetActive(true);
            }

            if (nameText != null)
            {
                nameText.text = myEquipment.BaseData.equipmentName;
                // 글씨는 배경색에 묻히지 않게 하얀색 또는 검은색으로 고정
                nameText.color = Color.black;
            }

            // 장비 등급에 따른 배경색 변경 로직
            if (backgroundImage != null)
            {
                if (myEquipment.Grade == EquipmentGrade.Epic)
                    backgroundImage.color = new Color(0.9f, 0.7f, 1f); // 에픽: 연한 보라색 배경
                else if (myEquipment.Grade == EquipmentGrade.Rare)
                    backgroundImage.color = new Color(0.7f, 0.9f, 1f); // 레어: 연한 파란색 배경
                else
                    backgroundImage.color = new Color(0.9f, 0.9f, 0.9f); // 노말: 밝은 회색 배경
            }

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
            if (nameText != null)
            {
                nameText.text = "";
            }

            // 빈 슬롯일 때의 기본 배경색
            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.8f, 0.8f, 0.8f); // 빈 슬롯: 약간 어두운 회색
            }

            if (plusIconObject != null) plusIconObject.SetActive(true);
            slotButton.interactable = true;
        }

        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(OnClickSlot);
    }

    private void OnClickSlot()
    {
        if (myEquipment == null) return;

        if (mainPanel != null && mainPanel.itemPopup != null)
        {
            mainPanel.itemPopup.OpenPopup(myEquipment, targetHero, mainPanel, isEquippedSlot);
        }
    }
}