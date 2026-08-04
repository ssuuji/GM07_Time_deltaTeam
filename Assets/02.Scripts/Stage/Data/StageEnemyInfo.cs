using System;
using UnityEngine;

[Serializable]
public class StageEnemyInfo 
{
    [Header("등장할 적 프리팹")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("적이 등장할 배치 위치")]
    [SerializeField] private EnemySpawnSlot spawnSlot;

    public GameObject EmeyPrefab => enemyPrefab;
    public EnemySpawnSlot SpawnSlot => spawnSlot;
}
