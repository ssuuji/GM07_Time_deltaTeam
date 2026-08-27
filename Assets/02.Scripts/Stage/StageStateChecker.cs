using TMPro;
using UnityEngine;

//StageManager의 상태를 체크하여, 텍스트로 띄우는 디버그용 클래스
//최종 병합 시에는 제거할 것.

public class StageStateChecker : MonoBehaviour
{
    [Header("스테이지 상태")]
    [SerializeField]private TMP_Text stageStateText;


    void Start()
    {
        StageManager.Instance.StageStateChanged += ShowStageState;
    }

    private void OnDestroy()
    {
        StageManager.Instance.StageStateChanged -= ShowStageState;
    }

    private void ShowStageState(StageState state)
    {
        if (stageStateText == null) return;

        Debug.Log("StageStateChecker 작동함");

        stageStateText.text = $"StageManager : {state}";
    }


}
