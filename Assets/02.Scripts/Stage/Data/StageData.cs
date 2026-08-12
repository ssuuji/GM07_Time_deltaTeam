using System.Collections.Generic;
using UnityEngine;

//사실상 스테이지들의 정보를 모두 담을 DB역할을 할 클래스.
//새로운 캠페인이 추가되었을 때 사용할 수 있을 것 같기도.

[CreateAssetMenu(fileName = "StageData", menuName = "Game Data / Stage Data")]
public class StageData : ScriptableObject
{
    [Header("스테이지 목록")]
    [SerializeField] private List<StageInfo> stages = new();


    public IReadOnlyList<StageInfo> Stages => stages;


    public StageInfo GetStage(int stageNumber, int sectionNumber) // 스테이지 번호랑 구간 번호가 일치하는 데이터 찾기
    {
        foreach (StageInfo stage in stages)
        {
            if(stage.StageNumber == stageNumber && stage.SectionNumber == sectionNumber)
            {
                return stage;
            }
        }
        return null;
    }
}
