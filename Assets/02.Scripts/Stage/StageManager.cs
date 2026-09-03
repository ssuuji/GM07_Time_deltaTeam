using AFKHero.Battle;
using AFKHero.Quest;
using AFKHero.Sound;
using AFKHero.UI;
using System;
using System.Collections;
using UnityEngine;

//StageDB를 들고있으면서, 특정 조건에 따라서 현재 스테이지를 다음 스테이지로 넘기게 할 관리자 클래스
//현재는 BattleManager의 이벤트를 구독하는 방식으로 승패처리가 동작한단 말이지
//그런데 만약 추가 컨텐츠에서도 BattleManager와 BattleSpawner의 로직을 사용해야 한다면?
//그 컨텐츠에서 승패처리가 됐을 때도 이게 발동될텐데...
//이벤트를 사용하지 않고, 자체적으로 승패처리를 할 수 있는 방식으로 리팩토링되어야 하지 않을까?
//BattleManager와 BattleSpawner는 싱글톤이 아니니까, 다른 오브젝트의 컴포넌트를 참조하면 되려나?
//StageManager도... 사실은 딱 그냥 DB를 들고 있으면서 넘기는 것만 처리했어야 하지 않았을까.
//만약 그랬다면, 싱글톤이 아닌 일반 클래스로 만들고, 상속이나 이런 걸로 바꿀 수 있었을지도...
//근데 여기서 싱글톤이었던 애를 갈아엎으면 고쳐야 될 부분 겁나 많아지잖아.
//이런 부분을... 조교님이나 강사님, 멘토님한테 물어봐야 할 수도 있을 것 같네.
//애초에... UIStageManager 이런 클래스로 기능분리를 했어야 했을 듯.


