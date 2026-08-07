using System.Collections.Generic;
using UnityEngine;

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
