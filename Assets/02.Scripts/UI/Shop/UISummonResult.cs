using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    //소환된 영웅의 결과 카드를 보여주는 UI
    public class UISummonResult : MonoBehaviour
    {
        [Header("1회 소환")]
        [SerializeField] private GameObject summon1Panel;                //1회 소환 결과 영역
        [SerializeField] private UISummonCard summon1Card;               //1회 소환 카드

        [Header("10회 소환")]
        [SerializeField] private GameObject summon10Panel;               //10회 소환 결과 영역
        [SerializeField] private List<UISummonCard> summon10Cards;       //10회 소환 카드
        [SerializeField] private Button openAllButton;                   //일괄오픈 버튼

        [Header("소환 연출")]
        [SerializeField] private RectTransform summonPoint;              //카드가 시작하는 제단 위치
        [SerializeField] private RectTransform summonPointCenter;        //카드가 모이는 중앙 위치
        [SerializeField] private ParticleSystem summonRise;              //이펙트 : 제단 -> 중앙으로 올라오는 빛
        [SerializeField] private ParticleSystem summonFlash;             //이펙트 : 중앙에서 터지는 빛
        
        //연출 - 시간
        private float riseDuration = 1.5f;                               //제단 -> 중앙 이동시간
        private float waitDuration = 0.2f;                               //중앙 대기 시간
        private float spreadDuration = 0.12f;                            //중앙 -> 최종위치 이동시간
        private float cardInterval = 0.07f;                              //10장 배치 간격

        //연출 - 상태
        private bool isSummon = false;                                   //소환중인지 확인
        private Sequence oneSummonSequence;                              //1회 소환 연출
        private Coroutine tenSummonCoroutine;                            //10회 소환 연출 코루틴

        //이펙트 - 기존 스케일 저장
        private Vector3 riseOriginScale;                                 //Rise   
        private Vector3 flashOriginScale;                                //Flash  

        //카드 - 기존값 저장
        private Vector3 oneCardOriginScale;                              //1회카드
        private List<Vector2> cardOriginPositions = new List<Vector2>(); //10회카드 - 원래 위치
        private List<Vector3> cardOriginScales = new List<Vector3>();    //10회카드 - 원래 크기
        private bool isSaveCard = false;                                 //10회카드 - 위치 저장 여부

        [Header("뒤로가기 버튼")]
        [SerializeField] private Button backButton;                      //검정 반투명 배경
                                                                   
        

        private void Awake()
        {
            riseOriginScale = summonRise.transform.localScale;     //Rise  기존 스케일 저장
            flashOriginScale = summonFlash.transform.localScale;   //Flash 기존 스케일 저장
            oneCardOriginScale = summon1Card.transform.localScale; //1회소환 카드 기존 스케일 저장

            //1회 소환 카드
            summon1Card.OnOpened += CheckAllCardsOpened;

            //10회 소환 카드
            foreach (UISummonCard card in summon10Cards)
            {
                card.OnOpened += CheckAllCardsOpened;
            }
        }

        private void OnDestroy()
        {
            summon1Card.OnOpened -= CheckAllCardsOpened;

            foreach (UISummonCard card in summon10Cards)
            {
                card.OnOpened -= CheckAllCardsOpened;
            }
        }

        #region 소환결과

        //소환 결과
        public void ShowResult(List<HeroData> heroes)
        {
            backButton.gameObject.SetActive(true);
            backButton.interactable = false;

            //1회
            if (heroes.Count == 1)
            {
                ShowOneResult(heroes[0]);
            }
            //10회
            else if (heroes.Count == 10)
            {
                ShowTenResult(heroes);
            }
        }

        //1회 소환 결과
        private void ShowOneResult(HeroData hero)
        {
            summon1Panel.SetActive(true);   //1회 영역만 활성화
            summon10Panel.SetActive(false);

            backButton.interactable = true; //연출 스킵 가능

            summon1Card.SetHeroData(hero);  //카드에 영웅 데이터 적용
            PlayOneSummonAnimation();       //카드소환
        }

        //10회 소환 결과
        private void ShowTenResult(List<HeroData> heroes)
        {
            summon1Panel.SetActive(false);
            summon10Panel.SetActive(true);                                     //10회 영역만 활성화
                                                                               
            backButton.interactable = true;                                    //연출 스킵이 가능하도록 배경 클릭
            openAllButton.gameObject.SetActive(false);                         //일괄오픈 버튼 숨기기
                                                                               
            SaveCardTransform();                                               //카드 원래 위치 저장
                                                                               
            for (int i = 0; i < heroes.Count; i++)                             
            {                                                                  
                summon10Cards[i].SetHeroData(heroes[i]);                       //카드에 영웅 데이터 적용
            }

            tenSummonCoroutine = StartCoroutine(PlayTenSummonAnimation());     //카드 소환 코루틴 (1장씩 챠라락)
        }

        #region 연출스킵
        //1회소환 - 연출스킵
        private void StopOneSummonAnim()
        {
            if (oneSummonSequence != null)
            {
                oneSummonSequence.Kill();
                oneSummonSequence = null;
            }

            RectTransform cardRect = summon1Card.GetComponent<RectTransform>();

            Vector3 riseStartPosition = summonRise.transform.parent.InverseTransformPoint(summonPoint.position);
            Vector3 flashCenterPosition = summonFlash.transform.parent.InverseTransformPoint(summonPointCenter.position);


            cardRect.DOKill();
            cardRect.localScale = oneCardOriginScale;

            //Rise 종료
            summonRise.transform.DOKill();
            summonRise.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            summonRise.transform.localScale = riseOriginScale;
            summonRise.transform.localPosition = riseStartPosition;

            //Flash 종료
            summonFlash.transform.DOKill();
            summonFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            summonFlash.transform.localScale = flashOriginScale;
            summonFlash.transform.localPosition = flashCenterPosition;

            isSummon = false;
        }

        //10회소환 - 카드 초기 위치 저장
        private void SaveCardTransform()
        {
            if (isSaveCard) return;

            cardOriginPositions.Clear();
            cardOriginScales.Clear();

            foreach (UISummonCard card in summon10Cards)
            {
                RectTransform cardRect = card.GetComponent<RectTransform>();

                cardOriginPositions.Add(cardRect.anchoredPosition);
                cardOriginScales.Add(cardRect.localScale);
            }

            isSaveCard = true;
        }

        //10회소환 - 연출스킵을 위한 카드위치 복구
        private void ResetCardTransform()
        {
            if (!isSaveCard) return;

            for (int i = 0; i < summon10Cards.Count; i++)
            {
                RectTransform cardRect = summon10Cards[i].GetComponent<RectTransform>();

                cardRect.DOKill();

                cardRect.anchoredPosition = cardOriginPositions[i];
                cardRect.localScale = cardOriginScales[i];
            }
        }

        //10회소환 - 연출스킵
        private void StopTenSummonAnim()
        {
            if (tenSummonCoroutine != null)
            {
                StopCoroutine(tenSummonCoroutine); //연출 코루틴 종료
                tenSummonCoroutine = null;
            }

            ResetCardTransform(); //카드 원래 위치로 복구

            Vector3 riseStartPosition = summonRise.transform.parent.InverseTransformPoint(summonPoint.position);
            Vector3 flashCenterPosition = summonFlash.transform.parent.InverseTransformPoint(summonPointCenter.position);

            //Rise 종료
            summonRise.transform.DOKill();
            summonRise.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            summonRise.transform.localScale = riseOriginScale;
            summonRise.transform.localPosition = riseStartPosition;

            //Flash 종료
            summonFlash.transform.DOKill();
            summonFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            summonFlash.transform.localScale = flashOriginScale;
            summonFlash.transform.localPosition = flashCenterPosition;

            isSummon = false;
            openAllButton.gameObject.SetActive(true); //연출 스킵 후 일괄 오픈 표시
        }

        #endregion

        //10회소환 - 일괄오픈버튼
        public void OnClickedOpenAll()
        {
            openAllButton.gameObject.SetActive(false);

            foreach (UISummonCard card in summon10Cards)
            {
                card.OnClickedFlip();
            }
        }

        //소환 결과 닫기
        public void OnClickedCloseResult()
        {
            //1회소환
            if (summon1Panel.activeSelf && isSummon)
            {
                StopOneSummonAnim();
                return;
            }

            if (summon1Panel.activeSelf && !summon1Card.IsOpen)
            {
                return;
            }

            //10회소환
            if (summon10Panel.activeSelf && isSummon)
            {
                StopTenSummonAnim();
                return;
            }
            if (summon10Panel.activeSelf)                     
            {
                foreach (UISummonCard card in summon10Cards)
                {
                    if (!card.IsOpen) return;
                }
            }

            summon1Panel.SetActive(false);
            summon10Panel.SetActive(false);

            backButton.interactable = false;
            backButton.gameObject.SetActive(false);
        }

        //모든 카드 오픈 여부 확인
        private void CheckAllCardsOpened()
        {
            //1회 소환
            if (summon1Panel.activeSelf)
            {
                if (summon1Card.IsOpen)
                {
                    backButton.interactable = true;
                }

                return;
            }

            //10회 소환
            if (summon10Panel.activeSelf)
            {
                foreach (UISummonCard card in summon10Cards)
                {
                    //하나라도 안 열렸으면 아직 닫기 불가
                    if (!card.IsOpen)
                    {
                        return;
                    }
                }

                //10장 모두 열림
                backButton.interactable = true;
            }
        }
        #endregion

        #region 연출

        //1회 소환 연출
        private void PlayOneSummonAnimation()
        {
            isSummon = true;

            RectTransform cardRect = summon1Card.GetComponent<RectTransform>();

            //초기화//
            Vector3 riseStartPosition = summonRise.transform.parent.InverseTransformPoint(summonPoint.position);
            Vector3 riseCenterPosition = summonRise.transform.parent.InverseTransformPoint(summonPointCenter.position);
            Vector3 flashCenterPosition = summonFlash.transform.parent.InverseTransformPoint(summonPointCenter.position);

            //카드                                                                                                                  
            cardRect.DOKill();                                                                                                       //이전 Tween 제거                         
            cardRect.localScale = Vector3.zero;                                                                                      //처음에 카드크기는 안보이게
                                                                                                                                     
            //Rise                                                                                                                   
            summonRise.transform.DOKill();                                                                                           //이전 Tween 제거
            summonRise.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);                                                  //이펙트 초기화
            summonRise.transform.localScale = riseOriginScale;                                                                       //원래 크기로 복구
            summonRise.transform.localPosition = riseStartPosition;                                                                  //시작 위치는 제단
                                                                                                                                     
            //Flash                                                                                                                  
            summonFlash.transform.DOKill();                                                                                          //이전 Tween 제거
            summonFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);                                                 //이펙트 초기화
            summonFlash.transform.localScale = flashOriginScale;                                                                     //원래 크기로 복구
            summonFlash.transform.localPosition = flashCenterPosition;                                                               //시작 위치는 중앙
                                                                                                                                    
            oneSummonSequence?.Kill();
            oneSummonSequence = DOTween.Sequence();

            //1. Rise : 제단 -> 중앙 이동 후 잠깐 대기                                                                           
            summonRise.Play(true);                                                                                                   //Rise 이펙트 재생
            oneSummonSequence.Append(summonRise.transform.DOLocalMove(riseCenterPosition, riseDuration).SetEase(Ease.InOutSine));    //이동
            oneSummonSequence.AppendInterval(waitDuration);                                                                          //대기

            //2. Flash : 재생 전 잠깐 작아졌다가 플래쉬 터트리고 잠깐 대기                                                         
            oneSummonSequence.Append(summonRise.transform.DOScale(riseOriginScale * 0.65f, 0.1f).SetEase(Ease.InQuad));              //작아졌다가
            oneSummonSequence.AppendCallback(() =>                                                                                           
            {                                                                                                                  
                summonFlash.Play(true);                                                                                               //Flash 이펙트 재생
                summonRise.Stop(true, ParticleSystemStopBehavior.StopEmitting);                                                       //Rise 이펙트 종료
            });
            oneSummonSequence.AppendInterval(0.15f);                                                                                 //대기

            //3. 카드등장 : 아주 작게 시작해서 크기 키운 뒤 기존 크기로 등장                                                       
            oneSummonSequence.AppendCallback(() => { cardRect.localScale = oneCardOriginScale * 0.05f; });                           //카드크기 작게 시작해서
            oneSummonSequence.Append(cardRect.DOScale(oneCardOriginScale * 1.08f, 0.22f).SetEase(Ease.OutBack));                     //원래 크기보다 살짝 더 키우고
            oneSummonSequence.Append(cardRect.DOScale(oneCardOriginScale, 0.1f).SetEase(Ease.OutQuad));                              //기존크기로 등장

            oneSummonSequence.OnComplete(() =>                                                                                       //연출 종료
            { 
                summonFlash.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                isSummon = false;
                oneSummonSequence = null;
            });
        }


        //10회 소환 연출
        private IEnumerator PlayTenSummonAnimation()
        {
            isSummon = true;

            //초기화//                                                                                                          
            Vector3 riseStartPosition = summonRise.transform.parent.InverseTransformPoint(summonPoint.position);
            Vector3 riseCenterPosition = summonRise.transform.parent.InverseTransformPoint(summonPointCenter.position);
            Vector3 flashCenterPosition = summonFlash.transform.parent.InverseTransformPoint(summonPointCenter.position);

            //카드                                                                                                             
            for (int i = 0; i < summon10Cards.Count; i++)
            {
                RectTransform cardRect = summon10Cards[i].GetComponent<RectTransform>();

                cardRect.DOKill();

                cardRect.anchoredPosition = cardOriginPositions[i];
                cardRect.localScale = Vector3.zero;
            }

            //Rise                                                                                                             
            summonRise.transform.DOKill();                                                                                     //이전 Tween 제거
            summonRise.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);                                            //이펙트 초기화
            summonRise.transform.localScale = riseOriginScale;                                                                 //원래 크기로 복구
            summonRise.transform.localPosition = riseStartPosition;                                                            //시작 위치는 제단

            //Flash                                                                                                            
            summonFlash.transform.DOKill();                                                                                    //이전 Tween 제거
            summonFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);                                           //이펙트 초기화
            summonFlash.transform.localScale = flashOriginScale;                                                               //원래 크기로 복구
            summonFlash.transform.localPosition = flashCenterPosition;                                                         //시작 위치는 중앙


            //1. Rise : 제단 -> 중앙 이동 후 잠깐 대기
            summonRise.Play(true); //Rise 이펙트 재생
            summonRise.transform.DOLocalMove(riseCenterPosition, riseDuration).SetEase(Ease.InOutSine);                        //이동
            yield return new WaitForSeconds(riseDuration);                                                                     //다른 카드들 이동완료까지 대기
            yield return new WaitForSeconds(waitDuration);                                                                     //다같이 잠깐 대기
                                                                                                                               
            //2. Flash : Rise가 작아진 뒤 Flash 재생 후 잠깐 대기
            yield return summonRise.transform.DOScale(riseOriginScale * 0.65f, 0.1f).SetEase(Ease.InQuad).WaitForCompletion(); //작아졌다가
            summonFlash.Play(true);                                                                                            //Flash이펙트 재생
            summonRise.Stop(true, ParticleSystemStopBehavior.StopEmitting);                                                    //Rise 이펙트 종료
            yield return new WaitForSeconds(0.15f);                                                                            //대기

            //3. 카드 등장 : 중앙에서 시작해서 한장씩 기존위치로 퍼지면서 등장
            for (int i = 0; i < summon10Cards.Count; i++)
            {
                RectTransform cardRect = summon10Cards[i].GetComponent<RectTransform>();

                Vector2 targetPosition = cardOriginPositions[i];                                                               //최종위치
                Vector3 targetScale = cardOriginScales[i];                                                                     //최종크기
                                                                                                                               
                cardRect.position = summonPointCenter.position;                                                                //모두 중앙에서 시작
                cardRect.localScale = targetScale * 0.08f;                                                                     //크기는 작게 시작
                                                                                                                               
                Sequence seq = DOTween.Sequence();                                                                             
                                                                                                                               
                seq.Append(cardRect.DOAnchorPos(targetPosition, spreadDuration).SetEase(Ease.OutCubic));                       //중앙 -> 최종 위치로 이동
                seq.Join(cardRect.DOScale(targetScale * 1.05f, spreadDuration).SetEase(Ease.OutBack));                         //이동하면서 기존크기보다 살짝 더 크게
                seq.Append(cardRect.DOScale(targetScale, 0.08f).SetEase(Ease.OutQuad));                                        //최종크기 설정
                                                                                                                               
                yield return new WaitForSeconds(cardInterval);                                                                 //다음카드 배치대기
            }

            yield return new WaitForSeconds(spreadDuration + 0.08f);

            summonFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            isSummon = false;
            tenSummonCoroutine = null;
            openAllButton.gameObject.SetActive(true); 
        }

        #endregion
    }

}
