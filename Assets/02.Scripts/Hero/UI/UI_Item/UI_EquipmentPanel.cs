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

    [Header("새로 추가한 버튼들")]
    public UnityEngine.UI.Button BulkSellButton;

    private void Start()
    {
        if (autoEquipButton != null)
        {
            autoEquipButton.onClick.AddListener(OnClickAutoEquip);
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }

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
        // 영웅 선택 상관없이 가방 먼저 보이기
        foreach (Transform child in inventoryContent)
        {
            Destroy(child.gameObject);
        }

        foreach (EquipmentData equip in EquipmentManager.Instance.equipmentInventory)
        {
            GameObject go = Instantiate(itemSlotPrefab, inventoryContent);
            UI_ItemSlot slot = go.GetComponent<UI_ItemSlot>();
            slot.Setup(equip, currentHero, this, false);
        }

        // 가방을 다 그렸다면 영웅이 있는지 검사
        if (currentHero == null) return;

        // 영웅이 있다면 스탯과 장착 슬롯 갱신
        if (hpText != null) hpText.text = currentHero.FinalMaxHP.ToString();
        if (attackText != null) attackText.text = currentHero.FinalAttack.ToString();
        if (defenseText != null) defenseText.text = currentHero.FinalDefense.ToString();

        if (weaponSlot != null) weaponSlot.Setup(currentHero.equippedWeapon, currentHero, this, true);

        weaponSlot.Setup(currentHero.equippedWeapon, currentHero, this, true);
        armorSlot.Setup(currentHero.equippedArmor, currentHero, this, true);
        pantsSlot.Setup(currentHero.equippedPants, currentHero, this, true);
        helmetSlot.Setup(currentHero.equippedHelmet, currentHero, this, true);
    }
}