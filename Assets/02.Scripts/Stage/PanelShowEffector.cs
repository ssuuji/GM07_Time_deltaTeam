using AFKHero.Sound;
using DG.Tweening;
using UnityEngine;

//패널에 붙이면, 활성화될 때 DOTween효과를 재생하고 사운드를 출력하게 하는 스크립트
//UIHoverEffector : 클릭했을 때 DOTween효과 + 사운드
//PanelShowEffector : 패널이 활성화됐을 때 DOTween효과 + 사운드

//원래는 StageStartNotifier를 확장시켜서 승리패널과 패배패널 연출을 담당하는 클래스로 만들고 싶었는데
//그러려면 StageManager에서 패널 제어하는 부분을 리팩토링해야 하는 이슈가 있었기에 오래 걸릴 것 같았습니다.
//이것도 기능 자체는 동일하긴 합니다.

/*
[사용 시 주의점]
1) 이 스크립트가 붙을 게임 오브젝트는 실행 전의 Inspector에선 비활성화된 상태여야만 합니다.
2) 이 스크립트 자체에선 SetActive(false)하지 않으므로, 다른 스크립트에서 SetActive로 활성화/ 비활성화가 제어되어야 합니다.
3) Awake에서는 꺼두지 않고, 오로지 OnEnable, OnDisable에서만 동작합니다.


*/


[RequireComponent(typeof(RectTransform))]
public class PanelShowEffector : MonoBehaviour
{
    private RectTransform rect;
    private Vector2 originalPosition;
    private Vector3 originalScale;

    private Sequence seq;

    [Header("적용할 연출")]
    [SerializeField] private Ease ease = Ease.OutBack;
    [SerializeField] private Vector3 endValue = new Vector3(1.1f, 1.1f, 1.1f);
    [SerializeField] private float duration = 0.2f;

    [Header("재생할 사운드")]
    [SerializeField] private SoundKey soundKey = SoundKey.UI_CardDraw;

    private void Awake()
    {
        rect = this.GetComponent<RectTransform>();

        //원본 크기와 원본 위치를 저장해둠
        originalPosition = rect.anchoredPosition;

        originalScale = rect.localScale;

        //시퀀스를 초기화하여 캐싱
        InitSequence();

        //확실히 돌려놓을 수 있도록 저장했던 값으로 변경
        rect.localScale = originalScale;

        rect.anchoredPosition = originalPosition;
    }

    private void InitSequence()
    {
        seq = DOTween.Sequence().Pause();

        seq.Append(rect.DOShakeAnchorPos(duration, 100f, 10, 90));

        seq.Join(rect.DOScale(endValue, duration).SetEase(ease));

        seq.SetAutoKill(false);
    }

    //활성화 시 효과음을 재생하고 시퀀를 실행함.
    private void OnEnable()
    {
        SoundManager.Instance.PlaySFX(soundKey);

        //Restart : 해당 시퀀스를 처음부터 다시 재생하라. 그렇기에 OnDisable에서 Pause해도 괜찮은 것. 대신, 거기서 원본 크기와 위치를 돌려두긴 해야함.
        if(seq !=null)
        {
            seq.Restart();
        }
    }

    //비활성화 시 시퀀스를 멈추고, 
    private void OnDisable()
    {
        if (seq != null)
        {
            seq.Pause();
        }     

        rect.localScale = originalScale;
        rect.anchoredPosition = originalPosition;
    }

}
