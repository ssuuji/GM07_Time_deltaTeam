using AFKHero.Quest;
using AFKHero.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;


//"전투" 버튼을 누르면, "전투 시작" 버튼이 있는 패널 하나를 띄우고
//그 패널 안에는 진행할 스테이지의 적들이 표시되게 할 클래스

//그니까... 이 프리뷰어 패널이 뜬 다음에, 거기 안의 "전투 시작" 버튼을 누르잖아?
//그러면, 패널이 닫혀야 한다고.

//StageBackgroundChanager처럼, 아예 StageManager의 필드로 추가해서 호출해야 하는 걸려나...
//아니면... StageManager가 어차피 싱글톤이고 StartStage가 public 메서드잖아.
//패널을 닫음과 동시에, StageManager.Instance.StartStage 해버리면 되는 거 아니냐고.
//이게 대체 뭔 구조냐.

public class StagePreviewer : MonoBehaviour
{
    ////"프리뷰 패널"과 "소환할 위치"만 설정해주시면 됩니다.
    //[Header("프리뷰 패널")]
    //[SerializeField] private RectTransform stagePreviewPanel;         // -> 버튼연결부분 -> UIManager로 옮겼습니다
    //private bool isPanelOpened = false;


    //소환할 위치. 프리뷰 패널 프리팹에 5개 있습니다.
    [Header("소환할 위치")]
    [SerializeField] private Transform[] enemyPrefabs;

    [Header("적 정보 텍스트")]
    [SerializeField] private TMP_Text[] enemyInfoTexts;

    [Header("스테이지 정보")]
    [SerializeField] private TMP_Text stageInfo;
    [SerializeField] private GameObject goldRewardObject;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private GameObject diaRewardObject;
    [SerializeField] private TMP_Text diaText;
    [SerializeField] private GameObject freeTicketRewardObject;
    [SerializeField] private TMP_Text freeTicketText;

    [Header("프리팹 복사본 리스트")]
    [SerializeField] private List<GameObject> copiedPrefabs;

    [Header("가이드")]
    [SerializeField] private RectTransform stageStartButton;

    /*                                                                 //버튼연결부분 -> UIManager로 옮겼습니다
    private void Awake()                                               //OnEnable , OnDisable : 전투프리뷰 패널이 열리고 닫힐때 실행되도록 추가했습니다
    {
        TogglePreviewPanel(isPanelOpened);
    }

    //버튼에 연결하여 패널을 띄우거나 끄는 메서드
    public void OnClickedButton()
    {
        if(StageManager.Instance.CurrentState != StageState.Idle)
        {
            Debug.Log("[StagePreviewer] : 현재 전투가 진행중이거나, 결과 패널이 떠 있는 상태입니다.");
            return;
        }

        //누를 때마다 bool값 반대로 변경
        isPanelOpened = !isPanelOpened;

        TogglePreviewPanel(isPanelOpened);
    }

    //매개변수에 따라 패널을 켜거나 끄면서, 필요한 기능(적 프리팹 출력 / 삭제)를 실행하는 메서드.
    private void TogglePreviewPanel(bool toggle)
    {
        if (stagePreviewPanel == null)
        {
            Debug.Log("[StagePreviewer] : 패널이 등록되지 않았습니다.");
        }
        stagePreviewPanel.gameObject.SetActive(toggle);

        //사실 if-else 써도 되긴 함. 아닌가? 오히려 if-else를 써야만 하나? 매개변수가 null이면 어떡함?
        //bool은 null을 허용 안 하니 괜찮나.
        switch(toggle)
        {
            case true:
                ShowEnemyPrefab();
                break;
            case false:
                ClearEnemyPrefab();
                break;

        }

    }
    */

    //전투 프리뷰 패널이 열렸을 때
    private void OnEnable()
    {
        ClearEnemyPrefab(); //활성화될 때도 한 번 비우고 하게끔
        ShowEnemyPrefab();
        ShowGuide();
    }

    //전투 프리뷰 패널이 닫혔을 때
    private void OnDisable()
    {
        ClearEnemyPrefab();
    }



    private void ShowEnemyPrefab()
    {
        //아무리 생각해도 받아오는 방식이 너무 에바 아님? 되긴 되는데 겁나 길잖아.
        //어떻게 해야 짧게 쓸 수 있는 걸까.
        //StageInfo currentstage = StageManager.Instance.StageData.GetStage(StageManager.Instance.CurrentStageNumber, StageManager.Instance.CurrentSectionNumber);

        //아니... 이걸 받아오지 말고, StageManager.Instance.CurrentStageInfo 이렇게 받아올 수 있는 구조만 짜면 되는데
        //문제는, 이놈의 currentStage를 설정하는 부분이 StartStage 안에만 있다고.
        //그래서 최초 실행할 때는 이게 null이라고

        //StageManager가 세이브파일을 로드해올 때 currentStageInfo도 셋팅하게 하여 우선은 동작하긴 합니다.
        //다만, SaveManager가 동작하지 않는 개인 씬에서는 로드 메서드가 동작하지 않는 문제가 있습니다.

        StageInfo currentStage = StageManager.Instance.CurrentStageInfo;

        if (currentStage == null)
        {
            Debug.Log("[StagePreviewer] : 표시할 스테이지 정보가 null입니다.");
            ClearStageInfo();
            UINoticePopup.Instance.ShowTime("모든 스테이지를 클리어하였습니다. \n 다음 업데이트를 기대해주세요!");
            return;
        }

        //UI - 스테이지 정보 표시
        ShowStageInfo(currentStage);

        //StageInfo에 들어있는 적 리스트를 복사해온다.
        List<HeroData> enemies = currentStage.Enemies;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null) continue;

