using UnityEngine;


public class StageManager : MonoBehaviour
{
    [Header("스테이지 데이터")]
    [SerializeField] private StageData stageData;

    [Header("적 생성")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("현재 진행 정보")]
    [SerializeField] private int currentStageNumber = 1;
    [SerializeField] private int currentSectionNumber = 1;

    private StageInfo currentStageInfo; // 현재 진행 중인 구간의 데이터

    public StageInfo CurrentStageInfo => currentStageInfo;

    private void Start()
    {
        StartStage();   
    }

    public void StartStage() // 스테이지 구간을 시작
    {
        currentStageInfo = stageData.GetStage(currentStageNumber, currentSectionNumber);

        if(currentStageInfo == null) // 스테이지 데이터가 없으면 진행 중단
        {
            print("스테이지 데이터 null");
            return;
        }

        //적의 레벨을 스테이지 기반으로 계산
        int enemyLevel = EnemyLevelCalculator.CalculateEnemyLevel(currentStageNumber, currentSectionNumber);

        //TODO : HeroBase(예정)에 구현될 레벨 창구를 통해 해당 값을 전달하여, 적의 레벨을 변경하고 그에 따라 스탯 변화
        //HeroBase가 아니라 Stage쪽 스크립트에서 레벨을 설정할 수 있도록 구현할 것. 주말 정도에 작업하신다고 하시니 충돌 안 나게 관리하기.

        enemySpawner.SpawnEnemies(currentStageInfo.Enemies); // 현재 구간의 적 목록을 enemySpawner에 전달
    }

    //여기에서 다음 스테이지로 넘기는 public 메서드가 하나 있어야 할 듯
    public void HandleBattleResult(AFKHero.Battle.BattleState state)
    {   
        //전투 결과를 처리하는 메서드이니, 패배했을 때 버튼을 띄우는 메서드도 얘가 호출해야 하지 않나?
        //그렇다면 state가 승리, 또는 패배가 아닐 때는 실행하지 않게 하고,
        //switch나 if문을 통해 패배 / 일반 스테이지 승리/ 보스 스테이지 승리를 처리해야 할 것.
        if (state != AFKHero.Battle.BattleState.Victory) return;
        
        //승리했으니 현 스테이지의 보상에 해당하는 골드를 매니저를 통해 지급
        CurrencyManager.Instance.AddGold(currentStageInfo.ClearGold);

        //보스 스테이지가 아닌 상태에서 승리 시 섹션의 숫자만 증가시킨다.
        if(!currentStageInfo.IsBossStage)
        {
            currentSectionNumber += 1;
        }

        //보스 스테이지인 상태에서 승리 시 스테이지의 숫자를 증가시키고, 섹션의 숫자는 1로 초기화한다.
        if(currentStageInfo.IsBossStage)
        {
            currentStageNumber += 1;
            currentSectionNumber = 1;
        }


        //반영된 결과를 바탕으로 다음 스테이지 시작
        StartStage();


    }
}