//세이브 로드 할 때, StageInfo도 셋팅하는 식으로 해야 할 것 같다.
//만약에 세이브 데이터가 없다? 그러면 처음 시작하는 거자나?
//그러면 그 땐 1-1로 셋팅.

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("스테이지 데이터")]
    [SerializeField] private StageData stageData;

    [Header("적 생성")]
    //[SerializeField] private EnemySpawner enemySpawner; //이걸 EnemySpawner가 아니라 BattleManager로 바꾸고,
    //EnemySpawner의 기능을 BattleSpawner가 일부 흡수하는 방향으로 가야할 것 같음.
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private BattleSpawner battleSpawner;

    [Header("배경화면과 사운드 교체 담당")]
    [SerializeField] private StageBackgroundChanger stageBackgroundChanger;
    [SerializeField] private StageBGMChanger stageBGMChanger;

    [Header("결과 패널")]
    [SerializeField] private RectTransform victoryPanel;
    [SerializeField] private RectTransform defeatPanel;

    [Header("현재 진행 정보")]
    [SerializeField] private int currentStageNumber = 1;
    [SerializeField] private int currentSectionNumber = 1;
    [SerializeField] private StageInfo currentStageInfo; // 현재 진행 중인 구간의 데이터
    [SerializeField] private StageState currentState = StageState.None;

    [Header("적 레벨")]
    [SerializeField] private int enemyLevel = 1;

    //IdleBattleHandler가 구독해야 할 이벤트
    //매개변수를 뭘로 받게 해야 할지 고민.
    //매개변수로는 StageState만 넘긴다. 어차피 프로퍼티 다 뚫려있어서 값을 읽을 순 있음.
    public event Action<StageState> StageStateChanged;


    [Header("마지막으로 클리어한 스테이지")]
    [SerializeField] private int lastStageNumber;
    [SerializeField] private int lastSectionNumber;


    // 추가된 부분 : 장비
    [Header("스테이지 장비 보상 설정")]
    [Tooltip("장비가 드롭될 확률 (0 ~ 100%)")]
    [Range(0f, 100f)]
    public float equipmentDropChance = 30f; // 기본 30% 확률로 설정

    [Tooltip("드롭될 수 있는 장비 목록")]
    public System.Collections.Generic.List<EquipmentData> possibleEquipmentDrops;

    //승리 패널을 자동으로 닫히게 할 코루틴
    private Coroutine autoClosePanelCoroutine;



    //프로퍼티
    public StageInfo CurrentStageInfo => currentStageInfo;
    public int CurrentStageNumber => currentStageNumber;
    public int CurrentSectionNumber => currentSectionNumber;
    public int LastStageNumber => lastStageNumber;
    public int LastSectionNumber => lastSectionNumber;
    public StageState CurrentState => currentState;
    public StageData StageData => stageData;
    public int EnemyLevel => enemyLevel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        victoryPanel.gameObject.SetActive(false);
        defeatPanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        //임시로 이벤트 구독
        battleManager.StateChanged += HandleBattleResult;

        //켰을 때 유저가 "전투시작" 버튼을 눌러야만 처음 스테이지 진행이 된다고 한다면, 여기서의 호출을 삭제해야 할 것.
        //StartStage();

        if(stageBackgroundChanger != null)
        {
            stageBackgroundChanger.SetBackgroundOnStart(lastStageNumber);
        }

        ChangeState(StageState.Idle);
    }

    private void OnDestroy()
    {
        battleManager.StateChanged -= HandleBattleResult;
    }

    //이게 아마도 전투시작 버튼에 연결되어야 하고, Start에서 실행이 되어서는 안 될텐데?
    //그리고, 이게 "재도전" 버튼에도 연결될 수 있을 것 같은데?
    public void StartStage() // 스테이지 구간을 시작
    {
        //전투가 실행 중일 때 실행하지 않게 할 방법 : BattleManager의 현재 상태에서 전투가 실행되선 안 되는 상태를 알아야 함.       
        //BattleManager의 현 상태가 Victory, Defeat인 상태로 바뀐 뒤, 다시 preparing으로 전환되는지 확인해야 함.        
        //BattleManager의 상태에 종속되지 않고, StageManage가 Working 상태일 때 실행을 제한하는 방식도 괜찮지 않을까.
        if(battleManager.CurrentState == BattleState.Fighting || battleManager.CurrentState == BattleState.UltimateSequence)
        {
            Debug.Log("전투가 이미 실행중입니다.");
            return;
        }

        victoryPanel.gameObject.SetActive(false);
        defeatPanel.gameObject.SetActive(false);

        //현재 번호에 따라서 StageData에서 Info를 받아온다.
        //이게 어디선가에선 호출되어야 계속 currentStageInfo가 교체되는 건데, 그게 대체 어디냐고.
        currentStageInfo = stageData.GetStage(currentStageNumber, currentSectionNumber);

        if (currentStageInfo == null) // 스테이지 데이터가 없으면 진행 중단
        {
            Debug.Log("[StageManager] : StageInfo가 셋팅되지 않았거나 마지막 스테이지를 클리어했습니다.");
            battleManager.ClearRegisteredUnits();
            battleSpawner.ClearSpawnedUnits();
            ChangeState(StageState.Idle);

            return;
        }

        //배경화면 전환. 있을 때만 바꿈.
        if (stageBackgroundChanger != null)
        {
            stageBackgroundChanger.ChangeBackground(currentStageNumber);
        }

        //적의 레벨을 스테이지 기반으로 계산
        enemyLevel = StageCalculator.CalculateEnemyLevel(currentStageNumber, currentSectionNumber);

        // [수정한 부분: 적 목록(Enemies)과 방금 계산한 레벨(enemyLevel)을 같이 넘겨줍니다]
        //StageInfo의 리스트가 StageEnemyInfo가 아니라 UnitData로 변경되어 주석처리함. BattleSpawner 기반으로 변경하기
        //enemySpawner.SpawnEnemies(currentStageInfo.Enemies, enemyLevel);
        
        // 0814 수정 부분 - 파티와 스테이지 데이터를 BattleSpawner에 연결
        if(PartyManager.Instance == null)
        {
            Debug.LogError("[StageManager] PartyManager를 찾을 수 없습니다.", this);
            return; 
        }

        if(battleManager == null)
        {
            Debug.LogError("[StageManger] BattleSpawner가 연결되지 않았습니다.", this);
            return;
        }

        UIBattleManager.Instance?.UpdatePartyUI();

        //방치전투 소환 로직은 이걸 참고해서 구현하면 될 듯.
        bool battleStarted = battleSpawner.SpawnBattle(
            PartyManager.Instance.partySlots,
            currentStageInfo.Enemies,
            enemyLevel,
            currentStageInfo.TimeLimit);

        if (!battleStarted)
        {
            Debug.LogError("[StageManager] 현재 스테이지 전투 생성에 실패했습니다.", this);
            ChangeState(StageState.Idle);
            return;
        }

        ChangeState(StageState.Working);
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
        ChangeState(StageState.Result);

        //승리 보상 UI 갱신
        UIBattleManager.Instance?.UpdateRewardUI(currentStageInfo);

        //승리 패널 활성화
        victoryPanel.gameObject.SetActive(true);

        //승리했으니 현 스테이지의 보상을 매니저를 통해 지급 => PlayerManager를 활용하는 것으로 변경
        TryGiveReward();

        //마지막으로 클리어한 스테이지와 섹션의 값을 저장 => 방치 전투에서 활용.
        lastStageNumber = currentStageNumber;
        lastSectionNumber = currentSectionNumber;

        QuestManager.Instance?.OnStageClear(currentStageNumber, currentSectionNumber); //스테이지 클리어 퀘스트 진행

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

        //여기에서도 Getstage를 호출하여 미리 셋팅을 해놓는 게 좋으려나.
        currentStageInfo = stageData.GetStage(currentStageNumber, currentSectionNumber);


        //TODO : SaveManager를 호출하여 값 저장
        GameSaveManager.Instance.SaveGame();

        autoClosePanelCoroutine = StartCoroutine(AutoClosePanelCo());
    }

    private void HandleDefeat()
    {
        ChangeState(StageState.Result);
        defeatPanel.gameObject.SetActive(true);
        battleManager.ClearRegisteredUnits();
        battleSpawner.ClearSpawnedUnits();
        GameSaveManager.Instance.SaveGame();
    }

    //승리 시, 3초 후 패널이 자동으로 닫히게 할 메서드
    private IEnumerator AutoClosePanelCo()
    {
        //3초 대기.
        //필요시 사전에 캐싱하여 리소스 확보한다. 추가로, 패널이 닫히는 시간을 표시하고 싶다면 필드가 있어야 한다.
        yield return new WaitForSeconds(3.0f);

        autoClosePanelCoroutine = null;

        victoryPanel.gameObject.SetActive(false);

        StartStage();
    }

    //패배 시, '확인' 버튼을 눌러 패널을 닫게 할 메서드
    public void CloseDefeatPanel()
    {               
        defeatPanel.gameObject.SetActive(false);       

        Debug.Log("패배 패널 닫음");
        stageBackgroundChanger.RevertStageBackground(currentStageNumber, currentSectionNumber);
        ChangeState(StageState.Idle);

        // 0814 수정
        // 다시 유닛 재 생성 후 전투
        //StartStage() ;
    }

    //"다음 스테이지" 버튼에 연결하여 사용하게 될 메서드
    public void NextStage()
    {
        if (autoClosePanelCoroutine != null) //3초 대기 후 닫기 코루틴이 실행된 상태라면
        {
            StopCoroutine(autoClosePanelCoroutine); //해당 코루틴을 종료시키고
            autoClosePanelCoroutine = null; //값을 비운다.
        }

        victoryPanel.gameObject.SetActive(false); //버튼이 눌렸을 때 패널이 닫히게 된다.
        StartStage();
    }

    //전투 승리 시, 다음 스테이지로 진행하지 않고 전투를 종료할 메서드
    public void StopStageProgress()
    {
        if(autoClosePanelCoroutine != null)
        {
            StopCoroutine(autoClosePanelCoroutine);
            autoClosePanelCoroutine = null;
        }

        victoryPanel.gameObject.SetActive(false);
        battleManager.ClearRegisteredUnits();
        battleSpawner.ClearSpawnedUnits();
        ChangeState(StageState.Idle);
    }

    //전투 진행 도중에 실행되면, 현재 전투 상태를 모두 초기화하고 StageManager를 Idle로 바꾸는 메서드
    public void EscapeStage()
    {
        if(currentState == StageState.Working)
        {
            battleManager.ClearRegisteredUnits();
            battleSpawner.ClearSpawnedUnits();
            stageBackgroundChanger.RevertStageBackground(currentStageNumber, currentSectionNumber);
            ChangeState(StageState.Idle);
        }
    }

    private void TryGiveReward()
    {
        if (AFKHero.Player.PlayerManager.Instance == null)
        {
            Debug.LogError("[StageManager] : PlayerManager의 Instance를 찾을 수 없습니다.");
            return;
        }
        AFKHero.Player.PlayerManager.Instance.AddGold(currentStageInfo.ClearGold);
        AFKHero.Player.PlayerManager.Instance.AddDia(currentStageInfo.ClearDia);
        AFKHero.Player.PlayerManager.Instance.AddFreeTicket(currentStageInfo.ClearTicket);

        // 드롭 확률 계산
        float randomValue = UnityEngine.Random.Range(0f, 100f);

        if (randomValue <= equipmentDropChance)
        {
            // 드롭할 장비 리스트가 비어있지 않은지 확인
            if (possibleEquipmentDrops != null && possibleEquipmentDrops.Count > 0)
            {
                // 리스트 안에서 랜덤으로 장비 하나 고르기
                int randomIndex = UnityEngine.Random.Range(0, possibleEquipmentDrops.Count);
                EquipmentData droppedItem = possibleEquipmentDrops[randomIndex];

                // 주사위를 굴려 랜덤 스탯의 진짜 장비를 생성
                EquipmentInstance newEquip = new EquipmentInstance(droppedItem);

                // 플레이어의 가방에 지급
                EquipmentManager.Instance.AddEquipment(newEquip);
                Debug.Log($"장비 드롭 성공! 획득한 장비: {droppedItem.equipmentName} (등급: {newEquip.Grade})");

                UIBattleManager.Instance.ShowDroppedEquipmentUI(droppedItem);
            }
        }
        else
        {
            // 장비가 떨어지지 않음
            Debug.Log($"아쉽게도 이번 스테이지에서는 장비가 드롭되지 않았습니다. (주사위: {randomValue:F1} / 목표: {equipmentDropChance})");
        }
    }

    private void ChangeState(StageState nextState)
    {
        if (currentState == nextState) return;

        currentState = nextState;


        if(stageBGMChanger != null )
        {
            stageBGMChanger.ChangeStageBGM(currentState, currentStageNumber);
        }
        else
        {
            Debug.LogWarning("[StageManager] : StageBGMChanger 컴포넌트가 없습니다.");
            stageBGMChanger = this.gameObject.AddComponent<StageBGMChanger>();
            stageBGMChanger.ChangeStageBGM(currentState, currentStageNumber);

        }
        //상태에 따른 BGM 변경
        //switch (currentState)
        //{
        //    case StageState.Idle:
        //        SoundManager.Instance?.PlayBGM(SoundKey.BGM_Idle);
        //        break;

        //    case StageState.Working:
        //        SoundManager.Instance?.PlayBGM(SoundKey.BGM_Stage);
        //        break;
        //}

        //이벤트 호출
        StageStateChanged?.Invoke(currentState);

        //현재 스테이지 UI 갱신
        UIBattleManager.Instance?.UpdateStageUI();
    }

    #region 세이브/ 로드

    public StageSaveData CreateStageSaveData()
    {
        StageSaveData saveData = new();
        saveData.currentStageNumber = currentStageNumber;
        saveData.currentSectionNumber = currentSectionNumber;
        saveData.lastStageNumber = lastStageNumber;
        saveData.lastSectionNumber = lastSectionNumber;

        return saveData;
    }

    public void LoadStageSaveData(StageSaveData saveData)
    {
        currentStageNumber = saveData.currentStageNumber;
        currentSectionNumber = saveData.currentSectionNumber;
        lastStageNumber = saveData.lastStageNumber;
        lastSectionNumber = saveData.lastSectionNumber;

        //그니까... 저장할 때 굳이 StageInfo까지 저장할 필요는 없잖아?
        //여기에서 로드할 때만 하면 되잖아.
        //그런데 중요한 것은, 최초 실행한 경우라면 로드할 데이터가 없을 텐데?
        //아닌가? 최초 실행할 때는 1 1 로 설정해뒀잖아.


        //3-1클리어까지 해보고, 이 부분에서 문제 생기는지 확인해보기.
        currentStageInfo = StageData.GetStage(currentStageNumber, currentSectionNumber);

        
    }

    #endregion
}
