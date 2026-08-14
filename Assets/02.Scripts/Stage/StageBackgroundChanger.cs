using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//StageManager에게서 CurrentStageNumber를 받고,
//컬렉션에 저장되어 있는 이미지를 꺼내서
//배경화면을 Stage에 맞게 변경할 클래스.

//각 스테이지마다 배경음을 다르게 설정할 예정이라면, 이 클래스가 하는 기능이 사실상 그건데?

public class StageBackgroundChanger : MonoBehaviour
{
    [SerializeField] private List<Sprite> backgroundList;
    [SerializeField] private Image backgroundImage;


    public void ChangeBackground(int currentStage)
    {
        if(backgroundList == null || backgroundList.Count == 0)
        {
            Debug.LogWarning("[StageBackgroundChanger] : 리스트가 비어있습니다.");
            return;
        }

        //리스트는 0부터 시작하니까, 0번 index가 1스테이지 배경화면
        int index = currentStage - 1;

        //backgroundList의 갯수보다 index가 크다는 것은, 스테이지의 갯수가 리스트에 등록된 배경화면 갯수보다 많다는 것.
        if(index < 0 || index >= backgroundList.Count)
        {
            Debug.LogWarning("[StageBackgroundChanger] : stage번호에 맞춰 바꿀 배경화면의 index가 없습니다.");
            return;
        }

        if (backgroundList[index] == null)
        {
            Debug.LogWarning("[StageBackgroundChanger] : stage번호에 맞춰 바꿀 배경화면이 등록되지 않았습니다.");
            return;
        }

        //이미 같은 배경 이미지라면 바꾸지 않는다.
        if (backgroundImage.sprite == backgroundList[index]) return;

        backgroundImage.sprite = backgroundList[index];
    }
}
