using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    //카드 1장에 대한 셋팅
    public class UISummonCard : MonoBehaviour
    {
        [Header("Card")]
        [SerializeField] private Image cardImage;        //등급별 카드 테두리
        [SerializeField] private GameObject cardClose;   //카드 뒷면
                                                         
        [Header("Hero")]                                 
        [SerializeField] private Image heroImage;        //영웅 이미지
        [SerializeField] private TMP_Text heroNameText;  //영웅 이름
        [SerializeField] private TMP_Text gradeText;     //영웅 등급

        [Header("Grade")]
        [SerializeField] private Sprite normalImage;     //노멀
        [SerializeField] private Sprite rareImage;       //레어
        [SerializeField] private Sprite epicImage;       //에픽

        private Vector3 originScale;                     //카드 원래크기 저장
        private float flipDuration = 0.2f;               //카드 뒤집기 시간
        private bool isFlip = false;                     //뒤집는 중인지 확인여부
        private bool isOpen = false;                     //오픈된 상태인지 확인여부

        public event Action OnOpened;                    //카드 오픈 완료 이벤트

        public bool IsOpen => isOpen;

        private void Awake()
        {
            originScale = transform.localScale; //원래 카드크기 저장
        }

        #region 카드 셋팅

        //영웅 데이터 적용
        public void SetHeroData(HeroData hero)
        {
            heroImage.sprite = hero.HeroIcon;   //영웅 이미지 적용
            heroNameText.text = hero.HeroName;  //영웅 이름 적용
            SetGrade(hero.HeroGrade);           //등급 테두리 적용

            isOpen = false;                     //새로운 카드니까 오픈전
            isFlip = false;                     //새로운 카드니까 뒤집기전

            ShowBack();                         //카드 뒷면 먼저 보여주기
        }

        //등급 테두리 설정
        private void SetGrade(HeroGrade grade)
        {
            switch (grade)
            {
                case HeroGrade.Normal:
                    cardImage.sprite = normalImage;
                    gradeText.text = "노멀";
                    break;
                case HeroGrade.Rare:
                    cardImage.sprite = rareImage;
                    gradeText.text = "레어";
                    break;
                case HeroGrade.Epic:
                    cardImage.sprite = epicImage;
                    gradeText.text = "에픽";
                    break;
            }
        }

        public void ShowBack() { cardClose.SetActive(true); }   //카드 뒷면 표시
        public void ShowFront() { cardClose.SetActive(false); } //카드 앞면 표시

        #endregion

        #region 카드 뒤집기

        //카드 클릭 버튼
        public void OnClickedFlip()
        {
            if (isFlip) return;
            if (isOpen) return;

            StartCoroutine(FlipCo()); //카드 뒤집기 코루틴
        }

        IEnumerator FlipCo()
        {
            isFlip = true; //뒤집기 시작

            float timer = 0;

            //1. 카드의 x의 크기를 먼저 0으로 줄이기
            while (timer < flipDuration)
            {
                timer += Time.deltaTime;

                float t = timer / flipDuration;
                float x = Mathf.Lerp(1f, 0f, t);

                transform.localScale = new Vector3(originScale.x * x, originScale.y, originScale.z);

                yield return null;
            }

            //2. 이제 앞면으로 보여주기
            ShowFront();
            timer = 0;

            //3. 다시 카드 크기 원래대로
            while (timer < flipDuration)
            {
                timer += Time.deltaTime;

                float t = timer / flipDuration;
                float x = Mathf.Lerp(0f, 1f, t);

                transform.localScale = new Vector3(originScale.x * x, originScale.y, originScale.z);

                yield return null;
            }

            //뒤집기가 끝났으니 정확한 크기로 복구
            transform.localScale = originScale;

            isOpen = true;  //오픈
            isFlip = false; //뒤집기도 끝

            OnOpened?.Invoke(); //카드 오픈 완료 알림
        }
        #endregion

    }
}