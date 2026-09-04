using DG.Tweening;
using UnityEngine;

//스테이지가 시작될 때 DOTween을 활용하여 시작되었을을 유저에게 안내하는 클래스

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



public class StageStartNotifier : MonoBehaviour
{
    //이 녀석이 필드를 추가하지말고,
    //움직일 패널에 붙이면 되잖아.



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
        originalPosition = rect.anchoredPosition;

        rect.anchoredPosition = originalPosition;

        gameObject.SetActive(false);

    }

    private void InitSequence()
    {
        seq = DOTween.Sequence().Pause().Append(rect.DOAnchorPosX(0, 0.4f).SetEase(Ease.OutQuad))
            .Append(rect.DOPunchPosition(new Vector2(0, 1), 0.5f, 15, 0.7f))
            .Append(rect.DOAnchorPosX(-originalPosition.x, 0.25f).SetEase(Ease.OutQuad));

        seq.SetAutoKill(false);

        
        
        seq.OnComplete(()=>
            {
                isPlaying = false;
                gameObject.SetActive(false);
            });



    }

    //중복실행 방지 로직 정도 추가하면 될 것 같은데.

    /*
    public void NotifyStart()
    {
        if (isPlaying) return;

        isPlaying = true;
        rect.anchoredPosition = originalPosition;
        gameObject.SetActive(true);
        
        if(seq != null )
        {
            seq.Restart();
        }



    }
    */

    public void NotifyStart()
    {
        // 이미 연출 중이면 실행하지 않음
        if (isPlaying)
            return;

        isPlaying = true;

        // 시작 위치로 복귀
        rect.anchoredPosition = originalPosition;

        // 패널 활성화
        gameObject.SetActive(true);

        // 기존 Sequence가 있다면 제거
        if (seq != null)
        {
            seq.Kill();
        }

        // Sequence 생성
        seq = DOTween.Sequence();

        seq.Append(
            rect.DOAnchorPosX(0, 0.4f)
                .SetEase(Ease.OutQuad)
        );

        seq.Append(
            rect.DOPunchAnchorPos(
                new Vector2(0, 50),
                0.5f,
                15,
                0.7f
            )
        );

        seq.Append(
            rect.DOAnchorPosX(-originalPosition.x, 0.25f)
                .SetEase(Ease.OutQuad)
        );

        seq.OnComplete(() =>
        {
            isPlaying = false;

            gameObject.SetActive(false);
        });

        // Sequence 재생
        seq.Play();
    }

}
