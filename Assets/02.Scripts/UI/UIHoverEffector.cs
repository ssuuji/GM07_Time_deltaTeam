using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using AFKHero.Sound;

//마우스를 올리고 때는 것에 따라 사운드 재생, DOTween 등 각종 효과를 부여할 클래스
//필요한 버튼에 추가하시면 좋을 것 같습니다.

public class UIHoverEffector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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


    //Enter와 Exit는 위에서 저장해둔 Tween을 각각 반대로 Pause, Restart합니다
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

    private void OnDestroy()
    {
        zoomInTween?.Kill();
        zoomOutTween?.Kill();
    }
}
