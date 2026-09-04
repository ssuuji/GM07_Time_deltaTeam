using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using AFKHero.Sound;

//마우스를 올리고 때는 것에 따라 / 모바일 환경에선 터치를 누르고 때는 것에 따라
//사운드 재생, DOTween 등 각종 효과를 부여할 클래스


//IpointerEnterHandler, IPointerExitHandler는 PC에서 주로 사용하며, 모바일 환경에선 마우스가 없기에 제대로 동작하지 않음
//따라서 IpointerDownHandler, IpointerUpHandler로 전환 시도.

public class UIHoverEffector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 originalScale;
    [Header("크기 설정")]
    [SerializeField]private float hoverScale = 1.09f;

    [Header("재생할 효과음")]
    [SerializeField] private SoundKey soundkey = SoundKey.UI_ButtonSelect;

    private Tween zoomInTween;
    private Tween zoomOutTween;

    private void Awake()
    {
        //원본크기 저장
        originalScale = transform.localScale;
        
        //Tween행동을 저장
        zoomInTween = transform
            .DOScale(hoverScale, 0.15f)
            .SetEase(Ease.OutQuad)
            .Pause()
            .SetAutoKill(false);

        zoomOutTween = transform
            .DOScale(originalScale, 0.15f)
            .SetEase(Ease.OutQuad)
            .Pause()
            .SetAutoKill(false);
    }


    /*
    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySFX(soundkey);
        zoomOutTween.Pause();
        zoomInTween.Restart();        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        zoomInTween.Pause();
        zoomOutTween.Restart();
    }
    */
    //버튼을 최초 클릭할 때
    public void OnPointerDown(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySFX(soundkey);
        zoomOutTween.Pause();
        zoomInTween.Restart();
    }

    //버튼에서 손을 땔 때
    public void OnPointerUp(PointerEventData eventData)
    {
        zoomInTween.Pause();
        zoomOutTween.Restart();
    }

    private void OnDestroy()
    {
        zoomInTween?.Kill();
        zoomOutTween?.Kill();
    }
}
