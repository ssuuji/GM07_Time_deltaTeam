using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ItemPopup : MonoBehaviour
{
    [Header("팝업 UI 연결")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI gradeText;
    public TextMeshProUGUI statText;

    // 장착/해제 버튼 연결
    public Button actionButton;
    public TextMeshProUGUI actionBtnText;

    // 장착 로직 처리를 위해 임시로 정보를 들고 있을 변수들
    private EquipmentInstance currentEquip;
    private HeroInstance currentHero;
    private UI_EquipmentPanel mainPanel;
    private bool isEquippedItem;

    // 팝업을 열 때 장착에 필요한 모든 정보를 함께 받아오기
    public void OpenPopup(EquipmentInstance equip, HeroInstance hero, UI_EquipmentPanel panel, bool isEquipped)
    {
        currentEquip = equip;
        currentHero = hero;
        mainPanel = panel;
        isEquippedItem = isEquipped;

        gameObject.SetActive(true); // 팝업창 켜기

        if (iconImage != null)
        {
            iconImage.sprite = equip.BaseData.equipmentIcon;
        }

        // 이름 및 등급 표시
        nameText.text = equip.BaseData.equipmentName;
        gradeText.text = $"등급: {equip.Grade}";

        if (equip.Grade == EquipmentGrade.Epic) gradeText.color = new Color(0.8f, 0.2f, 1f);
        else if (equip.Grade == EquipmentGrade.Rare) gradeText.color = new Color(0.2f, 0.6f, 1f);
        else gradeText.color = Color.white;

        // 스탯 표시
        statText.text = $"공격력 : {equip.Attack}\n방어력 : {equip.Defense}\n체  력 : {equip.HP}";

        // 이미 장착 중이면 '해제', 가방에 있으면 '장착'으로 글씨 변경
        if (actionButton != null && actionBtnText != null)
        {
            actionBtnText.text = isEquippedItem ? "해제" : "장착";

            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnClickActionButton); // 버튼에 클릭 이벤트 연결
        }
    }

    // 팝업창의 장착/해제 버튼을 눌렀을 때 실행될 함수
    private void OnClickActionButton()
    {
        if (currentEquip == null || currentHero == null) return;

        if (isEquippedItem)
        {
            // 해제 로직
            EquipmentManager.Instance.equipmentInventory.Add(currentEquip);
            currentHero.UnequipItem(currentEquip.BaseData.type);
        }
        else
        {
            // 장착 로직
            EquipmentManager.Instance.EquipToHero(currentHero, currentEquip);
        }

        // 장착/해제가 끝났으니 UI를 새로고침하고 팝업창을 닫기
        if (mainPanel != null) mainPanel.RefreshUI();
        ClosePopup();
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}