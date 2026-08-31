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

    // 강화 버튼 연결
    [Header("강화 버튼 연결")]
    public Button enhanceButton;

    // 판매 버튼 연결
    [Header("판매 버튼 연결")]
    public Button sellButton;

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

        // 강화 수치가 1 이상이면 이름 앞에 "+수치"를 붙임
        if (equip.EnhanceLevel > 0)
        {
            nameText.text = $"+{equip.EnhanceLevel} {equip.BaseData.equipmentName}";
        }
        else
        {
            nameText.text = equip.BaseData.equipmentName;
        }


        // 등급 표시
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

        // 강화 버튼 이벤트 및 10강 제한 연결
        if (enhanceButton != null)
        {
            // +10강이면 버튼을 클릭할 수 없게 만듭니다.
            enhanceButton.interactable = (equip.EnhanceLevel < 10);

            enhanceButton.onClick.RemoveAllListeners();
            enhanceButton.onClick.AddListener(OnClickEnhanceButton);
        }

        // 판매 버튼 이벤트 연결
        if (sellButton != null)
        {
            // 영웅이 장착 중인 아이템은 바로 팔 수 없게 버튼 비활성화 (가방에 있는 것만 판매 가능)
            sellButton.interactable = !isEquippedItem;

            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(OnClickSellButton);
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

    // 강화 버튼을 눌렀을 때 실행되는 확률 함수
    private void OnClickEnhanceButton()
    {
        if (currentEquip == null) return;

        // 이미 10강이면 진행 불가
        if (currentEquip.EnhanceLevel >= 10) return;

        // 강화 비용 계산 기본 1000골드 + 단계당 500골드씩 증가
        int enhanceCost = 1000 + (currentEquip.EnhanceLevel * 500);

        // 골드 부족 체크
        if (!AFKHero.Player.PlayerManager.Instance.TryUseGold(enhanceCost))
        {
            Debug.Log($"<color=orange>[강화 불가]</color> 골드가 부족합니다! (필요 골드: {enhanceCost}G)");
            return; // 골드가 부족하므로 강화를 중단
        }

        // 골드 차감을 마쳤으니 확률 강화 시도
        bool isSuccess = currentEquip.EnhanceItem();

        if (isSuccess)
        {
            Debug.Log($"<color=green>[강화 성공!]</color> {currentEquip.BaseData.equipmentName}이(가) +{currentEquip.EnhanceLevel}강이 되었습니다! (-{enhanceCost}G)");
        }
        else
        {
            Debug.Log($"<color=red>[강화 실패...]</color> {currentEquip.BaseData.equipmentName} 강화에 실패했습니다. (-{enhanceCost}G)");
        }

        // 바뀐 정보를 화면에 즉시 갱신
        OpenPopup(currentEquip, currentHero, mainPanel, isEquippedItem);
        if (mainPanel != null) mainPanel.RefreshUI();
    }

    //  판매 버튼을 눌렀을 때 실행
    private void OnClickSellButton()
    {
        if (currentEquip == null) return;

        // 장착 중인 아이템은 여기서 판매하지 않음
        if (isEquippedItem) return;

        // 매니저를 통해 장비 개별 판매
        EquipmentManager.Instance.SellEquipment(currentEquip);

        // 인벤토리 새로고침 후 팝업 닫기
        if (mainPanel != null) mainPanel.RefreshUI();
        ClosePopup();
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}