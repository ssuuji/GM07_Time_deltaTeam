using System;
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

    //엄... 시작 시에만 호출하여 배경화면을 셋팅하는 메서드
    public void SetBackgroundOnStart(int currentStageNumber, int lastStageNumber, int lastSectionNumber, StageInfo currentStageInfo)
    {
        if (backgroundList == null || backgroundList.Count == 0)
        {
            Debug.LogWarning("[StageBackgroundChanger] : 리스트가 비어있습니다.");
            return;
        }

        //예외처리를 잘 해둬서... null인 경우는 엥간하면 마지막 스테이지 깬 다음이지만...
        //이것 참 묘하네.
        //로직상 문제가 있네. 이전 스테이지 정보를 확인해봐야 할 것 같은데?
        if(currentStageInfo == null)
        {
            //현재 스테이지 정보가 null이다 = 이미 데이터베이스에 존재하는 마지막 스테이지를 깨둔 상태다.
            //그렇다면, 이전 스테이지를 봐서 그게 보스스테이지인지 아닌지 확인해야 할 듯.
            //이런 식으로 하지말고, 스테이지매니저에서 잘 검사해서 값을 넘겨주는 게 좋을 것 같은데,
            //그 부분은 리팩토링의 영역으로 남겨둬야 할 듯.
            StageInfo baseStage = StageManager.Instance.StageData.GetStage(lastStageNumber, lastSectionNumber);

            if(baseStage == null)
            {
                Debug.LogWarning("[StageBackgroundChanger] : 잘못된 값이 클리어 기록으로 들어있거나, 처음 시작한 겁니다.");
                return;
            }

            //만일 보스스테이지를 깼는데 다음 스테이지가 없는 경우(4-5를 깼는데 5-1이 없는 경우)
            if (baseStage.IsBossStage)
            {
                ChangeBackground(currentStageNumber - 1); //기존에 바꾸던 방식에서 1빼서 바꿔달라고 하기
            }
            else if(!baseStage.IsBossStage)
            {
                ChangeBackground(currentStageNumber);
            }          
        }
        else
        {
            ChangeBackground(currentStageNumber);
        }

    }
}
