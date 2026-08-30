using UnityEngine;

//StageNumber와 SectionNumber를 받아 필요한 계산을 수행할 정적 클래스

public static class StageCalculator 
{

    //적이 소환되는 시점에서 호출할 int형 정적 메서드
    //최소레벨 1, 최대레벨 9999로 임의로 설정했으며,
    //현 시점의 계산 공식은 (stageNumber -1) * 10 + stageNumber입니다. ex) 2-1스테이지면 11을 반환
    //한 스테이지에서 적들마다 다른 레벨을 가질 수 있도록 구현하고자 한다면
    //매개변수로 기본 레벨을 받도록 수정하거나,
    //값을 덮어씌우는 게 아닌 더하는 방식으로 구현하면 될 것 같습니다.
    public static int CalculateEnemyLevel(int stageNumber, int sectionNumber)
    {
        return Mathf.Clamp((stageNumber - 1) * 10 + sectionNumber, 1, 9999);
    }

    public static int CaculateIdleBattleReward(int lastStageNumber, int lastSectionNumber)
    {
        int IdleBattleRewardPerSecond = (lastStageNumber - 1) * 10 + lastSectionNumber * 5;

        return IdleBattleRewardPerSecond;
    }
}
