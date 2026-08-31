using AFKHero.Battle;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

/*
[방치 전투의 조건(파기된 안건입니다)]
1. 플레이어의 파티가 소환되어야 한다.
2. 궁극기는 사용하지 않는다. 
3. 플레이어의 파티는 죽거나 체력이 닳아선 안 된다.
4. 적 중 2마리 정도 오게 하고, 그 두 마리가 다 죽으면 똑같이 새로 소환해서 계속 전투하게 한다.
5. 보상은 적이 죽었을 때, 현재 스테이지 기반으로 골드를 측정하여 PlayerManager의 AddGold로 지급하게 한다.
*/

//StageManager의 상태를 확인하고, Idle상태로 전환됐을 때 방치 전투의 흐름을 시작하게 하는 클래스
//또한, StageManager가 Working 상태로 전환됐을 때, 기존의 흐름을 모두 정리해야 한다.

public class IdleBattleHandler : MonoBehaviour
{
    [Header("배치")]
    [SerializeField] private FormationData formationData;
    [SerializeField] private Transform idleBattleOrigin;
    [SerializeField] private Transform unitContainer;
    [SerializeField] private List<GameObject> copiedPrefabs;

    //패널 같은 것도 추가해서, "훈련중" 텍스트 띄우게끔.

    [Header("더미")]
    [SerializeField] private Transform dummyTarget;

    [Header("보상 지급 및 애니메이션 실행 주기")]
    [SerializeField] private float rewardTime = 1.0f;
    private WaitForSeconds wait;

    private Coroutine idleBattleCoroutine;


    private void Awake()
    {
        ToggleDummyTarget(false);
        wait = new WaitForSeconds(rewardTime);
    }
    void Start()
    {
        StageManager.Instance.StageStateChanged += StartIdleBattle;
        StageManager.Instance.StageStateChanged += ClearIdleBattle;
    }

    //이벤트를 구독해제하는 과정에서(게임 종료 시점에서) NullReferenceException이 발생하고,
    //그 결과로 게임이 저장되지 않는 현상이 있었습니다.
    //임시로 StageManager가 null이 아닐 때만 구독해제 하게 했습니다.
    private void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.StageStateChanged -= StartIdleBattle;
            StageManager.Instance.StageStateChanged -= ClearIdleBattle;
        }
    }

    //StageManager의 이벤트 발행에 의해, Idle 상태로 전환되었을 때만 방치 전투가 실행됩니다.
    public void StartIdleBattle(StageState state)
    {
        //StageManager가 Idle이 아니거나, 올바르지 않은 스테이지 번호가 클리어 기록으로 들어있다면 실행하지 않습니다.
        if (state != StageState.Idle) return;

        StageInfo baseStage = StageManager.Instance.StageData.GetStage(StageManager.Instance.LastStageNumber, StageManager.Instance.LastSectionNumber);
        if (baseStage == null) return;

        //파티슬롯길이만큼 내부 요소가 null인 것을 찾았다 = 파티가 전부 비어있다.
        if (CheckPartySlot() == PartyManager.Instance.partySlots.Length)
        {
            Debug.Log("파티가 없어서 실행 안 됨");
            return;
        }

        SpawnParty();

        ToggleDummyTarget(true);

        //이걸 이제 발전시킨다면, 그런 식으로 할 수 있겠지.
        //이 메서드가 실행되는 시점에서 파티의 정보를 불러오고,
        //배치한 영웅 수나 영웅들 중 가장 낮거나 높은 레벨도 계산에 반영하도록 CalculateIdleBattleReward를 수정할 수도 있겠지.

        int IdleReward = StageCalculator.CaculateIdleBattleReward(StageManager.Instance.LastStageNumber, StageManager.Instance.LastSectionNumber);

        idleBattleCoroutine = StartCoroutine(HandleIdleBattleCo(IdleReward));
    }

    //파티슬롯이 전부 비어있는지 체크할 메서드
    private int CheckPartySlot()
    {
        int slotCount = 0;

        HeroInstance[] heroes = PartyManager.Instance.partySlots;

        for (int i = 0; i < heroes.Length; i++)
        {
            if (heroes[i] == null)
            {
                slotCount++;
            }
        }

        return slotCount;

    }

    private void SpawnParty()
    {
        //현재 파티 슬롯의 배치 상태를 복사
        HeroInstance[] heroes = PartyManager.Instance.partySlots;

        Debug.Log(heroes.Length);

        for (int i = 0; i < heroes.Length; i++)
        {
            //formationData를 통해 월드 좌표로 변환
            Vector3 spawnPosition = formationData.GetWolrdPosition(TeamType.Ally, i, idleBattleOrigin.position);

            //데이터에 있는 프리팹만 생성하여, GameObject형 리스트에 추가

            if (heroes[i] != null && heroes[i].data != null)
            {
                GameObject copiedPrefab = Instantiate(heroes[i].data.HeroPrefab, spawnPosition, Quaternion.identity, unitContainer);

                copiedPrefabs.Add(copiedPrefab);
            }

            /*
            //이 부분에서 문제가 발생하는 것으로 추정.
            //가령, 파티를 4번에 하나만 배치했다면 copiedPrefab에는 0번으로 추가된다
            //현재 파티 목록을 완전히 복사하고, null일 때는 실행하지 않아야 하며, 번호를 맞춰야 한다.
            Animator anim = copiedPrefabs[i].GetComponent<Animator>();
            */
        }
    }

    private IEnumerator HandleIdleBattleCo(int IdleReward)
    {
        while (true)
        {
            //GetComponentInChildren을 남발하여 성능저하를 일으킬 가능성이 높은데 이거 어떻게 할지.
            for (int i = 0; i < copiedPrefabs.Count; i++)
            {
                //여기서 NullReferenceException이 우연히도 일어나지 않는데, 안정적이지 않은 방식으로 for문 돌리고 있음.
                Animator anim = copiedPrefabs[i].GetComponentInChildren<Animator>();
                anim.SetTrigger("2_Attack");
            }

            yield return wait;
            Debug.Log($"[IdleBattleHandler] : {rewardTime} 초 기다렸습니다");
            AFKHero.Player.PlayerManager.Instance.AddGold(IdleReward);
            Debug.Log($"[IdleBattleHandler] : {IdleReward} 지급함");
        }
    }

    private void ToggleDummyTarget(bool toggle)
    {
        if (dummyTarget == null)
        {
            Debug.LogWarning("[IdleBattleHandler] : 더미 타겟이 없습니다.");
            return;
        }

        dummyTarget.gameObject.SetActive(toggle);
    }

    //StageManager가 Working 상태로 전환될 때 방치 전투 현황을 모두 정리할 메서드
    public void ClearIdleBattle(StageState state)
    {
        if (state != StageState.Working) return;

        //더미 타겟을 끕니다.
        ToggleDummyTarget(false);

        //코루틴을 멈추고 비웁니다.
        if (idleBattleCoroutine != null)
        {
            StopCoroutine(idleBattleCoroutine);
            idleBattleCoroutine = null;
        }

        //내부에 있는 프리팹들을 전부 파괴합니다.
        for (int i = 0; i < copiedPrefabs.Count; i++)
        {
            Destroy(copiedPrefabs[i].gameObject);
        }

        //리스트를 초기화합니다.
        copiedPrefabs.Clear();
    }
}
