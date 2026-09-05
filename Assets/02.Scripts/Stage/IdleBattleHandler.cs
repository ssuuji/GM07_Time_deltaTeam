using AFKHero.Battle;
using AFKHero.Quest;
using AFKHero.Sound;
using AFKHero.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;


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

//그러니까, 하드코딩이란 게 약간 이런 거지.
//이걸 그대로 뜯어가서 바로 쓸 수 없잖아. 이걸 위해서 텍스트 출력 오브젝트도 만들어야 하니까.
//뭐랄까... 근데 얘가 UI적으로 하는 일이 딱 하나밖에 없어서 얘를 위한 클래스를 파는 것도 좀 애매하고.
//바란스를 잘 잡아지.

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

    [Header("방치전투 현황 출력")]
    [SerializeField] private TMP_Text idleBattleText;

    [Header("보상 지급 및 애니메이션 실행 주기")]
    [SerializeField] private float rewardTime = 1.0f;
    private WaitForSeconds wait;
    private int idleReward;     

    [Header("장비드랍확률")]
    [Tooltip("장비가 드롭될 확률 (0 ~ 100%)")]
    [Range(0f, 100f)]
    [SerializeField] private float equipmentRate = 0.3f;

    private Coroutine idleBattleCoroutine;


    private void Awake()
    {
        ToggleDummyTarget(false);
        wait = new WaitForSeconds(rewardTime);
    }
    void Start()
    {

        //0905 : Editor상에서는 문제없이 동작하나, 빌드 파일에서는 방치 전투가 실행되지 않는 문제가 있었습니다.
        //StageManager의 Start가 먼저 실행되어 상태가 Idle로 전환되면, 이벤트 발행을 놓치게 되는 문제입니다.
        //따라서 이미 Idle이라면 강제로 최초 1회 실행하도록 방어코드를 작성해야 합니다.
        //Start에서 무언가 실행되어야 하는 경우는 이와 같이 작성합니다.
        if (StageManager.Instance != null)
        {
            StageManager.Instance.StageStateChanged += StartIdleBattle;
            StageManager.Instance.StageStateChanged += ClearIdleBattle;

            // 게임 시작 시 이벤트 구독 전 이미 Idle 상태로 진입해버린 경우를 대비해 직접 실행
            if (StageManager.Instance.CurrentState == StageState.Idle)
            {
                StartIdleBattle(StageState.Idle);
            }
        }

        if (PartyManager.Instance != null)
        {
            PartyManager.Instance.partyChanged += OnPartyChanaged;
        }
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
        if(PartyManager.Instance != null)
        {
            PartyManager.Instance.partyChanged -= OnPartyChanaged;
        }

    }

    //StageManager의 이벤트 발행에 의해, Idle 상태로 전환되었을 때만 방치 전투가 실행됩니다.
    public void StartIdleBattle(StageState state)
    {
        //실행방지 코드가 너무 긴데, 저 조건들을 다 검사해서 bool로 반환하는 거 써도 되긴 할 듯.
        //각자마다 디버그로그 다르게 찍고 싶으면 상관없지만.

        //StageManager가 Idle이 아니거나, 올바르지 않은 스테이지 번호가 클리어 기록으로 들어있다면 실행하지 않습니다.
        if (state != StageState.Idle)
        {
            RefreshRewardText(false);
            return;
        }

        StageInfo baseStage = StageManager.Instance.StageData.GetStage(StageManager.Instance.LastStageNumber, StageManager.Instance.LastSectionNumber);
        if (baseStage == null)
        {
            RefreshRewardText(false);
            return;
        }

        //파티슬롯길이만큼 내부 요소가 null인 것을 찾았다 = 파티가 전부 비어있다.
        if (CheckPartySlot() == PartyManager.Instance.partySlots.Length)
        {
            Debug.Log("파티가 없어서 실행 안 됨");
            RefreshRewardText(false);
            return;
        }

        if(idleBattleCoroutine != null)
        {
            StopCoroutine(idleBattleCoroutine);
            idleBattleCoroutine = null;
        }


        SpawnParty();

        ToggleDummyTarget(true);

        //이걸 이제 발전시킨다면, 그런 식으로 할 수 있겠지.
        //이 메서드가 실행되는 시점에서 파티의 정보를 불러오고,
        //배치한 영웅 수나 영웅들 중 가장 낮거나 높은 레벨도 계산에 반영하도록 CalculateIdleBattleReward를 수정할 수도 있겠지.




        //겁나 이상하긴 한데 이렇게 될걸?
        idleReward = StageCalculator.CaculateIdleBattleReward(StageManager.Instance.LastStageNumber, StageManager.Instance.LastSectionNumber)+

      
        PartyManager.Instance.partySlots.Length - CheckPartySlot();

        Debug.Log($"<color=cyan>[IdleBattleHandler]</color> : 스테이지 계산값 {StageCalculator.CaculateIdleBattleReward(StageManager.Instance.LastStageNumber, StageManager.Instance.LastSectionNumber)} 에서 파티슬롯 {PartyManager.Instance.partySlots.Length} 을 더하고 {CheckPartySlot()}을 빼서 보상을 설정했습니다.");


        //CheckPartySlot이 반대로 되어야 해.
        //아예 int slotCount = PartManager.Instance.partySlots.Length 이렇게 하고
        //slocCount--; 해야지.]
        //그리고 위에서 == 0으로 검사하고.
        //딸랑 얘만 쓰는 텍스트 필드니까... 솔직히 기능 클래스여도 텍스트 하나정돈 괜찮잖아?


        //이거... UINoticePopup쓰지 말고, 그렇게 하자.
        //어차피 "훈련중" 텍스트 뜨잖아? 그니까 그 아래 정도에 매 초마다 몇 골드 지급 이것만 띄우고
        //저 위의 return 상황 있잖아? 거기에다가 text.text ==""; 이거 넣으면 될 듯.
        RefreshRewardText(true);    
           
        idleBattleCoroutine = StartCoroutine(HandleIdleBattleCo());
    }

    //IdleBattle이 실행되지 않는 상태에선 텍스트를 ""로 전환하고, 실행되는 상태에서는 현황을 표시할 메서드
    //StartIdleBattle에서 return하는 부분에는 false 넣고, UINoticePopup 부분에는 true 넣으면 됨
    private void RefreshRewardText(bool toggle)
    {
        if(idleBattleText == null)
        {
            Debug.Log("[IdleBattleHandler] : 방치전투 현황을 출력할 텍스트 필드가 설정되지 않았습니다");
            return;
        }

        //TODO : 텍스트 필드 추가, 패널도 추가(켜고 끌 수 있게?), idleReward를 지역변수가 아니라 필드로 전환

        //idleBattlePanel.SetActive(toggle);

        switch (toggle)
        {
            case true:
                idleBattleText.text = $"매 {rewardTime}초마다 {idleReward} Gold 획득!";
                break;
            case false:
                idleBattleText.text = "";
                break;
        }

    }

    //파티슬롯이 전부 비어있는지 체크할 메서드
    private int CheckPartySlot()
    {
        int emptySlotCount = 0;
            
        //내부 참조가 필요하기 때문에 int[]형으로 받지 말고 이렇게 받는 게 맞긴 함.
        HeroInstance[] heroes = PartyManager.Instance.partySlots;
              
        for (int i = 0; i < heroes.Length; i++)
        {
            //왠진 모르겠는데, 이런 거 검사할 때는 영웅의 data도 null인지 같이 검사해야 함.
            //그 말은, 처음 시작할 땐 파티에 있는 인스턴스가 전부 null이 아니라는 건데?
            //처음에는 슬롯 내부의 인스턴스가 다 있다고 인식하고, 재시작 시에는 아니라는 건데 이게 대체 무슨 현상이지
            //애초에 이 메서드 자체가 반대로 설계되어있어서 알아보기 어렵네.
            if (heroes[i] == null || heroes[i].data == null)
            {
                Debug.Log("[IdleBattleHandler] : 방치전투 보상 지급을 위해 슬롯을 검사중입니다.");
                emptySlotCount++;
            }
        }

        return emptySlotCount;

    }

    //비어있는 파티슬롯 수만큼 빼서, 최종적으로는 현재 파티에 있는 인수가 몇 명인지 반환하는 메서드
    private int CheckPartySlot1()
    {
        int slotLength = PartyManager.Instance.partySlots.Length;

        //이러면 근데... 그렇잖아? slotLength 값이 줄어들잖아.
        for (int i = 0; i < slotLength; i++)
        {
            if(PartyManager.Instance.partySlots[i] == null)
            {
                slotLength--;
            }
        }

        return slotLength;
    }

    private void SpawnParty()
    {
        //현재 파티 슬롯의 배치 상태를 복사
        HeroInstance[] heroes = PartyManager.Instance.partySlots;

        for (int i = 0; i < heroes.Length; i++)
        {
            //formationData를 통해 월드 좌표로 변환
            Vector3 spawnPosition = formationData.GetWolrdPosition(TeamType.Ally, i, idleBattleOrigin.position);

            //데이터에 있는 프리팹만 생성하여, GameObject형 리스트에 추가

            if (heroes[i] != null && heroes[i].data != null)
            {
                GameObject copiedPrefab = Instantiate(heroes[i].data.HeroPrefab, spawnPosition, Quaternion.identity, unitContainer);
                
                //임시로, 0번과 1번만 Flip
                if(i == 0 || i == 1)
                {
                    //SpriteRenderer prefabRenderer;

                    //if(copiedPrefab.TryGetComponent<SpriteRenderer>(out prefabRenderer))
                    //{
                    //    prefabRenderer.flipX = true;
                    //}                

                    //각각 다른 파츠들이 조합되어 만들어지는 에셋이므로 이런 식으로 flip하여 사용할 수 없다.
                    copiedPrefab.transform.localScale = new Vector3(-1f, 1f, 1f);

                }

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

    private IEnumerator HandleIdleBattleCo()
    {
        while (true)
        {
            //GetComponentInChildren을 남발하여 성능저하를 일으킬 가능성이 높은데 이거 어떻게 할지.
            for (int i = 0; i < copiedPrefabs.Count; i++)
            {
                //여기서 NullReferenceException이 우연히도 일어나지 않는데, 안정적이지 않은 방식으로 for문 돌리고 있음.
                SoundManager.Instance.PlaySFX(SoundKey.SFX_Attack_1);
                Animator anim = copiedPrefabs[i].GetComponentInChildren<Animator>();
                anim.SetTrigger("2_Attack");
            }

            yield return wait;
            //Debug.Log($"[IdleBattleHandler] : {rewardTime} 초 기다렸습니다");
            //AFKHero.Player.PlayerManager.Instance.AddGold(IdleReward);
            //Debug.Log($"[IdleBattleHandler] : {IdleReward} 지급함");

            GiveIdleBattleReward();
        }
    }

    private void GiveIdleBattleReward()
    {
        if(AFKHero.Player.PlayerManager.Instance == null)
        {
            Debug.LogWarning($"[IdleBattleHandler] : PlayerManager를 찾을 수 없습니다.");
            return;
        }    

        AFKHero.Player.PlayerManager.Instance.AddGold(idleReward);

        if (EquipmentManager.Instance == null)
        {
            Debug.LogWarning($"[IdleBattleHandler] : EquipmentManager를 찾을 수 없습니다.");
            return;
        }

        //설정한 확률을 통과했다면 장비지급 메서드를 호출합니다.
        if (UnityEngine.Random.Range(0f, 100f) <= equipmentRate)
        {
            Debug.Log($"<color=cyan>[IdleBattleHandler]</color> 방치 전투로 장비를 획득했습니다.");
            EquipmentManager.Instance.GiveRandomEquipment();
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


    //파티매니저의 이벤트를 구독하여 방치 전투 현황을 갱신할 메서드
    public void OnPartyChanaged()
    {
        //클리어 메서드를 실행하기 위해서, 강제로 StageState.Working을 넣습니다. 뭔가 아닌 것 같지만...
        ClearIdleBattle(StageState.Working);

        StartIdleBattle(StageManager.Instance.CurrentState);

    }

    #region 보상지급 연출(미구현)

    //UIQuestManager에서 뜯어왔는데, 이거 좀 더 좋게? 하려면
    //아예 UIRewardGiver 이런 클래스를 싱글톤으로 만들고
    //보상 주는 애들이 해당 클래스를 호출해서 연출 시작하게 하면 좋을 듯?
    //내가 만들 수 있으면 좋겟는데... 지금 이 시점에선 무리

    //private void PlayMainQuestRewardEffect(RewardType rewardType, Vector3 startPosition)
    //{
    //    GameObject rewardImage = null;
    //    RectTransform rewardTarget = null;

    //    switch (rewardType)
    //    {
    //        case RewardType.Gold:
    //            rewardImage = goldImage;
    //            rewardTarget = goldTarget;
    //            break;

    //        case RewardType.Dia:
    //            rewardImage = diaImage;
    //            rewardTarget = diaTarget;
    //            break;

    //        case RewardType.FreeTicket:
    //            rewardImage = freeTicketImage;
    //            rewardTarget = freeTicketTarget;
    //            break;
    //    }

    //    if (rewardImage == null || rewardTarget == null || rewardEffectRoot == null)
    //    {
    //        return;
    //    }

    //    PlayRewardEffect(rewardImage, rewardTarget, startPosition);
    //}

    ////보상 이미지가 퍼진 후 상단 재화 UI로 이동
    //private void PlayRewardEffect(GameObject rewardImage, RectTransform rewardTarget, Vector3 startPosition)
    //{
    //    int rewardCount = Random.Range(3, 6);

    //    Vector2 startLocalPosition = rewardEffectRoot.InverseTransformPoint(startPosition);
    //    Vector2 targetLocalPosition = rewardEffectRoot.InverseTransformPoint(rewardTarget.position);

    //    for (int i = 0; i < rewardCount; i++)
    //    {
    //        GameObject rewardEffect = Instantiate(rewardImage, rewardEffectRoot);

    //        rewardEffect.SetActive(true);

    //        RectTransform rewardRect = rewardEffect.GetComponent<RectTransform>();

    //        if (rewardRect == null)
    //        {
    //            Destroy(rewardEffect);
    //            continue;
    //        }

    //        rewardRect.anchoredPosition = startLocalPosition;
    //        rewardRect.localScale = Vector3.one;

    //        Vector2 randomDirection = Random.insideUnitCircle.normalized;

    //        float randomDistance = Random.Range(rewardSpreadDistance * 0.5f, rewardSpreadDistance);

    //        Vector2 spreadPosition = startLocalPosition + randomDirection * randomDistance;

    //        bool isLastReward = i == rewardCount - 1;

    //        Sequence sequence = DOTween.Sequence();

    //        sequence.Append(rewardRect.DOAnchorPos(spreadPosition, rewardSpreadDuration).SetEase(Ease.OutQuad));
    //        sequence.AppendInterval(0.05f + i * 0.03f);
    //        sequence.Append(rewardRect.DOAnchorPos(targetLocalPosition, rewardMoveDuration).SetEase(Ease.InQuad));
    //        sequence.Join(rewardRect.DOScale(0.3f, rewardMoveDuration).SetEase(Ease.InQuad));

    //        sequence.OnComplete(() =>
    //        {
    //            if (isLastReward)
    //            {
    //                rewardTarget.DOKill();

    //                rewardTarget.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f).SetUpdate(true);
    //            }

    //            Destroy(rewardEffect);
    //        });
    //    }
    //}
    #endregion

}
