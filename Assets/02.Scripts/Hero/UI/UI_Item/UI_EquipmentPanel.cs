using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UI_EquipmentPanel : MonoBehaviour
{
    [Header("현재 선택된 영웅")]
    public HeroInstance currentHero;

    [Header("영웅 스탯 텍스트")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;

    [Header("장착 중인 장비 슬롯 4개")]
    public UI_ItemSlot weaponSlot;
    public UI_ItemSlot armorSlot;
    public UI_ItemSlot pantsSlot;
    public UI_ItemSlot helmetSlot;

    [Header("가방(인벤토리) UI")]
    public Transform inventoryContent;
    public GameObject itemSlotPrefab;

    [Header("간편 장착 버튼")]
    public Button autoEquipButton;
    public Button BulkSellButton;

    [Header("장비 정보 팝업창")]
    public UI_ItemPopup itemPopup;

    // ====================================
    // 다중 선택 판매 시스템 변수들
    // ====================================
    [Header("다중 판매 시스템")]
    public Button toggleSellModeButton; // 판매 모드 켜기/끄기 버튼
    public TextMeshProUGUI toggleSellModeText; // 버튼 글씨
    public Button sellSelectedButton; // "선택한 장비 팔기" 실행 버튼

    public bool isSellMode = false; // 현재 판매 모드인지 상태 저장
    public List<EquipmentInstance> selectedItemsToSell = new List<EquipmentInstance>(); // 체크한 아이템 장바구니

    private void Start()
    {
        if (autoEquipButton != null) autoEquipButton.onClick.AddListener(OnClickAutoEquip);

        // 일괄 판매 버튼
        if (BulkSellButton != null) BulkSellButton.onClick.AddListener(OnClickBulkSell);

        // 선택 판매 모드 전환 버튼 연결
        if (toggleSellModeButton != null) toggleSellModeButton.onClick.AddListener(ToggleSellMode);

        // 선택한 아이템 판매 실행 버튼 연결
        if (sellSelectedButton != null) sellSelectedButton.onClick.AddListener(OnClickSellSelected);
    }

    private void OnEnable()
    {
        isSellMode = false; // 창을 열 땐 항상 일반 모드로 초기화
        selectedItemsToSell.Clear();
        RefreshUI();
    }

    private void OnClickAutoEquip()
    {
        if (currentHero != null)
        {
            EquipmentManager.Instance.AutoEquipHero(currentHero);
            RefreshUI();
        }
    }

    public void OnClickBulkSell()
    {
        EquipmentManager.Instance.BulkSell();
        RefreshUI();
    }

    // =========================
    // 선택 판매 모드 로직
    // =========================
    public void ToggleSellMode()
    {
        isSellMode = !isSellMode;
        selectedItemsToSell.Clear(); // 모드가 바뀔 때마다 장바구니 비우기

        if (toggleSellModeText != null)
            toggleSellModeText.text = isSellMode ? "판매 취소" : "선택 판매";

        // 선택 판매 실행 버튼은 판매 모드일 때만 켜지게 설정
        if (sellSelectedButton != null)
            sellSelectedButton.gameObject.SetActive(isSellMode);

        RefreshUI(); // 화면을 새로고침해서 체크박스 상태 초기화
    }

    public void ToggleItemSelection(EquipmentInstance item, UI_ItemSlot slot)
    {
        if (selectedItemsToSell.Contains(item))
        {
            selectedItemsToSell.Remove(item);
            slot.SetCheckmark(false); // 장바구니에 있으면 빼고 체크 해제
        }
        else
        {
            selectedItemsToSell.Add(item);
            slot.SetCheckmark(true); // 없으면 넣고 체크 표시
        }
    }

    public void OnClickSellSelected()
    {
        if (selectedItemsToSell.Count == 0) return;

        EquipmentManager.Instance.SellSelectedEquipments(selectedItemsToSell);

        ToggleSellMode(); // 다 팔았으니 일반 모드로 돌아가면서 자동 새로고침
    }

    public void OpenPanel(HeroInstance hero)
    {
        currentHero = hero;
        gameObject.SetActive(true);
        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (Transform child in inventoryContent) Destroy(child.gameObject);

        foreach (EquipmentInstance equip in EquipmentManager.Instance.equipmentInventory)
        {
            GameObject go = Instantiate(itemSlotPrefab, inventoryContent);
            UI_ItemSlot slot = go.GetComponent<UI_ItemSlot>();
            slot.Setup(equip, currentHero, this, false);

            // UI 새로고침 시점에 이미 선택된 아이템이라면 체크박스를 켜줌
            if (isSellMode && selectedItemsToSell.Contains(equip))
            {
                slot.SetCheckmark(true);
            }
        }

        if (currentHero == null) return;

        if (hpText != null) hpText.text = currentHero.FinalMaxHP.ToString();
        if (attackText != null) attackText.text = currentHero.FinalAttack.ToString();
        if (defenseText != null) defenseText.text = currentHero.FinalDefense.ToString();

        if (weaponSlot != null) weaponSlot.Setup(currentHero.equippedWeapon, currentHero, this, true);
        if (armorSlot != null) armorSlot.Setup(currentHero.equippedArmor, currentHero, this, true);
        if (pantsSlot != null) pantsSlot.Setup(currentHero.equippedPants, currentHero, this, true);
        if (helmetSlot != null) helmetSlot.Setup(currentHero.equippedHelmet, currentHero, this, true);
    }
}