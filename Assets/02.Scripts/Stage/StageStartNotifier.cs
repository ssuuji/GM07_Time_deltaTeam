using DG.Tweening;
using UnityEngine;

//스테이지가 시작될 때 DOTween을 활용하여 시작되었을을 유저에게 안내하는 클래스

//TODO
//이 클래스는 StageNotifier로 확장할 여지가 있다.
//승리 시/ 패배 시 패널도 유사하게 DoTween효과를 넣어서
//화면 밖에서 튀어나온다던가 하게 할 수 있을 것.
//문제는... 지금 그걸 하려면
//StageManager를 꽤 많이 손을 봐야 함.
//내부에 코루틴 제어도 있어서.
//NotifyStart랑은 좀 다르게, 승리 패널과 패배 패널은

//아... 아닌가?

//그냥 똑같은 패널 참조만 하고 연출만 제공해도 현 시점에선 돌아가지 않나?
//그렇게 할라면야 할 수 있는데
//모냥새가 안 좋잖아. 할 거면 제대로 해야지.
//일단... 해야되는 거 최대한 하고
//발표까지 마치고 개인적으로 연구하는 걸로


/*

방법 1 :

1) 오른쪽에서 왼쪽으로 이동하는 Tween을 만든다.
2) 원하는 위치에 도달했을 때(이건 순수 계산으로 해야 할 듯) Pause하고, 펀치 효과를 부여하고, 그게 끝나면 다시 Restart한다?


방법 2 :

1) 오른쪽에서 왼쪽으로 이동하는 Tween을 만든다.
2) 원하는 위치에 도달한 다음 펀치 효과를 부여하고, 중앙에서 왼쪽으로 마저 이동하는 Tween을 시작한다.

방법 3 :
1) 위 모든 과정을 sequence로 처리한다.

*/

//반드시 RectTransform이 있도록 보장함
[RequireComponent(typeof(RectTransform))]
public class StageStartNotifier : MonoBehaviour
{
    //이 녀석이 필드를 추가하지말고,
    //움직일 패널에 붙이면 되잖아.

    //아... 인스펙터에서 딸깍으로 제어하고 싶긴 한데 필드 좀 많아지네.
    //지금 시점에선 인스펙터 수정하나 프리팹 수정하나 거기서 거기일 듯.

    /*
    [Header]로 필요한 필드

    [Header("첫 이동 연출")
    [SerializeField] private float position1;
    [SerializeField] private float duration1;


    [Heade("두 번째 펀치 연출")
    [SerializeField] private Vector2 punchVector;
    [SerializeField] private float punchPower;
    [SerializeField] private float punchDuration;
    [SerializeField] private int vibrato;
    [SerializeField] private float elasticity;

    [Header("마지막 이동 연출")
    //좌표는 필요 없어
    [SerializeField] private float duration2;

    
    
    */


    private Vector2 originalPosition;
    private float moveDistance = 3.0f;

    private Tween moveTween1;
    private Tween punchTween;
    private Tween moveTween2;

    private RectTransform rect;

    private bool isPlaying = false;

    private Sequence seq;

    private void Awake()
    {
        rect = this.GetComponent<RectTransform>();

        //원본 위치를 저장한다
        originalPosition = rect.anchoredPosition;

        //시퀀스 초기화를 진행한다
        InitSequence();

        //초기화 완료 후 혹시 모르니 원래 위치로 돌린다
        rect.anchoredPosition = originalPosition;

        //해당 오브젝트를 확실히 꺼두기 위해 한 번 더 SetActive(false)를 호출한다.
        gameObject.SetActive(false);

    }

    //사용할 시퀀스를 초기화해두는 메서드

    private void InitSequence()
    {
        //시퀀스를 생성하고 Pause한 다음에, 하고 싶은 것들을 Append와 Join으로 연결한다.
        seq = DOTween.Sequence().Pause().Append(rect.DOAnchorPosX(0, 0.4f).SetEase(Ease.OutQuad))
            .Append(rect.DOPunchAnchorPos(new Vector2(1, 1) * 50, 0.5f, 15, 0.7f))
            .Append(rect.DOAnchorPosX(-originalPosition.x, 0.25f).SetEase(Ease.OutQuad));

        //중요 : Autokill을 끈다. 안 끄면 GC에 의해 날아감.
        seq.SetAutoKill(false);        
        

        //완료됐을 시에만 플레이 여부를 끄고, 해당 오브젝트도 비활성화해둔다.
        seq.OnComplete(()=>
            {
                isPlaying = false;
                gameObject.SetActive(false);
            });
    }

    //중복실행 방지 로직 정도 추가하면 될 것 같은데.

    
    public void NotifyStart()
    {
        if (isPlaying) return;

        //실행 시 실행됐다고 켠다.
        isPlaying = true;

        //해당 패널의 원래 위치로 돌린다.
        rect.anchoredPosition = originalPosition;

        //패널을 켠다.
        gameObject.SetActive(true);
        
        //Restart로 시퀀스를 재생한다.
        if(seq != null )
        {
            seq.Restart();
        }
    }    

    //public void NotifyStart()
    //{
    //    // 이미 연출 중이면 실행하지 않음
    //    if (isPlaying)
    //        return;

    //    isPlaying = true;

    //    // 시작 위치로 복귀
    //    rect.anchoredPosition = originalPosition;

    //    // 패널 활성화
    //    gameObject.SetActive(true);

    //    // 기존 Sequence가 있다면 제거
    //    if (seq != null)
    //    {
    //        seq.Kill();
    //    }

    //    // Sequence 생성
    //    seq = DOTween.Sequence();

    //    seq.Append(
    //        rect.DOAnchorPosX(0, 0.4f)
    //            .SetEase(Ease.OutQuad)
    //    );

    //    seq.Append(
    //        rect.DOPunchAnchorPos(
    //            new Vector2(0, 50),
    //            0.5f,
    //            15,
    //            0.7f
    //        )
    //    );

    //    seq.Append(
    //        rect.DOAnchorPosX(-originalPosition.x, 0.25f)
    //            .SetEase(Ease.OutQuad)
    //    );

    //    seq.OnComplete(() =>
    //    {
    //        isPlaying = false;

    //        gameObject.SetActive(false);
    //    });

    //    // Sequence 재생
    //    seq.Play();
    //}

}
