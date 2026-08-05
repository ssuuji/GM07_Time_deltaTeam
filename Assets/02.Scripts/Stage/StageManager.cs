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

        enemySpawner.SpawnEnemies(currentStageInfo.Enemies); // 현재 구간의 적 목록을 enemySpawner에 전달
    }


}