            //인스펙터 상에 등록해놨던 배열 내부의 게임오브젝트의 자식으로, 그 위치에다 생성합니다. 
            GameObject enemy = Instantiate(enemies[i].HeroPrefab, enemyPrefabs[i].position, Quaternion.identity, enemyPrefabs[i]);

            //레벨 표시도 이곳에서 처리하게 할 수 있을지도.
            enemy.transform.localScale = Vector3.one * 150f;

            //UI에 가려지는 걸 방지하기 위해 SortingGroup 컴포넌트를 가져와 값 변경
            SortingGroup sortingGroup = enemy.GetComponentInChildren<SortingGroup>();

            if (sortingGroup != null)
            {
                //레이어 이름과 정렬 순서를 정함
                sortingGroup.sortingLayerName = "UI";
                sortingGroup.sortingOrder = 10;
            }

            //소환한 프리팹은 리스트로 등록해둡니다.
            copiedPrefabs.Add(enemy);


            //11레벨짜리 적을 StageInfo에 셋팅하는 게 아니라, 소환 시점에서 레벨을 결정하니까...
            //우선 이대로도 동작은 할 것이므로... 나중에 개선하는 걸로...
            int enemyLevel = StageCalculator.CalculateEnemyLevel(StageManager.Instance.CurrentStageNumber, StageManager.Instance.CurrentSectionNumber);

            if (enemies[i].HeroGrade == HeroGrade.Epic)
            {
                enemyInfoTexts[i].text = $"<color=red>Lv. {enemyLevel} {enemies[i].HeroName}</color>";
            }
            else
            {
                enemyInfoTexts[i].text = $"Lv. {enemyLevel} {enemies[i].HeroName}";
            }
        }        
    }

    //패널 안에 생성됐던 프리팹 지울 메서드.
    private void ClearEnemyPrefab()
    {
        //리스트에 들어있는 수만큼 삭제한 다음
        for (int i = 0; i < copiedPrefabs.Count; i++)
        {
            Destroy(copiedPrefabs[i]);
        }

        //레벨과 이름 표시했던 것도 지움
        for (int i = 0; i < enemyInfoTexts.Length; i++)
        {
            enemyInfoTexts[i].text = "";
        }

        //리스트를 초기화
        copiedPrefabs.Clear();
    }

    //패널의 "전투 시작" 버튼을 눌렀을 때 실행할 메서드
    public void OnClickedStartStageButton()
    {
        //값을 false로 바꿔 프리뷰 패널을 닫습니다.         //버튼연결부분 -> UIManager로 옮겼습니다
        //isPanelOpened = false;
        //TogglePreviewPanel(isPanelOpened);

        UIManager.Instance.CloseView();     //전투 프리뷰 패널 닫기

        //StageManager의 StartStage를 호출합니다.
        StageManager.Instance.StartStage();

        //가이드 종료
        if (StageManager.Instance.CurrentState == StageState.Working && GuideManager.Instance != null && GuideManager.Instance.IsTarget(GuideTarget.Battle) && GuideManager.Instance.IsStep(GuideStep.ClickStageStart))
        {
            GuideManager.Instance.EndGuide();
        }
    }

    //스테이지 정보 표시 UI
    //ShowEnemyPrefab 부분에서 return 걸려서 실행 안 됨.
    private void ShowStageInfo(StageInfo currentStage)
    {
        //표시하기 전에 한 번 클리어한 상태로 사용
        ClearStageInfo();

        stageInfo.text = $"STAGE {StageManager.Instance.CurrentStageNumber}-{StageManager.Instance.CurrentSectionNumber}";
               

        //골드
        bool hasGold = currentStage.ClearGold > 0;
        goldRewardObject.SetActive(hasGold);

        if (hasGold)
        {
            goldText.text = currentStage.ClearGold.ToString();
        }

        //다이아
        bool hasDia = currentStage.ClearDia > 0;
        diaRewardObject.SetActive(hasDia);

        if (hasDia)
        {
            diaText.text = currentStage.ClearDia.ToString();
        }

        //무료 티켓
        bool hasFreeTicket = currentStage.ClearTicket > 0;
        freeTicketRewardObject.SetActive(hasFreeTicket);

        if (hasFreeTicket)
        {
            freeTicketText.text = currentStage.ClearTicket.ToString();
        }
    }

    //오브젝트들 전부 끄고 텍스트도 초기화해버리는 메서드
    private void ClearStageInfo()
    {
        stageInfo.text = "";
        goldRewardObject.SetActive(false);
        goldText.text = "";
        diaRewardObject.SetActive(false);
        diaText.text = "";
        freeTicketRewardObject.SetActive(false);
        freeTicketText.text = "";
    }

    //스테이지 시작 가이드 표시
    private void ShowGuide()
    {
        if (GuideManager.Instance == null) return;
        if (!GuideManager.Instance.IsTarget(GuideTarget.Battle)) return;
        if (!GuideManager.Instance.IsStep(GuideStep.ClickStageStart)) return;
        if (stageStartButton == null) return;

        GuideManager.Instance.ShowGuide(stageStartButton);
    }
}
