using AFKHero.Battle;
using AFKHero.UI;
using System.Collections;
using UnityEngine;

//StageDB를 들고있으면서, 특정 조건에 따라서 현재 스테이지를 다음 스테이지로 넘기게 할 관리자 클래스

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("스테이지 데이터")]
    [SerializeField] private StageData stageData;

    [Header("적 생성")]
    [SerializeField] private EnemySpawner enemySpawner; //이걸 EnemySpawner가 아니라 BattleManager로 바꾸고,
    //EnemySpawner의 기능을 BattleSpawner가 일부 흡수하는 방향으로 가야할 것 같음.

    [SerializeField] private BattleManager battleManager;

    [SerializeField] private BattleSpawner battleSpawner;

    [Header("결과 패널")]
    [SerializeField] private RectTransform victoryPanel;
    [SerializeField] private RectTransform defeatPanel;

    [Header("현재 진행 정보")]
    [SerializeField] private int currentStageNumber = 1;
    [SerializeField] private int currentSectionNumber = 1;

    [Header("마지막으로 클리어한 스테이지")]
    [SerializeField] private int lastStageNumber;
    [SerializeField] private int lastSectionNumber;

    //승리 패널을 자동으로 닫히게 할 코루틴
    private Coroutine autoClosePanelCoroutine;
    private StageInfo currentStageInfo; // 현재 진행 중인 구간의 데이터


    public StageInfo CurrentStageInfo => currentStageInfo;
    public int CurrentStageNumber => currentStageNumber;
    public int CurrentSectionNumber => currentSectionNumber;
    public int LastStageNumber => lastStageNumber;
    public int LastSectionNumber => lastSectionNumber;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);


        victoryPanel.gameObject.SetActive(false);
        defeatPanel.gameObject.SetActive(false);
    }
    private void Start()
    {
        //임시로 이벤트 구독
        battleManager.StateChanged += HandleBattleResult;

        //켰을 때 유저가 "전투시작" 버튼을 눌러야만 처음 스테이지 진행이 된다고 한다면, 여기서의 호출을 삭제해야 할 것.
        StartStage();

    }

    private void OnDestroy()
    {
        battleManager.StateChanged -= HandleBattleResult;
    }


    //이게 아마도 전투시작 버튼에 연결되어야 하고, Start에서 실행이 되어서는 안 될텐데?
    //그리고, 이게 "재도전" 버튼에도 연결될 수 있을 것 같은데?
    public void StartStage() // 스테이지 구간을 시작
    {
        victoryPanel.gameObject.SetActive(false);
        defeatPanel.gameObject.SetActive(false);


        //현재 번호에 따라서 StageData에서 Info를 받아온다.
        currentStageInfo = stageData.GetStage(currentStageNumber, currentSectionNumber);

        if(currentStageInfo == null) // 스테이지 데이터가 없으면 진행 중단
        {
            print("스테이지 데이터 null");
            return;
        }

        //현재 스테이지 UI 갱신
        UIBattleManager.Instance?.UpdateStageUI();

        //적의 레벨을 스테이지 기반으로 계산
        int enemyLevel = EnemyLevelCalculator.CalculateEnemyLevel(currentStageNumber, currentSectionNumber);

        // [수정한 부분: 적 목록(Enemies)과 방금 계산한 레벨(enemyLevel)을 같이 넘겨줍니다]
        //StageInfo의 리스트가 StageEnemyInfo가 아니라 UnitData로 변경되어 주석처리함. BattleSpawner 기반으로 변경하기
        //enemySpawner.SpawnEnemies(currentStageInfo.Enemies, enemyLevel);       
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
        if (currentStageInfo == null)
        {
            Debug.LogError("현재 스테이지의 정보가 null입니다.");
            return;
        }

        //승리 패널부터 우선 활성화
        victoryPanel.gameObject.SetActive(true);


        if(AFKHero.Player.PlayerManager.Instance == null)
        {
            Debug.LogError("[StageManager] : PlayerManager의 Instance를 찾을 수 없습니다.");
            return;
        }


        //승리했으니 현 스테이지의 보상에 해당하는 골드를 매니저를 통해 지급 => PlayerManager를 활용하는 것으로 변경
        //골드 이외의 다른 보상이 있더라도 새 클래스를 늘리기보단
        //ClearDia, ClearTicket 필드를 StageInfo에 만드는 것을 고려한다.
        //지급만 총괄하는 메서드를 구현해서 여기서 호출해도 될 듯.
        AFKHero.Player.PlayerManager.Instance.AddGold(currentStageInfo.ClearGold);

        //마지막으로 클리어한 스테이지와 섹션의 값을 저장 => 방치 전투에서 활용.
        lastStageNumber = currentStageNumber;
        lastSectionNumber = currentSectionNumber;

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

        //TODO : SaveManager를 호출하여 값 저장
        TempSaveManager.Instance.SaveStage();

        autoClosePanelCoroutine = StartCoroutine(AutoClosePanelCo());
    }

    private void HandleDefeat()
    {
        defeatPanel.gameObject.SetActive(true);
        //
        TempSaveManager.Instance.SaveStage();
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
    }
}
