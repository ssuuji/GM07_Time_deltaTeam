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

    public void SpawnEnemies(List<StageEnemyInfo> enemies)
    {
        foreach (StageEnemyInfo enemyInfo in enemies) // 목록을 순회 하면서 한명씩 생성
        {
            SpawnEnemy(enemyInfo);
        }
    }

    private void SpawnEnemy(StageEnemyInfo enemyInfo) // 적 한명을 지정한 슬롯 위치에 생성
    {
        Transform spawnPoint = GetSpawnPoint(enemyInfo.SpawnSlot);

        if (enemyInfo.EnemyPrefab == null || spawnPoint == null)
        {
            return;
        }

        CreateEnemy(enemyInfo, spawnPoint);
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
