using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class TitleSceneController : MonoBehaviour
    {
        [SerializeField] private Image logo;          //로고 이미지
        [SerializeField] private TMP_Text startText;  //시작 메세지
        [SerializeField] private Image[] heroes;      //중앙 영웅들 이미지
        [SerializeField] private Image fadeImage;     //페이드 이미지

        private Sequence seq;          //타이틀연출용 시퀀스
        private bool isStart = false;  //시작버튼 클릭 확인 여부
        private bool canStart = false; //시작버튼 클릭 가능여부

        private void Start()
        {
            PlayTitle(); //타이틀 연출 시작
        }

        #region 타이틀 연출

        private void PlayTitle()
        {
            //영웅등장 -> 로고 등장 -> 시작문구표시 순서
            seq = DOTween.Sequence();
            seq.Append(fadeImage.DOFade(0f, 0.5f).SetEase(Ease.OutQuad)); //화면 페이드인
            seq.AppendInterval(0.1f);                                   //화면 밝아진 후에 영웅 등장

            //로고와 시작문구 숨겨두기
            logo.rectTransform.localScale = Vector3.zero;
            startText.alpha = 0;

            ////////////////////////
            //1. 영웅등장
            ////////////////////////
            for (int i = 0; i < heroes.Length; i++)
            {
                RectTransform hero = heroes[i].rectTransform; //현재 영웅의 위치
                Vector2 targetPos = hero.anchoredPosition;   //도착할 위치

                //맨 아래쪽부터 도착위치까지 올라오는 느낌으로
                hero.anchoredPosition = targetPos + Vector2.down * 300.0f; //시작위치를 맨 아래로 잡고
                hero.localScale = Vector3.zero;                            //크기 0 으로 설정

                seq.Append(hero.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutBack)); //아래쪽에서 도착위치까지 이동
                seq.Join(hero.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));     //이동하면서 크기도 원래 크기로

                //다음영웅 표시간격
                seq.AppendInterval(0.1f);
            }
            //영웅이 다 모였으니 로고 등장 전 잠깐의 간격주기
            seq.AppendInterval(0.2f);


            ////////////////////////
            //2. 로고등장
            ////////////////////////
            //잔상용로고
            Image logoGhost = Instantiate(logo, logo.transform.parent);             //원본로고 복제
            logoGhost.transform.SetSiblingIndex(logo.transform.GetSiblingIndex());　//잔상은 원본 로고 뒤쪽에 위치
            logoGhost.rectTransform.localScale = Vector3.one * 3f;                //원본로고 보다 조금 더 크게
            logoGhost.DOFade(0f, 0f);                                               //알파값 0

            //원본로고
            RectTransform logoRect = logo.rectTransform; //로고위치
            logoRect.localScale = Vector3.one * 2.5f;    //처음엔 완전크게
            logo.DOFade(0f, 0f);                         //알파값 0

            seq.Append(logoRect.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutCubic)); //로고가 원래 크기로 줄어들면서 등장
            seq.Join(logo.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));                   //작아지면서 색이 진해지게
            seq.Insert(seq.Duration() - 0.28f, logoGhost.DOFade(0.25f, 0.08f));      //잔상은 살짝 늦게 표시
            seq.Insert(seq.Duration() - 0.28f, logoGhost.rectTransform.DOScale(Vector3.one * 1.0f, 0.4f).SetEase(Ease.OutCubic)); //잔상도 크기가 작아지게
            seq.Insert(seq.Duration() - 0.12f, logoGhost.DOFade(0f, 0.2f));          //잔상은 도착후 다시 알파값 0

            seq.Append(logoRect.DOScale(Vector3.one * 1.5f, 0.1f).SetEase(Ease.OutQuad)); //원래 크기에서 쪼금 더 커진다음
            seq.Join(logo.DOFade(0.7f, 0.1f));                                             //알파값 살짝 낮추고
            seq.Append(logoRect.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));        //다시 원래 크기로 돌아오면서
            seq.Join(logo.DOFade(1f, 0.12f));                                              //원래 색상으로

            seq.AppendCallback(() => { Destroy(logoGhost.gameObject); }); //잔상 삭제

            //시작문구 등장 전 잠깐의 간격주기
            seq.AppendInterval(0.2f);


            ////////////////////////
            //3. 시작문구
            ////////////////////////
            seq.Append(startText.DOFade(0.8f, 0.3f)); //시작문구 천천히 등장
            //등장 후 뾰잉뾰잉한 느낌으로 반복
            seq.AppendCallback(() => 
            {
                canStart = true;
                startText.rectTransform.DOScale(1.1f, 0.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                PlayHeroFloating(); //영웅들도 둥실둥실 떠 있는 느낌으로
            });

        }

        private void PlayHeroFloating()
        {
            for (int i = 0; i < heroes.Length; i++)
            {
                RectTransform hero = heroes[i].rectTransform;

                //현재 영웅 위치 저장
                float originY = hero.anchoredPosition.y;

                //영웅마다 이동 높이를 살짝 다르게 설정
                float floatHeight = 8f + (i % 3) * 3f;

                //영웅마다 움직이는 속도를 다르게 해서
                //전부 똑같이 움직이는 느낌 방지
                float duration = 1.0f + i * 0.1f;

                hero.DOAnchorPosY(originY + floatHeight, duration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetDelay(i * 0.08f);
            }
        }

        #endregion

        public void OnClickedStart()
        {
            if (!canStart) return; //연출 끝난 후 클릭 할 수 있도록
            if (isStart) return;   //중복클릭 방지

            isStart = true;

            startText.rectTransform.DOKill(); //시작문구 반복 종료
            
            Sequence startSeq = DOTween.Sequence();
            startSeq.Append(startText.DOFade(0f, 0.15f));                       //시작문구 사라지고
            startSeq.Append(fadeImage.DOFade(1f, 0.5f).SetEase(Ease.InQuad));   //화면 페이드아웃

            //게임씬 이동
            startSeq.OnComplete(() => { SceneManager.LoadScene("UI"); });
        }

    }
}

