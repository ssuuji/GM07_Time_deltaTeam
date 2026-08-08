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

        [Header("Summon Animation")]
        [SerializeField] private RectTransform summonPoint;       //카드가 시작하는 제단 위치
        [SerializeField] private RectTransform summonPointCenter; //카드가 모이는 중앙 위치

        [SerializeField] private float riseDuration = 0.5f;       //제단 → 중앙 이동시간
        [SerializeField] private float spreadDuration = 0.12f;    //중앙 → 최종위치 이동시간
        [SerializeField] private float cardInterval = 0.07f;      //10장 배치 간격

        [Header("Summon Effect")]
        [SerializeField] private ParticleSystem summonRise;   //제단 → 중앙으로 올라오는 빛
        [SerializeField] private ParticleSystem summonFlash;  //중앙에서 터지는 빛

        [SerializeField] private float glowRiseDuration = 0.6f; //Glow 상승 시간
        [SerializeField] private float glowWaitDuration = 0.15f; //중앙 대기 시간
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

            PlayOneSummonAnimation();
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

           StartCoroutine(PlayTenSummonAnimation());
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

        //====================================
        // 1회 소환 연출
        //====================================
        private void PlayOneSummonAnimation()
        {
            RectTransform cardRect = summon1Card.GetComponent<RectTransform>();

            //카드 최종 위치와 크기 저장
            Vector3 targetPosition = cardRect.position;
            Vector3 targetScale = cardRect.localScale;

            //이전 Tween 제거
            cardRect.DOKill();
            summonRise.transform.DOKill();
            summonFlash.transform.DOKill();


            //====================================
            // 카드 초기화
            //====================================

            cardRect.position = targetPosition;
            cardRect.localScale = Vector3.zero;


            //====================================
            // Rise 초기화
            //====================================

            summonRise.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            //제단에서 시작
            summonRise.transform.position = summonPoint.position;

            //처음부터 원래 크기
            summonRise.transform.localScale = riseOriginScale;


            //====================================
            // Flash 초기화
            //====================================

            summonFlash.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            summonFlash.transform.position =
                summonPointCenter.position;

            summonFlash.transform.localScale =
                flashOriginScale;


            //====================================
            // Rise 재생
            //====================================

            summonRise.Play(true);


            Sequence sequence = DOTween.Sequence();


            //====================================
            // 1. Rise가 제단 → 중앙 상승
            //====================================

            sequence.Append(
                summonRise.transform.DOMove(
                    summonPointCenter.position,
                    glowRiseDuration
                )
                .SetEase(Ease.InOutSine)
            );


            //====================================
            // 2. 중앙에서 빛이 모이는 시간
            //====================================

            sequence.AppendInterval(0.20f);


            //====================================
            // 3. Flash 직전 응축
            //====================================

            sequence.Append(
                summonRise.transform.DOScale(
                    riseOriginScale * 0.65f,
                    0.10f
                )
                .SetEase(Ease.InQuad)
            );


            //====================================
            // 4. Flash
            //====================================

            sequence.AppendCallback(() =>
            {
                summonFlash.transform.position =
                    summonPointCenter.position;

                summonFlash.Play(true);

                summonRise.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmitting
                );
            });


            //Flash를 조금 더 보여줌
            sequence.AppendInterval(0.15f);


            //====================================
            // 5. 카드 등장
            //====================================

            sequence.AppendCallback(() =>
            {
                //거의 점처럼 작은 상태에서 등장
                cardRect.localScale =
                    targetScale * 0.05f;
            });

            sequence.Append(
                cardRect.DOScale(
                    targetScale * 1.08f,
                    0.22f
                )
                .SetEase(Ease.OutBack)
            );


            //====================================
            // 6. 카드 안착
            //====================================

            sequence.Append(
                cardRect.DOScale(
                    targetScale,
                    0.10f
                )
                .SetEase(Ease.OutQuad)
            );
        }


        //====================================
        // 10회 소환 연출
        //====================================
        private IEnumerator PlayTenSummonAnimation()
        {
            List<Vector3> targetPositions = new List<Vector3>();
            List<Vector3> targetScales = new List<Vector3>();


            //====================================
            // 1. 카드 원래 위치 / 크기 저장
            //====================================

            foreach (UISummonCard card in summon10Cards)
            {
                RectTransform cardRect =
                    card.GetComponent<RectTransform>();

                targetPositions.Add(cardRect.position);
                targetScales.Add(cardRect.localScale);

                cardRect.DOKill();

                //처음에는 숨김
                cardRect.localScale = Vector3.zero;
            }


            //====================================
            // 2. Rise / Flash 초기화
            //====================================

            summonRise.transform.DOKill();
            summonFlash.transform.DOKill();

            summonRise.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            summonFlash.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );


            summonRise.transform.position = summonPoint.position;
            summonRise.transform.localScale = riseOriginScale;


            summonFlash.transform.position =
                summonPointCenter.position;

            summonFlash.transform.localScale =
                flashOriginScale;


            //====================================
            // 3. Rise 재생
            //====================================

            summonRise.Play(true);


            //====================================
            // 4. 제단 → 중앙 상승
            //====================================

            summonRise.transform.DOMove(
                summonPointCenter.position,
                glowRiseDuration
            )
            .SetEase(Ease.InOutSine);



            yield return new WaitForSeconds(
                glowRiseDuration
            );


            //====================================
            // 5. 중앙에서 잠깐 유지
            //====================================

            yield return new WaitForSeconds(0.20f);


            //====================================
            // 6. 응축
            //====================================

            yield return summonRise.transform
                .DOScale(
                    riseOriginScale * 0.65f,
                    0.10f
                )
                .SetEase(Ease.InQuad)
                .WaitForCompletion();


            //====================================
            // 7. Flash
            //====================================

            summonFlash.transform.position =
                summonPointCenter.position;

            summonFlash.Play(true);

            summonRise.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );


            //Flash를 충분히 보여줌
            yield return new WaitForSeconds(0.15f);


            //====================================
            // 8. 카드 한 장씩 중앙 → 최종 위치
            //====================================

            for (int i = 0; i < summon10Cards.Count; i++)
            {
                RectTransform cardRect =
                    summon10Cards[i].GetComponent<RectTransform>();

                Vector3 targetPosition =
                    targetPositions[i];

                Vector3 targetScale =
                    targetScales[i];


                //모두 중앙에서 시작
                cardRect.position =
                    summonPointCenter.position;

                //좀 더 작게 시작
                cardRect.localScale =
                    targetScale * 0.08f;


                Sequence cardSequence =
                    DOTween.Sequence();


                //중앙 → 최종 위치
                cardSequence.Append(
                    cardRect.DOMove(
                        targetPosition,
                        spreadDuration
                    )
                    .SetEase(Ease.OutCubic)
                );


                //이동하면서 커짐
                cardSequence.Join(
                    cardRect.DOScale(
                        targetScale * 1.05f,
                        spreadDuration
                    )
                    .SetEase(Ease.OutBack)
                );


                //살짝 안착
                cardSequence.Append(
                    cardRect.DOScale(
                        targetScale,
                        0.08f
                    )
                    .SetEase(Ease.OutQuad)
                );


                yield return new WaitForSeconds(
                    cardInterval
                );
            }
        }

        #endregion
    }

}
