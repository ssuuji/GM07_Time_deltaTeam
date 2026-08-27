using UnityEngine;

//StageManager의 상태를 확인하고, Idle상태일 때 방치 전투의 흐름을 시작하게 하는 클래스
public class IdleBattleHandler : MonoBehaviour
{
  
    void Start()
    {
        StageManager.Instance.StageStateChanged += StartIdleBattle;
    }

    private void OnDestroy()
    {
        StageManager.Instance.StageStateChanged -= StartIdleBattle;
    }


    /*
    [방치 전투의 조건]
    1. 플레이어의 파티가 소환되어야 한다.
    2. 궁극기는 사용하지 않는다. 
    3. 플레이어의 파티는 죽거나 체력이 닳아선 안 된다.
    4. 적 중 2마리 정도 오게 하고, 그 두 마리가 다 죽으면 똑같이 새로 소환해서 계속 전투하게 한다.
    5. 보상은 적이 죽었을 때, 현재 스테이지 기반으로 골드를 측정하여 PlayerManager의 AddGold로 지급하게 한다.
    */

    //방치 전투를 실행할 메서드
    //Idle 상태가 아니거나, 클리어 기록이 올바르지 않을 경우 실행하지 않습니다.
    public void StartIdleBattle(StageState state)
    {
        if (state != StageState.Idle) return;

        StageInfo baseStage = StageManager.Instance.StageData.GetStage(StageManager.Instance.LastStageNumber, StageManager.Instance.LastSectionNumber);
        if (baseStage ==null) return;


    }



    private void SpawnParty()
    {

    }
}
