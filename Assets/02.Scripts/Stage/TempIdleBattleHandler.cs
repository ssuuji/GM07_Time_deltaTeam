using AFKHero.Battle;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;


//유닛들 클리어도 이벤트 구독을 시켜놓고, StageManager가 Working상태일 때 지워버리게 되어 있습니다.
public class TempIdleBattleHandler : MonoBehaviour
{
    [Header("배치")]
    [SerializeField] private FormationData formationData;
    [SerializeField] private Transform idleBattleOrigin;
    [SerializeField] private Transform unitContainer;
    [SerializeField] private List<GameObject> copiedPrefabs;

    [Header("더미")]
    [SerializeField] private Transform dummyTarget;

    private Coroutine attackCoroutine;


    private void Awake()
    {
        ToggleDummyTarget(false);
    }
    void Start()
    {
        StageManager.Instance.StageStateChanged += StartIdleBattle;
        StageManager.Instance.StageStateChanged += ClearIdleBattle;
    }

    //이벤트를 구독해제하는 과정에서(게임 종료 시점에서) NullReferenceException이 발생하고,
    //그 결과로 게임이 저장되지 않는 현상이 있습니다.
    private void OnDestroy()
    {
        StageManager.Instance.StageStateChanged -= StartIdleBattle;
        StageManager.Instance.StageStateChanged -= ClearIdleBattle;
    }


    //StageManager의 이벤트 발행에 의해 실행됩니다.
    public void StartIdleBattle(StageState state)
    {
        if (state != StageState.Idle) return;

        StageInfo baseStage = StageManager.Instance.StageData.GetStage(StageManager.Instance.LastStageNumber, StageManager.Instance.LastSectionNumber);
        if (baseStage == null) return;

        SpawnParty();

        ToggleDummyTarget(true);   

    }

    private void SpawnParty()
    {
        //현재 파티 슬롯의 배치 상태를 복사
        HeroInstance[] heroes = PartyManager.Instance.partySlots;

        Debug.Log(heroes.Length);

        for (int i = 0; i < heroes.Length; i++)
        {
            //배치되어 있지 않다면 건너뛰기
            if (heroes[i] == null) continue;

            //formationData를 통해 월드 좌표로 변환
            Vector3 spawnPosition = formationData.GetWolrdPosition(TeamType.Ally, i, idleBattleOrigin.position);

            //데이터에 있는 프리팹만 생성하여, GameObject형 리스트에 추가
            copiedPrefabs.Add(Instantiate(heroes[i].data.HeroPrefab, spawnPosition, Quaternion.identity));

            //이 부분에서 문제가 발생하는 것으로 추정.
            //가령, 파티를 4번에 하나만 배치했다면 copiedPrefab에는 0번으로 추가된다
            //현재 파티 목록을 완전히 복사하고, null일 때는 실행하지 않아야 하며, 번호를 맞춰야 한다.
            UnitAnimationController anim = copiedPrefabs[i].AddComponent<UnitAnimationController>();

            anim.Initialize(null);
        }

        attackCoroutine = StartCoroutine(ShowAttackCo());
    }


    //코루틴으로 공격 모션을 실행시키려고 했으나, 디버그 로그는 출력됨에도 실행은 안 되는 현상이 있습니다.
    private IEnumerator ShowAttackCo()
    {
        while(true)
        {

            for (int i = 0; i < copiedPrefabs.Count; i++)
            {
                UnitAnimationController anim = copiedPrefabs[i].GetComponent<UnitAnimationController>();
                anim.PlayAttack(JobType.Warrior);
                Debug.Log("공격 실행됨");
            }

            yield return new WaitForSeconds(1.0f);
        }

    }


    private void ToggleDummyTarget(bool toggle)
    {
        if(dummyTarget == null)
        {
            Debug.LogWarning("더미 타겟이 없습니다.");
            return;
        }

        dummyTarget.gameObject.SetActive(toggle);
    }


    //StageManager가 Working 상태로 전환될 때 정리할 메서드
    public void ClearIdleBattle(StageState state)
    {
        if (state != StageState.Working) return;

        ToggleDummyTarget(false);

        if(attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        for (int i = 0; i < copiedPrefabs.Count; i++)
        {
            Destroy(copiedPrefabs[i].gameObject);
        }
        //내부 요소만 지우고 할 순 없는 건지.
        copiedPrefabs.Clear();
    }

}
