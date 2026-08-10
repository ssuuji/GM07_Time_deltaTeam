using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 포인트")] // 적의 좌표 배치
    [SerializeField] private Transform frontLeft;
    [SerializeField] private Transform frontRight;
    [SerializeField] private Transform backLeft;
    [SerializeField] private Transform backCenter;
    [SerializeField] private Transform backRight;

    // 수정한 부분: StageInfo 전체가 아니라, 적 목록 리스트와 몬스터 레벨을 매개변수로 받기
    public void SpawnEnemies(List<StageEnemyInfo> enemies, int enemyLevel)
    {
        foreach (StageEnemyInfo enemyInfo in enemies)
        {
            SpawnEnemy(enemyInfo, enemyLevel); // 받아온 레벨 전달
        }
    }

    private void SpawnEnemy(StageEnemyInfo enemyInfo, int enemyLevel)
    {
        Transform spawnPoint = GetSpawnPoint(enemyInfo.SpawnSlot);

        if (enemyInfo.EnemyPrefab == null || spawnPoint == null) return;

        GameObject spawnedEnemy = CreateEnemy(enemyInfo, spawnPoint);

        // [추가된 부분: 생성된 적의 스크립트를 찾아 레벨과 적군 상태를 세팅해 줌]
        HeroBase enemyScript = spawnedEnemy.GetComponent<HeroBase>();
        if (enemyScript != null)
        {
            // 수정된 부분: 인스펙터에 등록해둔 몬스터 원본 데이터를 가져오기
            HeroData data = enemyScript.defaultEnemyData;
            if (data != null)
            {
                HeroInstance monsterInstance = new HeroInstance(data, true);
                monsterInstance.level = enemyLevel; // 스테이지 레벨 적용

                // 적군(true), 일반몹(true)으로 Init 호출
                enemyScript.Init(monsterInstance, true, true);
            }
        }
    }

    private GameObject CreateEnemy(StageEnemyInfo enemyInfo, Transform spawnPoint) // 적 생성 부분  추후에 풀 매니저 생겼을 때 수정 
    {
        return Instantiate(enemyInfo.EnemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    private Transform GetSpawnPoint(EnemySpawnSlot spawnSlot) // 스폰 위치를 반환
    {
        switch (spawnSlot)
        {
            case EnemySpawnSlot.FrontLeft:
                return frontLeft;

            case EnemySpawnSlot.FrontRight:
                return frontRight;

            case EnemySpawnSlot.BackLeft:
                return backLeft;

            case EnemySpawnSlot.BackCenter:
                return backCenter;

            case EnemySpawnSlot.BackRight:
                return backRight;

            default:
                return null;
        }
    }
}
