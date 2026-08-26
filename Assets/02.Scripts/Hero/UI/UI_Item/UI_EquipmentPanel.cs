using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private void Start()
    {
        if (autoEquipButton != null) autoEquipButton.onClick.AddListener(OnClickAutoEquip);
    }

    private void OnEnable() => RefreshUI();

    private void OnClickAutoEquip()
    {
        if (currentHero != null)
        {
            EquipmentManager.Instance.AutoEquip(currentHero);
            RefreshUI();
        }
    }

    public void OnClickBulkSell()
    {
        EquipmentManager.Instance.BulkSell();
        RefreshUI();
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

        // 가방에서 EquipmentInstance(진짜 장비)를 꺼내오도록 타입 변경!
        foreach (EquipmentInstance equip in EquipmentManager.Instance.equipmentInventory)
        {
            GameObject go = Instantiate(itemSlotPrefab, inventoryContent);
            UI_ItemSlot slot = go.GetComponent<UI_ItemSlot>();
            slot.Setup(equip, currentHero, this, false);
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