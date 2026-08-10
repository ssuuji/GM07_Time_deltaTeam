using AFKHero.Battle;
using System.Collections;
using UnityEngine;

//StageDB를 들고있으면서, 특정 조건에 따라서 현재 스테이지를 다음 스테이지로 넘기게 할 관리자 클래스

public class StageManager : MonoBehaviour
{
    [Header("스테이지 데이터")]
    [SerializeField] private StageData stageData;

    [Header("적 생성")]
    [SerializeField] private EnemySpawner enemySpawner; //이걸 EnemySpawner가 아니라 BattleManager로 바꾸고,
    //EnemySpawner의 기능을 BattleSpawner가 일부 흡수하는 방향으로 가야할 것 같음.

    [SerializeField] private BattleManager battleManager;

    [Header("결과 패널")]
    [SerializeField] private RectTransform victoryPanel;
    [SerializeField] private RectTransform defeatPanel;


    [Header("현재 진행 정보")]
    [SerializeField] private int currentStageNumber = 1;
    [SerializeField] private int currentSectionNumber = 1;

    [Header("마지막으로 클리어한 스테이지")]
    [SerializeField] private int lastStageNumber;
    [SerializeField] private int lastSectionNumber;

    private StageInfo currentStageInfo; // 현재 진행 중인 구간의 데이터

    public StageInfo CurrentStageInfo => currentStageInfo;

    public int LastStageNumber => lastStageNumber;
    public int LastSectionNumber => lastSectionNumber;


    private void Awake()
    {
        victoryPanel.gameObject.SetActive(false);
        defeatPanel.gameObject.SetActive(false);
    }
    private void Start()
    {
        StartStage();

        //임시로 이벤트 구독
        battleManager.StateChanged += HandleBattleResult;
    }

    private void OnDestroy()
    {
        battleManager.StateChanged -= HandleBattleResult;
    }

    public void StartStage() // 스테이지 구간을 시작
    {
        victoryPanel.gameObject.SetActive(false);
        defeatPanel.gameObject.SetActive(false);

        currentStageInfo = stageData.GetStage(currentStageNumber, currentSectionNumber);

        if(currentStageInfo == null) // 스테이지 데이터가 없으면 진행 중단
        {
            print("스테이지 데이터 null");
            return;
        }

        //적의 레벨을 스테이지 기반으로 계산
        int enemyLevel = EnemyLevelCalculator.CalculateEnemyLevel(currentStageNumber, currentSectionNumber);       

        // [수정한 부분: 적 목록(Enemies)과 방금 계산한 레벨(enemyLevel)을 같이 넘겨줍니다]
        enemySpawner.SpawnEnemies(currentStageInfo.Enemies, enemyLevel);       
    }

    //전투상태가 변경되었을 때 이벤트로 호출되며, 승리 / 패배에 따라 다른 코드들을 실행하게 할 메서드
    public void HandleBattleResult(AFKHero.Battle.BattleState state)
    {
        switch(state)
        {
            case AFKHero.Battle.BattleState.Victory:
                HandleVictory();
                break;
            case AFKHero.Battle.BattleState.Defeat:
                HandleDefeat();
                break;
            default:
                break;
        }
    }

    //승리 시 호출될 메서드
    private void HandleVictory()
    {
        //승리했으니 현 스테이지의 보상에 해당하는 골드를 매니저를 통해 지급
        //CurrencyManager.Instance.AddGold(currentStageInfo.ClearGold);

        //마지막으로 클리어한 스테이지와 섹션의 값을 저장 => 방치 전투에서 활용.
        lastStageNumber = currentStageNumber;
        lastSectionNumber = currentSectionNumber;

        //TODO : SaveManager를 호출하여 값 저장

        //보스 스테이지가 아닌 상태에서 승리 시 섹션의 숫자만 증가시킨다.
        if (!currentStageInfo.IsBossStage)
        {
            currentSectionNumber += 1;
        }
        //보스 스테이지인 상태에서 승리 시 스테이지의 숫자를 증가시키고, 섹션의 숫자는 1로 초기화한다.
        else if (currentStageInfo.IsBossStage)
        {
            currentStageNumber += 1;
            currentSectionNumber = 1;
        }

        //반영된 결과를 바탕으로 다음 스테이지 시작. 승리 패널이 있다면 승리 패널 띄우는 부분으로 변경될 것
        victoryPanel.gameObject.SetActive(true);

    }

    private void HandleDefeat()
    {
        defeatPanel.gameObject.SetActive(true);
        //
    }
}
