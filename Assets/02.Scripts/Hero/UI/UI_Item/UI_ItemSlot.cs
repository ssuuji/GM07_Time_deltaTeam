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

    [Header("선택 판매 UI")]
    public GameObject checkmarkObject;

    public void Setup(EquipmentInstance equipData, HeroInstance hero, UI_EquipmentPanel panel, bool isEquipped)
    {
        myEquipment = equipData;
        targetHero = hero;
        mainPanel = panel;
        isEquippedSlot = isEquipped;

        // 슬롯이 새로 만들어지거나 갱신될 때 체크박스는 끄고 시작
        if (checkmarkObject != null) checkmarkObject.SetActive(false);

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
                nameText.color = Color.black;
            }

            // 장비 등급에 따른 배경색 변경 로직
            if (backgroundImage != null)
            {
                if (myEquipment.Grade == EquipmentGrade.Epic)
                    backgroundImage.color = new Color(0.9f, 0.7f, 1f); // 에픽
                else if (myEquipment.Grade == EquipmentGrade.Rare)
                    backgroundImage.color = new Color(0.7f, 0.9f, 1f); // 레어
                else
                    backgroundImage.color = new Color(0.9f, 0.9f, 0.9f); // 노말
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
                backgroundImage.color = new Color(0.8f, 0.8f, 0.8f);
            }

            if (plusIconObject != null) plusIconObject.SetActive(true);
            slotButton.interactable = true;
        }

        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(OnClickSlot);
    }

    public void SetCheckmark(bool isOn)
    {
        if (checkmarkObject != null) checkmarkObject.SetActive(isOn);
    }

    private void OnClickSlot()
    {
        if (myEquipment == null) return;

        // 현재 패널이 판매 모드인지 확인하고, 판매 모드일 경우 팝업을 띄우지 않고 선택 처리만 수행
        if (mainPanel != null && mainPanel.isSellMode && !isEquippedSlot)
        {
            mainPanel.ToggleItemSelection(myEquipment, this);
            return; // 팝업이 안 뜨도록 여기서 함수 종료
        }

        // 판매 모드가 아닐 때만 기존처럼 팝업창 열기
        if (mainPanel != null && mainPanel.itemPopup != null)
        {
            mainPanel.itemPopup.OpenPopup(myEquipment, targetHero, mainPanel, isEquippedSlot);
        }
    }
}