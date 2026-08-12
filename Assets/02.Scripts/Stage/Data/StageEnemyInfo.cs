using AFKHero.Battle;
using System;
using UnityEngine;

//해당 스테이지에서 소환할 적의 목록을 담는 컨테이너형 클래스.
//소환 위치는 이곳에서 결정하지 말고, 소환할 곳에서 담당한다.


//근데, 생각해보니까 어차피 이 클래스는 StageInfo에서 List로 관리된다.
//이미 구현된 UnitData같은 다른 클래스를 StageInfo가 List로 필드로 가지면 되는 거 아닌가?

//StageInfo 스크립트에서, List<StageEnemyInfo>를 다른 걸로 교체해버리면, 이 클래스 자체가 필요가 없어진다.

//그런데, 엥간하면 영웅 쪽에서 만든 Data를 사용하는 방식으로 BattleSpawner, StageInfo를 수정하는 게 맞기 때문에
//UnitData를 활용하는 방안은 BattleSpawner가 동작하기 위한 임시방편일 뿐이다.

//결론적으로, 영웅 쪽 데이터와 전투시스템 쪽 데이터 중 어느 쪽을 사용해야 할 지 결정해야 한다.


[Serializable]
public class StageEnemyInfo 
{
    [Header("등장할 적 프리팹")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("적이 등장할 배치 위치")]
    [SerializeField] private EnemySpawnSlot spawnSlot;

    [SerializeField] private UnitData unitData;

    public GameObject EnemyPrefab => enemyPrefab;
    public EnemySpawnSlot SpawnSlot => spawnSlot;
    public UnitData UnitData => unitData;
}
