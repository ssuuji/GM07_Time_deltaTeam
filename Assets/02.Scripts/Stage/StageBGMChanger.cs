using AFKHero.Sound;
using UnityEngine;

//이게... StageManager의 이벤트 기반 클래스가 되는 것보다...
//StageManager가 필드로 갖는 게 모냥새가 깔끔할 것 같다.
//만약 이벤트로 한다고 치잖아?
//그럼 또 다른 게임오브젝트가 필요해...
//아닌가? 그냥 스테이지 매니저에 추가해도 되나?

//일단 이벤트 기반으로 구현해서 테스트 ㄱ

public class StageBGMChanger : MonoBehaviour
{
    //private void Start()
    //{
    //    StageManager.Instance.StageStateChanged += ChangeStageBGM;
    //}

    //private void OnDestroy()
    //{
    //    //생각해보니까 이거 이벤트 구독 해제하는거 있잖아
    //    //이 클래스 객체가 파괴될 때 해제하는 거잖아?
    //    //씬전환이 없긴하지만, 있다면 해제되는데?
    //    //Start에서... 재구독이... 되나? Start는 1회 실행 아닌가?
    //    if(StageManager.Instance != null)
    //    {
    //        StageManager.Instance.StageStateChanged -= ChangeStageBGM;
    //    }
    //}


    //이벤트 기반으로 했는데 재생이 안 됨
    //1) 타이틀씬에서 게임씬 넘어갔을 때 재생 안됨
    //2) 1스테이지 브금은 재생하는데 나머진 재생안함
    //3) 보스는 또 제대로 재생함
    //스테이지 브금을 조건에 맞게 변경하는 메서드
    public void ChangeStageBGM(StageState state, int stageNumber)
    {
        if (state == StageState.None || state == StageState.Result) return;

        switch (state)
        {
            case StageState.Idle:
                SoundManager.Instance?.PlayBGM(SoundKey.BGM_Idle);
                Debug.Log("Idle상태의 배경음을 재생합니다");
                break;
            case StageState.Working:
                SelectStageBGM(stageNumber);
                break;
            //default: //사실... 위에서 예외처리 해놔서 default일 때가 없어.
            //    break;
        }

    }

    //StageState가 Working으로 전환됐다면, 거기에 맞춰서 브금 고름.
    private void SelectStageBGM(int stageNumber)
    {
        if(StageManager.Instance.CurrentStageInfo == null)
        {
            Debug.LogError("[StageBGMChanger] : currentStageInfo가 null입니다.");
            return;
        }

        bool isBossStage = StageManager.Instance.CurrentStageInfo.IsBossStage;
        //이런 식으로 Switch문 편집하는 식으로 하지 말라고 하셨는데... 어떻게 하더라.

    
        if(!isBossStage)
        {
            Debug.Log($"[StageBGMChanger] : {stageNumber}스테이지 BGM을 재생합니다");
            //스테이지 번호에 따라 다른 브금 재생. 각 case는 CurrentStageNumber임.
            switch (stageNumber)
            {
                case 1:
                    SoundManager.Instance?.PlayBGM(SoundKey.BGM_Stage);
                    Debug.Log($"[StageBGMChanger] : 1스테이지 BGM 재생에 성공했습니다.");
                    break;
                case 2:
                    SoundManager.Instance?.PlayBGM(SoundKey.BGM_Stage2);
                    Debug.Log($"[StageBGMChanger] : 2스테이지 BGM 재생에 성공했습니다.");
                    break;
                case 3:
                    SoundManager.Instance?.PlayBGM(SoundKey.BGM_Stage3);
                    Debug.Log($"[StageBGMChanger] : 3스테이지 BGM 재생에 성공했습니다.");
                    break;
                case 4:
                    SoundManager.Instance?.PlayBGM(SoundKey.BGM_Stage4);
                    break;
                case 5:
                    SoundManager.Instance?.PlayBGM(SoundKey.BGM_Stage5);
                    break;
                default:
                    SoundManager.Instance?.PlayBGM(SoundKey.BGM_Stage);
                    Debug.Log($"[StageBGMChanger] : 재생에 실패하여 기본 BGM을 재생합니다.");
                    break;
            }
        }
        else if (isBossStage)
        {
            SoundManager.Instance.PlayBGM(SoundKey.BGM_Boss);
            Debug.Log("보스 스테이지 BGM을 재생합니다");
        }
    }
}
