using UnityEngine;
using TMPro;

public class UI_ItemPopup : MonoBehaviour
{
    [Header("팝업 UI 연결")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI gradeText;
    public TextMeshProUGUI statText;

    // 장비 정보를 받아서 팝업창에 세팅하고 켜는 함수
    public void OpenPopup(EquipmentInstance equip)
    {
        gameObject.SetActive(true); // 팝업창 켜기

        // 이름 표시
        nameText.text = equip.BaseData.equipmentName;

        // 등급 표시 및 텍스트 색상 변경
        gradeText.text = $"등급: {equip.Grade}";

        if (equip.Grade == EquipmentGrade.Epic)
        {
            gradeText.color = new Color(0.8f, 0.2f, 1f); // 에픽 = 보라색
        }
        else if (equip.Grade == EquipmentGrade.Rare)
        {
            gradeText.color = new Color(0.2f, 0.6f, 1f); // 레어 = 파란색
        }
        else
        {
            gradeText.color = Color.white; // 노말 = 흰색
        }

        // 주사위로 확정된 진짜 스탯 표시
        statText.text = $"공격력 : {equip.Attack}\n" +
                        $"방어력 : {equip.Defense}\n" +
                        $"체  력 : {equip.HP}";
    }

    // 팝업 닫기
    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}