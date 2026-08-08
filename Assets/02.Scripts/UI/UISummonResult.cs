using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AFKHero.UI
{
    //소환된 영웅의 결과 카드를 보여주는 UI
    public class UISummonResult : MonoBehaviour
    {
        [Header("1회 소환")]
        [SerializeField] private GameObject summon1Panel;  //1회 소환 결과 영역
        [SerializeField] private UISummonCard summon1Card; //1회 소환 카드

        [Header("10회 소환")]
        [SerializeField] private GameObject summon10Panel;         //10회 소환 결과 영역
        [SerializeField] private List<UISummonCard> summon10Cards; //10회 소환 카드

        [Header("소환 연출")]
        [SerializeField] private RectTransform summonPoint;       //카드가 시작하는 제단 위치
        [SerializeField] private RectTransform summonPointCenter; //카드가 모이는 중앙 위치
        [SerializeField] private ParticleSystem summonRise;       //이펙트 : 제단 -> 중앙으로 올라오는 빛
        [SerializeField] private ParticleSystem summonFlash;      //이펙트 : 중앙에서 터지는 빛
        private float riseDuration = 1.5f;      //제단 -> 중앙 이동시간
        private float waitDuration = 0.2f;      //중앙 대기 시간
        private float spreadDuration = 0.12f;   //중앙 -> 최종위치 이동시간
        private float cardInterval = 0.07f;     //10장 배치 간격
        private Vector3 riseOriginScale;
        private Vector3 flashOriginScale; 


        private void Awake()
        {
            riseOriginScale = summonRise.transform.localScale;
            flashOriginScale = summonFlash.transform.localScale;
        }

        #region 소환결과

        //소환 결과
        public void ShowResult(List<HeroData> heroes)
        {
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

            summon1Card.SetHeroData(hero); //카드에 영웅 데이터 적용
            PlayOneSummonAnimation();      //카드소환
        }

        //10회 소환 결과
        private void ShowTenResult(List<HeroData> heroes)
        {
            summon1Panel.SetActive(false);
            summon10Panel.SetActive(true);  //10회 영역만 활성화

            for (int i = 0; i < heroes.Count; i++)
            {
                summon10Cards[i].SetHeroData(heroes[i]); //카드에 영웅 데이터 적용
            }

           StartCoroutine(PlayTenSummonAnimation()); //카드 소환 코루틴 (1장씩 챠라락)
        }

        //10회소환 - 일괄오픈버튼
        public void OnClickedOpenAll()
        {
            foreach (UISummonCard card in summon10Cards)
            {
                card.OnClickedFlip();
            }
        }

        #endregion

        #region 연출

        //1회 소환 연출
        private void PlayOneSummonAnimation()
        {
            RectTransform cardRect = summon1Card.GetComponent<RectTransform>();

            //초기화//

            //카드
            Vector3 originScale = cardRect.localScale;  //카드 기존 크기 저장
            cardRect.DOKill();                          //이전 Tween 제거                         
            cardRect.localScale = Vector3.zero;         //처음에 카드크기는 안보이게

            //Rise
            summonRise.transform.DOKill();                                           //이전 Tween 제거
            summonRise.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);  //이펙트 초기화
            summonRise.transform.localScale = riseOriginScale;                       //원래 크기로 복구
            summonRise.transform.position = summonPoint.position;                    //시작 위치는 제단

            //Flash
            summonFlash.transform.DOKill();                                          //이전 Tween 제거
            summonFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); //이펙트 초기화
            summonFlash.transform.localScale = flashOriginScale;                     //원래 크기로 복구
            summonFlash.transform.position = summonPointCenter.position;             //시작 위치는 중앙
            

            Sequence seq = DOTween.Sequence();

            //1. Rise : 제단 -> 중앙 이동 후 잠깐 대기
            summonRise.Play(true);                                                                                     //Rise 이펙트 재생
            seq.Append(summonRise.transform.DOMove(summonPointCenter.position, riseDuration).SetEase(Ease.InOutSine)); //이동
            seq.AppendInterval(waitDuration);                                                                          //대기

            //2. Flash : 재생 전 잠깐 작아졌다가 플래쉬 터트리고 잠깐 대기
            seq.Append(summonRise.transform.DOScale(riseOriginScale * 0.65f, 0.1f).SetEase(Ease.InQuad)); //작아졌다가
            seq.AppendCallback(() =>
            {
                summonFlash.Play(true);                                                                   //Flash 이펙트 재생
                summonRise.Stop(true, ParticleSystemStopBehavior.StopEmitting);                           //Rise 이펙트 종료
            });                                                                                           
            seq.AppendInterval(0.15f);                                                                    //대기

            //3. 카드등장 : 아주 작게 시작해서 크기 키운 뒤 기존 크기로 등장
            seq.AppendCallback(() => { cardRect.localScale = originScale * 0.05f; });       //카드크기 작게 시작해서
            seq.Append(cardRect.DOScale(originScale * 1.08f, 0.22f).SetEase(Ease.OutBack)); //원래 크기보다 살짝 더 키우고
            seq.Append(cardRect.DOScale(originScale, 0.1f).SetEase(Ease.OutQuad));          //기존크기로 등장
        }


        //10회 소환 연출
        private IEnumerator PlayTenSummonAnimation()
        {
            List<Vector3> originPositions = new List<Vector3>(); //카드 기존위치 저장
            List<Vector3> originScales = new List<Vector3>();    //카드 기존크기 저장

            //초기화//

            //카드
            foreach (UISummonCard card in summon10Cards)
            {
                RectTransform cardRect = card.GetComponent<RectTransform>();

                originPositions.Add(cardRect.position); //카드 기존위치 저장
                originScales.Add(cardRect.localScale);  //카드 기존크기 저장
                cardRect.DOKill();                      //이전 Tween 제거
                cardRect.localScale = Vector3.zero;     //처음에는 카드 안보이게
            }

            //Rise
            summonRise.transform.DOKill();                                           //이전 Tween 제거
            summonRise.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);  //이펙트 초기화
            summonRise.transform.localScale = riseOriginScale;                       //원래 크기로 복구
            summonRise.transform.position = summonPoint.position;                    //시작 위치는 제단

            //Flash
            summonFlash.transform.DOKill();                                          //이전 Tween 제거
            summonFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); //이펙트 초기화
            summonFlash.transform.localScale = flashOriginScale;                     //원래 크기로 복구
            summonFlash.transform.position = summonPointCenter.position;             //시작 위치는 중앙


            //1. Rise : 제단 -> 중앙 이동 후 잠깐 대기
            summonRise.Play(true); //Rise 이펙트 재생
            summonRise.transform.DOMove(summonPointCenter.position, riseDuration).SetEase(Ease.InOutSine); //이동
            yield return new WaitForSeconds(riseDuration);                                                 //다른 카드들 이동완료까지 대기
            yield return new WaitForSeconds(waitDuration);                                                 //다같이 잠깐 대기

            //2. Flash : Rise가 작아진 뒤 Flash 재생 후 잠깐 대기
            yield return summonRise.transform.DOScale(riseOriginScale * 0.65f, 0.1f).SetEase(Ease.InQuad).WaitForCompletion(); //작아졌다가
            summonFlash.Play(true);                                                                                            //Flash이펙트 재생
            summonRise.Stop(true, ParticleSystemStopBehavior.StopEmitting);                                                    //Rise 이펙트 종료
            yield return new WaitForSeconds(0.15f);                                                                            //대기

            //3. 카드 등장 : 중앙에서 시작해서 한장씩 기존위치로 퍼지면서 등장
            for (int i = 0; i < summon10Cards.Count; i++)
            {
                RectTransform cardRect = summon10Cards[i].GetComponent<RectTransform>();

                Vector3 targetPosition = originPositions[i];                                           //최종위치
                Vector3 targetScale = originScales[i];                                                 //최종크기
                                                                                                       
                cardRect.position = summonPointCenter.position;                                        //모두 중앙에서 시작
                cardRect.localScale = targetScale * 0.08f;                                             //크기는 작게 시작

                Sequence seq = DOTween.Sequence();

                seq.Append(cardRect.DOMove(targetPosition, spreadDuration).SetEase(Ease.OutCubic));    //중앙 -> 최종 위치로 이동
                seq.Join(cardRect.DOScale(targetScale * 1.05f, spreadDuration).SetEase(Ease.OutBack)); //이동하면서 기존크기보다 살짝 더 크게
                seq.Append(cardRect.DOScale(targetScale, 0.08f).SetEase(Ease.OutQuad));                //최종크기 설정

                yield return new WaitForSeconds(cardInterval);                                         //다음카드 배치대기
            }
        }

        #endregion
    }

}
