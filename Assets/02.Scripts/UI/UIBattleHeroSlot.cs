using AFKHero.Battle;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AFKHero.UI
{
    //전투화면 하단 영웅 슬롯 UI
    public class UIBattleHeroSlot : MonoBehaviour, IPointerClickHandler
    {
        [Header("영웅 정보")]
        [SerializeField] private Image heroIcon;                  //영웅 아이콘
        [SerializeField] private TMP_Text heroNameText;           //영웅 이름
        [SerializeField] private Slider hpSlider;                 //HP
        [SerializeField] private Slider ultimateSlider;           //궁극기 게이지

        [Header("등급")]
        [SerializeField] private Image gradeImage;                //영웅 등급에 따라 변경되는 카드 테두리
        [SerializeField] private Sprite normal;                   //노멀 / 노멀+ 
        [SerializeField] private Sprite rare;                     //레어 / 레어+ 
        [SerializeField] private Sprite epic;                     //에픽 / 에픽+ 

        [Header("궁극기 연출")]
        [SerializeField] private GameObject ultimateReadyEffect;  //궁극기 준비 완료 이펙트
        [SerializeField] private RectTransform cardTransform;     //확대/이동할 카드 영역
        [SerializeField] private float readyScale = 1.3f;         //카드 확대 배율
        [SerializeField] private float readyMoveY = 64f;          //카드가 위로 이동하는 거리
        [SerializeField] private float scaleDuration = 0.15f;     //연출 시간
        private Vector3 originalCardScale;                        //카드의 원래 크기
        private Vector2 originalCardPosition;                     //카드의 원래 위치
        private Tween scaleTween;                                 //카드 크기 Tween
        private Tween moveTween;                                  //카드 위치 Tween
        private bool canUseUltimate;                              //궁극기 사용 영웅 판별
        private BattleUnit battleUnit;
        private UnitUltimateController ultimateController;

        [Header("사망 연출")]
        [SerializeField] private Material grayscaleMaterial;      //흑백 머티리얼
        private Image[] cardImages;                               //카드 흑백 처리
        private Material[] originalMaterials;                     //기존 material 저장
        

        private void Awake()
        {
            originalCardScale = cardTransform.localScale;          //기본 크기 저장
            originalCardPosition = cardTransform.anchoredPosition; //기본 위치 저장

            cardImages = GetComponentsInChildren<Image>(true);     //흑백처리를 위한 Image 컴포넌트 저장
            
            originalMaterials = new Material[cardImages.Length]; 
            for (int i = 0; i < cardImages.Length; i++)
            {
                originalMaterials[i] = cardImages[i].material;     //기존 material 저장
            }
        }

        private void OnDestroy()
        {
            ResetBattleUnit(); //구독해제 + 연출제거
        }

        #region 영웅 및 전투 유닛 설정

        //영웅 기본 정보 적용
        public void SetHero(HeroInstance hero)
        {
            if (hero == null || hero.data == null)
            {
                Hide(); //슬롯 숨김
                return;
            }

            ResetBattleUnit(); //이전 전투 유닛 연결 및 이벤트 해제

            gameObject.SetActive(true); //활성화
            SetDeadEffect(false); //흑백처리 해제

            heroIcon.sprite = hero.data.HeroIcon;    //아이콘
            heroNameText.text = hero.data.HeroName;  //이름
            SetHP(hero.FinalMaxHP, hero.FinalMaxHP); //HP
            SetUltimate(0f, 1f);                     //궁극기
            SetHeroGrade(hero.currentGrade);         //등급 테두리
        }

        //실제 전투에 생성된 BattleUnit을 슬롯과 연결
        public void SetBattleUnit(BattleUnit unit)
        {
            ResetBattleUnit(); //구독해제 + 연출제거

            battleUnit = unit;
            if (battleUnit == null) return;

            ultimateController = battleUnit.UltimateController;

            canUseUltimate =
                ultimateController != null && ultimateController.CheckCanUseUltimate(); //궁극기를 사용하는 영웅인지 판별

            ultimateController = battleUnit.GetComponent<UnitUltimateController>();
            if (ultimateController != null)
            {
                ultimateController.ExecutionStarted += OnUltimateStarted;     //궁극기 실행 이벤트 구독
                ultimateController.ExecutionCompleted += OnUltimateCompleted; //궁극기 종료 이벤트 구독
            }

            if (battleUnit.Health != null)
            {
                SetHP(battleUnit.Health.CurrentHealth, battleUnit.Health.MaxHealth); //HP 반영
                SetDeadEffect(battleUnit.Health.CurrentHealth <= 0);                 //흑백처리
                battleUnit.Health.HealthChanged += OnHealthChanged;                  //HP 변경 이벤트 구독
            }

            if (battleUnit.Energy != null)
            {
                SetUltimate(battleUnit.Energy.CurrentEnergy, battleUnit.Energy.MaxEnergy); //궁극기게이지 반영
                battleUnit.Energy.EnergyChanged += OnEnergyChanged;                        //궁극기게이지 변경 이벤트 구독
            }
        }

        #endregion

        #region 전투 유닛 이벤트

        //BattleUnit의 HP 변경 이벤트
        private void OnHealthChanged(BattleUnit unit, int currentHealth, int maxHealth)
        {
            SetHP(currentHealth, maxHealth); //변경된 HP 반영

            
            bool isDead = currentHealth <= 0; //사망여부
            SetDeadEffect(isDead);            //흑백처리
            if (isDead)
            {
                ResetUltimateEffect(); //사망했다면 연출제거
            }
        }

        //BattleUnit의 궁극기게이지 변경 이벤트
        private void OnEnergyChanged(BattleUnit unit, int currentEnergy, int maxEnergy)
        {
            SetUltimate(currentEnergy, maxEnergy); //변경된 궁극기게이지 반영
        }

        //궁극기 실행 이벤트
        private void OnUltimateStarted(BattleUnit unit)
        {
            PlayUltimateStartEffect(); //연출시작
        }

        //궁극기 종료 이벤트
        private void OnUltimateCompleted(BattleUnit unit)
        {
            PlayUltimateEndEffect();   //연출종료
        }

        #endregion

        #region UI 입력

        //영웅 카드 클릭 : IPointerClickHandler는 버튼클릭연결 없이 카드의 클릭을 받을 수 있음!
        public void OnPointerClick(PointerEventData eventData)
        {
            if (battleUnit == null || !canUseUltimate) return;

            UIBattleManager.Instance?.TryUseUltimate(battleUnit); //해당 카드를 클릭했다는 정보만 전달 -> 이후 작업은 Battlemanager에서 판단
        }

        #endregion

        #region 궁극기 연출

        //연출 시작 : 카드테두리 이펙트를 활성화 + 확대 + 위로 쪼금 올리기
        private void PlayUltimateStartEffect()
        {
            if (!canUseUltimate) return; //HeroData 에서 궁극기룰 사용하지 않는 영웅은 연출제외

            SetUltimateReadyEffect(true); //카드테두리 이펙트를 활성화(궁극기 준비 완료 표시 이펙트)

            scaleTween?.Kill();
            moveTween?.Kill();

            scaleTween = cardTransform.DOScale(originalCardScale * readyScale, scaleDuration).SetEase(Ease.OutQuad);          //카드 확대
            moveTween = cardTransform.DOAnchorPosY(originalCardPosition.y + readyMoveY, scaleDuration).SetEase(Ease.OutQuad); //위로 이동
        }

        //연출 종료
        private void PlayUltimateEndEffect()
        {
            SetUltimateReadyEffect(false); //이펙트 제거

            scaleTween?.Kill();
            moveTween?.Kill();

            scaleTween = cardTransform.DOScale(originalCardScale, scaleDuration).SetEase(Ease.OutQuad);       //크기 복구
            moveTween = cardTransform.DOAnchorPos(originalCardPosition, scaleDuration).SetEase(Ease.OutQuad); //위치 복구 
        }

        //궁극기 준비 완료 이펙트 활성화/비활성화
        private void SetUltimateReadyEffect(bool isReady)
        {
            if (ultimateReadyEffect == null) return;

            ultimateReadyEffect.SetActive(isReady);
        }

        //궁극기 관련 연출 모두 제거
        private void ResetUltimateEffect()
        {
            scaleTween?.Kill();
            moveTween?.Kill();

            scaleTween = null;
            moveTween = null;

            if (ultimateReadyEffect != null)
            {
                ultimateReadyEffect.SetActive(false); //준비 완료 이펙트 비활성화
            }

            if (cardTransform != null)
            {
                cardTransform.localScale = originalCardScale;           //크기 복구
                cardTransform.anchoredPosition = originalCardPosition;  //위치 복구
            }
        }

        #endregion

        #region 사망 연출

        //영웅 사망시 카드 흑백처리
        private void SetDeadEffect(bool isDead)
        {
            if (cardImages == null || originalMaterials == null) return;

            for (int i = 0; i < cardImages.Length; i++)
            {
                if (cardImages[i] == null) continue;

                cardImages[i].material = isDead ? grayscaleMaterial : originalMaterials[i];
            }
        }

        #endregion

        #region UI 갱신

        //HP 게이지 갱신
        public void SetHP(float currentHP, float maxHP)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        //궁극기 게이지 및 준비 완료 이펙트 갱신
        public void SetUltimate(float currentEnergy, float maxEnergy)
        {
            ultimateSlider.maxValue = maxEnergy;
            ultimateSlider.value = currentEnergy;

            //BattleUnit 생존 확인
            bool isAlive = battleUnit != null && battleUnit.Stats != null && battleUnit.Stats.IsAlive;
            if (!canUseUltimate || !isAlive)
            {
                SetUltimateReadyEffect(false);
                return;
            }

            if (maxEnergy > 0 && currentEnergy >= maxEnergy)
            {
                SetUltimateReadyEffect(true); //이펙트 활성화
            }
            else if (ultimateController == null || !ultimateController.IsExecuting) //IsExecuting : 궁극기가 아직 실행중이라면 이펙트 활성화 유지되도록
            {
                SetUltimateReadyEffect(false);
            }
        }

        //등급 테두리 설정
        private void SetHeroGrade(HeroGrade grade)
        {
            switch (grade)
            {
                //Normal
                case HeroGrade.Normal:
                case HeroGrade.NormalPlus:
                    gradeImage.sprite = normal;
                    break;

                //Rare
                case HeroGrade.Rare:
                case HeroGrade.RarePlus:
                    gradeImage.sprite = rare;
                    break;

                //Epic 
                case HeroGrade.Epic:
                case HeroGrade.EpicPlus:
                    gradeImage.sprite = epic;
                    break;
            }
        }

        #endregion

        #region 초기화 및 해제

        //구독해제 + 연출제거
        private void ResetBattleUnit()
        {
            if (battleUnit != null)
            {
                //HP 변경 이벤트 구독 해제
                if (battleUnit.Health != null)
                {
                    battleUnit.Health.HealthChanged -= OnHealthChanged;
                }

                //궁극기 Energy 변경 이벤트 구독 해제
                if (battleUnit.Energy != null)
                {
                    battleUnit.Energy.EnergyChanged -= OnEnergyChanged;
                }
            }

            if (ultimateController != null)
            {
                //궁극기 실행 시작/종료 이벤트 구독 해제
                ultimateController.ExecutionStarted -= OnUltimateStarted;
                ultimateController.ExecutionCompleted -= OnUltimateCompleted;

                ultimateController = null;
            }

            //기존 BattleUnit 참조와 궁극기 사용 가능 상태 초기화
            battleUnit = null;
            canUseUltimate = false;

            //궁극기 연출 초기화
            ResetUltimateEffect();
        }

        //영웅이 존재하지 않는 빈 슬롯 처리
        public void Hide()
        {
            ResetBattleUnit();
            SetDeadEffect(false);
            gameObject.SetActive(false);
        }

        #endregion
    }
}