using DG.Tweening;
using UnityEngine;

public class GuideArrow : MonoBehaviour
{
    [SerializeField] private RectTransform arrowRect;
    [SerializeField] private float offsetY = 70f;
    [SerializeField] private float moveDistance = 15f;
    [SerializeField] private float moveDuration = 0.5f;

    [SerializeField] private Canvas guideCanvas;

    private RectTransform target;
    private Tween moveTween;

    private void LateUpdate()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Hide();
            return;
        }

        transform.position = target.position;
    }

    public void Show(RectTransform target)
    {
        if (target == null)
        {
            Hide();
            return;
        }

        this.target = target;

        gameObject.SetActive(true);

        SetSorting(target);

        transform.position = target.position;
        arrowRect.anchoredPosition = new Vector2(0f, offsetY);

        PlayAnimation();
    }

    public void Hide()
    {
        target = null;
        moveTween?.Kill();
        gameObject.SetActive(false);
    }

    //가이드 타겟은 유지하고 화살표만 잠시 숨기기
    public void Pause()
    {
        moveTween?.Kill();
        gameObject.SetActive(false);
    }

    //숨겨둔 화살표 다시 표시
    public void Resume()
    {
        if (target == null || !target.gameObject.activeInHierarchy) return;

        gameObject.SetActive(true);
        SetSorting(target);

        transform.position = target.position;
        arrowRect.anchoredPosition = new Vector2(0f, offsetY);

        PlayAnimation();
    }

    //가이드 대상보다 한 단계 위에 표시
    private void SetSorting(RectTransform target)
    {
        Canvas targetCanvas = target.GetComponentInParent<Canvas>();

        if (targetCanvas == null) return;

        guideCanvas.overrideSorting = true;
        guideCanvas.sortingLayerID = targetCanvas.sortingLayerID;
        guideCanvas.sortingOrder = targetCanvas.sortingOrder + 1;
    }

    private void PlayAnimation()
    {
        moveTween?.Kill();

        arrowRect.anchoredPosition = new Vector2(0f, offsetY);
        moveTween = arrowRect.DOAnchorPosY(offsetY + moveDistance, moveDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
}