using AFKHero.Battle;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace AFKHero.UI
{
    //전투탭 UI 매니저
    public class UIBattleManager : MonoBehaviour
    {
        public static UIBattleManager Instance { get; private set; }

        [Header("하단 영웅 UI")]
        [SerializeField] private Transform heroSlotTransform;     //영웅 슬롯 생성 위치
        [SerializeField] private UIBattleHeroSlot heroSlotPrefab; //영웅 슬롯 프리팹
        private UIBattleHeroSlot[] heroSlots = new UIBattleHeroSlot[5]; //전투 하단에 표시할 영웅 슬롯 5개

        [Header("궁극기 모드")]
        [SerializeField] private BattleManager battleManager;     
        [SerializeField] private TMP_Text ultimateModeText;       //자동(노란색)/수동(흰색) 텍스트
        private readonly Color32 autoYellow = new Color32(255, 220, 60, 255); //자동 모드
        private readonly Color32 manualWhite = new Color32(255, 255, 255, 255); //수동 모드

        [Header("현재 스테이지 정보")]
        [SerializeField] private TMP_Text currentStageText;       //현재 진행 중인 스테이지 표시

        [Header("타이머 및 적 체력")]
        [SerializeField] private GameObject stageTimer;
        [SerializeField] private TMP_Text battleLimitTime;
        [SerializeField] private Slider enemyUnitAllHp;
        [SerializeField] private TMP_Text enemyUnitAllHpText;

        [Header("전투 보상")]
        [SerializeField] private GameObject rewardGoldPanel;      //골드
        [SerializeField] private GameObject rewardDiaPanel;       //다이아
        [SerializeField] private GameObject rewardTicketPanel;    //무료뽑기권
        [SerializeField] private TMP_Text rewardGoldText;         //골드 수량
        [SerializeField] private TMP_Text rewardDiaText;          //다이아 수량
        [SerializeField] private TMP_Text rewardTicketText;       //소환권 수량

        [Header("전투 보상 (장비)")]
        [SerializeField] private GameObject rewardEquipPanel;     //장비 드롭 보상 UI
        [SerializeField] private TMP_Text rewardEquipText;        //드롭된 장비 이름
        private bool isEquipDroppedThisStage = false;             //이번 판에 장비가 떨어졌는지 기억

        [Header("궁극기 Dim")]
        [SerializeField] private SpriteRenderer ultimateDim;       //전투 화면을 어둡게
        [SerializeField] private float dimAlpha = 0.6f;            //Dim 알파값
        [SerializeField] private float dimDuration = 0.15f;        //Dim 페이드 인/아웃 시간
        [SerializeField] private int ultimateSortingOrder = 10000; //궁극기 사용 유닛을 Dim보다 위에 표시
        private BattleUnit highlightedUltimateUnit;                //화면에서 강조되고 있는 궁극기 유닛
        private SortingGroup ultimateSortingGroup;                 //궁극기 사용 유닛을 Dim보다 위에 표시하기 위한 SortingGroup
        private bool addedSortingGroup;                            //기존 SortingGroup이 없어서 런타임에 새로 추가했는지 여부(종료시 제거)
        private int originalSortingOrder;                          //기존 SortingGroup이 존재했다면 궁극기 종료 후 기존 order값 으로 복구

        [Header("궁극기 카메라")]
        [SerializeField] private Camera battleCamera;                         //궁극기 연출 카메라 (확대를 위한)
        [SerializeField] private BoxCollider2D cameraArea;                    //카메라가 이동 범위 제한 영역
        [SerializeField] private float ultimateZoomSize = 3.8f;               //확대 사이즈
        [SerializeField] private float zoomDuration = 0.15f;                  //카메라 이동 및 확대/복귀 시간
        [SerializeField] private float cameraFocusStrength = 1f;              //0: 이동 안 함 / 1: 궁극기 유닛 위치까지 이동
        [SerializeField] private Vector2 ultimateCameraOffset = Vector2.zero; //궁극기 유닛 기준 카메라 포커스 보정값
        private float originalCameraSize;                                     //기존 카메라 사이즈 
        private Vector3 originalCameraPosition;                               //기존 카메라 위치

        [Header("스테이지 비상탈출 버튼")]
        [SerializeField] private RectTransform escapeButton;
        [SerializeField] private RectTransform escapePanel;

        private void Awake()
        {
            Instance = this;
            
            CreateHeroSlots(); //게임 시작 시 하단 영웅 슬롯 5개 생성

            if (battleCamera != null)
            {
                originalCameraSize = battleCamera.orthographicSize;        //기존 카메라 사이즈 
                originalCameraPosition = battleCamera.transform.position;  //기존 카메라 위치
            }

            ToggleEscapePanel(false);
        }

        private void Start()
        {
            if (battleManager != null)
            {
                battleManager.UltimateUseModeChanged += UpdateUltimateModeUI; //궁극기 모드 변경 
                battleManager.UltimateStarted += OnUltimateStarted;           //궁극기 연출 시작
                battleManager.UltimateFinished += OnUltimateFinished;         //궁극기 연출 종료
                battleManager.UnitDied += OnUnitDied;                         //유닛 사망 이벤트 구독
                
                battleManager.StateChanged += OnBattleStateChanged;           //전투 상태 변경
                battleManager.BattleTimeChanged += UpdateBattleTimeUI;        //전투 시간

                UpdateUltimateModeUI(battleManager.UltimateMode);             //현재 궁극기 모드에 맞는 UI 반영
                UpdateBattleTimeUI(battleManager.RemainingBattleTime,battleManager.BattleTimeLimit);
            }
            
            if (StageManager.Instance != null)
            {
                StageManager.Instance.StageStateChanged += OnStageStateChanged; //스테이지 상태 변경 이벤트 구독
                StageManager.Instance.StageStateChanged += ToggleEscapeButton;

                OnStageStateChanged(StageManager.Instance.CurrentState);        //현재 상태 UI 반영

            }
        }

        private void OnDestroy()
        {
            if (battleManager != null)
            {
                //모든 이벤트 구독 해제
                battleManager.UltimateUseModeChanged -= UpdateUltimateModeUI;
                battleManager.UltimateStarted -= OnUltimateStarted;
                battleManager.UltimateFinished -= OnUltimateFinished;
                battleManager.UnitDied -= OnUnitDied;
                battleManager.StateChanged -= OnBattleStateChanged;
                battleManager.BattleTimeChanged -= UpdateBattleTimeUI;
            }

            if (StageManager.Instance != null)
            {
                StageManager.Instance.StageStateChanged -= OnStageStateChanged;
                StageManager.Instance.StageStateChanged -= ToggleEscapeButton;
            }

            //연출 초기화
            ResetUltimateEffect();
        }

        #region 전투 UI

        //전투 탭의 주요 UI 전체 갱신
        public void UpdateBattleUI()
        {
            if (StageManager.Instance?.CurrentState == StageState.Working) return;

            UpdateStageUI(); //현재 진행 중인 스테이지 번호 갱신
            UpdatePartyUI(); //현재 파티 편성 정보를 영웅 슬롯 UI에 반영
        }

        #region 스테이지 비상탈출
        //bool값으로 직접 제어할수도 있고, 아니면 StageManager의 이벤트를 구독할 수도 있다.
        //아... 이벤트로 실행되는 메서드 private로 해도 되는구나...

        //엄....

        //1단은 버튼이 활성화되어야지.
        //그 다음, 버튼을 눌렀을 때 패널이 팝업되어야지
        //그 다음, 내부의 버튼 중 "예"를 눌렀을 때 스테이지가 Idle로 돌아가야지
        //그러니까... 이건 필요해.
        public void ToggleEscapeButton(StageState state) 
        {
            if(escapeButton == null)
            {
                Debug.LogWarning("[UIBattleManager] : 비상탈출 버튼이 등록되지 않았습니다.");
                return;
            }

            //아니... 어차피 이벤트 발행할 때 매개변수는 currentState라고
            //그러니까 이런 데서 StageManager.Instance.CurrentState 이럴 필요 없다고...

            //이 버튼은 스테이지가 진행중일 때만 활성화되어야 한다.
            if(state != StageState.Working)
            {
                escapeButton.gameObject.SetActive(false);
            }
            else
            {
                escapeButton.gameObject.SetActive(true);
            }
        }
                
        //비상탈출 버튼을 누르면 패널을 출력합니다.
        public void OnClickedEscapeButton()
        {
            escapePanel.gameObject.SetActive(true);
        }

        //패널에서 "예" 버튼을 누를 시 실행할 메서드
        public void ConfirmEscape()
        {            
            ToggleEscapePanel(false);

            StageManager.Instance.EscapeStage();
        }

        //패널에서 "아니오" 버튼을 누를 시 실행할 메서드
        public void CancelEscape()
        {
            ToggleEscapePanel(false);
        }

        private void ToggleEscapePanel(bool toggle)
        {

            if (escapePanel == null)
            {
                Debug.LogWarning("[UIBattleManager] : escapePanel이 연결되지 않았습니다.");
                return;
            }

            escapePanel.gameObject.SetActive(toggle);
        }

        #endregion

        //현재 진행 중인 스테이지 번호 갱신
        public void UpdateStageUI()
        {
            if (StageManager.Instance == null || currentStageText == null) return;

            if (StageManager.Instance.CurrentState == StageState.None)
            {
                currentStageText.text = "";
                return;
            }
            if (StageManager.Instance.CurrentState == StageState.Idle)
            {
                currentStageText.text = "훈련중";
                return;
            }

            if (StageManager.Instance.CurrentStageInfo != null)
            {
                currentStageText.text = $"STAGE {StageManager.Instance.CurrentStageNumber}-{StageManager.Instance.CurrentSectionNumber}";
            }
            else
            {
                currentStageText.text = $"STAGE {StageManager.Instance.LastStageNumber}-{StageManager.Instance.LastSectionNumber}";
                UINoticePopup.Instance.ShowTime("모든 스테이지를 클리어했습니다.");
            }                
        }

        //스테이지 상태 변경
        private void OnStageStateChanged(StageState state)
        {
            ResetUltimateEffect(); //스테이지 전투 시작/종료 시 진행 중인 궁극기 연출 강제 초기화

            //스테이지 전투 중일 때만 타이머 표시
            if (stageTimer != null)
            {
                stageTimer.SetActive(state == StageState.Working);
            }

            //실제 전투가 끝나고 방치전투 상태로 돌아왔을 때
            if (state != StageState.Idle) return;

            UpdatePartyUI();
        }

        //남은 전투 시간 갱신
        private void UpdateBattleTimeUI(float remainingTime, float battleTimeLimit)
        {
            if (this.battleLimitTime == null) return;

            this.battleLimitTime.text = remainingTime.ToString("F1"); // .0 표시
        }

        //전투 상태 변경
        private void OnBattleStateChanged(BattleState state)
        {
            if (state != BattleState.Fighting) return;

            SubscribeEnemyHealth();
            UpdateEnemyAllHpUI();
        }

        //적 체력 변경 이벤트 구독
        private void SubscribeEnemyHealth()
        {
            foreach (BattleUnit enemy in battleManager.EnemyUnits)
            {
                if (enemy == null || enemy.Health == null) continue;

                //중복 구독 방지
                enemy.Health.HealthChanged -= OnEnemyHealthChanged;
                enemy.Health.HealthChanged += OnEnemyHealthChanged;
            }
        }

        //적 체력 변경
        private void OnEnemyHealthChanged(BattleUnit unit, int currentHealth, int maxHealth)
        {
            UpdateEnemyAllHpUI();
        }

        //적 전체 체력 갱신
        private void UpdateEnemyAllHpUI()
        {
            if (battleManager == null || enemyUnitAllHp == null) return;

            int currentHealth = 0;
            int maxHealth = 0;

            foreach (BattleUnit enemy in battleManager.EnemyUnits)
            {
                if (enemy == null || enemy.Health == null) continue;

                currentHealth += enemy.Health.CurrentHealth;
                maxHealth += enemy.Health.MaxHealth;
            }

            enemyUnitAllHp.minValue = 0f;
            enemyUnitAllHp.maxValue = 1f;
            float targetValue = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            enemyUnitAllHp.DOKill();
            enemyUnitAllHp.DOValue(targetValue, 0.25f).SetEase(Ease.OutQuad);
            enemyUnitAllHpText.text = $"{currentHealth} / {maxHealth}";
        }

        #endregion

        #region 파티 UI

        //전투 화면 하단에 사용할 영웅 슬롯을 미리 5개 생성
        private void CreateHeroSlots()
        {
            for (int i = 0; i < heroSlots.Length; i++)
            {
                heroSlots[i] = Instantiate(heroSlotPrefab, heroSlotTransform);
                heroSlots[i].Hide(); //파티 정보 적용 전에는 기본 프리팹 모습 숨김
            }
        }

        //현재 파티 편성 정보를 영웅 슬롯 UI에 반영
        public void UpdatePartyUI()
        {
            if (PartyManager.Instance == null) return;

            for (int i = 0; i < heroSlots.Length; i++)
            {
                HeroInstance hero = PartyManager.Instance.partySlots[i];

                if (hero == null || hero.data == null)
                {
                    heroSlots[i].Hide(); //UI 슬롯 숨김
                    continue;
                }

                heroSlots[i].SetHero(hero); //영웅 기본 정보를 UI에 반영
            }
        }

        //아군 BattleUnit을 해당 UI 슬롯과 연결
        public void SetBattleUnit(int slotIndex, BattleUnit unit)
        {
            //전투 하단 UI는 아군만 표시하므로 적 유닛은 연결하지 않음
            if (unit == null || unit.Team != TeamType.Ally) return;

            heroSlots[slotIndex].SetBattleUnit(unit);
        }

        #endregion

        #region 궁극기 모드

        //Auto 버튼 클릭 시 궁극기 자동/수동 모드 전환
        public void OnClickedUltimateMode()
        {
            if (battleManager == null) return;

            battleManager.ToggleUltimateUseMode();
        }

        //현재 궁극기 모드에 따라 Auto 텍스트 색상 갱신
        private void UpdateUltimateModeUI(UltimateUseMode mode)
        {
            if (ultimateModeText == null) return;

            bool isAuto = mode == UltimateUseMode.Auto;
            ultimateModeText.color = isAuto ? autoYellow : manualWhite; //자동(노란색) / 수동(흰색)
        }

        //영웅 카드 클릭을 통해 사용할 궁극기 선택
        public bool TryUseUltimate(BattleUnit unit)
        {
            if (battleManager == null) return false;

            return battleManager.TrySelectQueueUltimate(unit);
        }

        #endregion

        #region 궁극기 연출

        //궁극기 시작
        private void OnUltimateStarted(BattleUnit unit)
        {
            if (unit == null) return;
            
            highlightedUltimateUnit = unit; //현재 연출의 대상 유닛 저장

            ShowUltimateDim();              //배경 및 다른 유닛 어둡게
            HighlightUltimateUnit(unit);    //궁극기 사용 유닛만 Dim 위로 표시
            FocusUltimateCamera(unit);      //궁극기 사용 유닛 쪽으로 카메라 이동 및 확대
        }

        //궁극기 종료
        private void OnUltimateFinished(BattleUnit unit)
        {
            if (unit != highlightedUltimateUnit) return;

            HideUltimateEffect();
        }

        //유닛 사망
        private void OnUnitDied(BattleUnit unit)
        {
            if (unit != highlightedUltimateUnit) return;

            HideUltimateEffect(); //연출 종료
        }

        //궁극기 실행 중 전투 필드를 어둡게 표시
        private void ShowUltimateDim()
        {
            if (ultimateDim == null) return;

            ultimateDim.DOKill();
            ultimateDim.DOFade(dimAlpha, dimDuration); //어둡게
        }

        //궁극기 사용 유닛만 Dim보다 위에 표시
        private void HighlightUltimateUnit(BattleUnit unit)
        {
            ultimateSortingGroup = unit.GetComponent<SortingGroup>(); //BattleUnit에 기존 SortingGroup이 있는지 확인

            if (ultimateSortingGroup == null)
            {
                ultimateSortingGroup = unit.gameObject.AddComponent<SortingGroup>(); //기존 SortingGroup이 없다면 런타임에 임시로 추가
                addedSortingGroup = true;
            }
            else
            {
                originalSortingOrder = ultimateSortingGroup.sortingOrder; // 기존 SortingGroup이 있는 경우 궁극기 종료 후 원래 값으로 돌려주기 위해 현재 Sorting Order 저장
                addedSortingGroup = false;
            }

            ultimateSortingGroup.sortingOrder = ultimateSortingOrder; //궁극기 사용 유닛만 어두워지지 않고 밝게 보이도록 처리
        }

        //궁극기 사용 유닛을 중심으로 카메라 이동 및 확대
        private void FocusUltimateCamera(BattleUnit unit)
        {
            if (battleCamera == null) return;

            battleCamera.DOKill();
            battleCamera.transform.DOKill();

            
            Vector3 unitPosition = unit.transform.position;                                                    //궁극기 사용 유닛의 현재 위치
            Vector3 targetPosition = new Vector3(unitPosition.x + ultimateCameraOffset.x, unitPosition.y + ultimateCameraOffset.y, originalCameraPosition.z); //목표 위치 계산
            Vector3 focusPosition = Vector3.Lerp(originalCameraPosition, targetPosition, cameraFocusStrength); //유닛 위치까지 이동
            focusPosition = ClampCameraPosition(focusPosition);                                                //카메라 화면 위치 제한
            battleCamera.DOOrthoSize(ultimateZoomSize, zoomDuration).SetEase(Ease.OutQuad);                    //유닛쪽으로 카메라 확대
            battleCamera.transform.DOMove(focusPosition, zoomDuration).SetEase(Ease.OutQuad);                  //목표위치까지 카메라 이동
        }

        //카메라 화면이 cameraArea 밖을 보여주지 않도록 위치 제한
        private Vector3 ClampCameraPosition(Vector3 targetPosition)
        {
            if (cameraArea == null || battleCamera == null) return targetPosition;

            Bounds bounds = cameraArea.bounds; //BoxCollider2D 월드 영역

            float halfHeight = ultimateZoomSize;                //카메라 중심에서 화면 위쪽 끝까지의 거리
            float halfWidth = halfHeight * battleCamera.aspect; //화면 비율을 이용해 카메라 중심에서 좌우 끝까지의 거리 계산

            //카메라 중심이 영역 끝까지 이동하면 화면 절반이 바깥으로 나가므로
            //카메라 화면 크기만큼 안쪽으로 이동 가능한 실제 범위를 계산
            float minX = bounds.min.x + halfWidth;
            float maxX = bounds.max.x - halfWidth;
            float minY = bounds.min.y + halfHeight;
            float maxY = bounds.max.y - halfHeight;

            //계산된 범위 안에서만 카메라 중심이 이동하도록 제한
            float x = Mathf.Clamp(targetPosition.x, minX, maxX);
            float y = Mathf.Clamp(targetPosition.y, minY, maxY);

            return new Vector3(x, y, originalCameraPosition.z);
        }

        //궁극기 연출 종료
        private void HideUltimateEffect()
        {
            //궁극기 시작 시 적용했던 연출을 각각 원래 상태로 복구
            HideUltimateDim();
            RestoreUltimateUnit();
            RestoreBattleCamera();

            //현재 강조 중인 유닛 정보 초기화
            highlightedUltimateUnit = null;
        }

        //궁극기 Dim 페이드 아웃
        private void HideUltimateDim()
        {
            if (ultimateDim == null) return;

            ultimateDim.DOKill();
            ultimateDim.DOFade(0f, dimDuration); //페이드 아웃
        }

        //궁극기 사용 유닛의 Sorting 상태 원복
        private void RestoreUltimateUnit()
        {
            if (ultimateSortingGroup == null) return;

            if (addedSortingGroup)
            {
                Destroy(ultimateSortingGroup); //궁극기 연출을 위해 임시로 추가한 SortingGroup이면 제거
            }
            else
            {
                ultimateSortingGroup.sortingOrder = originalSortingOrder; //궁극기 시작 전 저장한 Sorting Order로 복구
            }

            ultimateSortingGroup = null;
            addedSortingGroup = false;
        }

        //카메라를 궁극기 실행 전 상태로 복구
        private void RestoreBattleCamera()
        {
            if (battleCamera == null) return;

            battleCamera.DOKill();
            battleCamera.transform.DOKill();
            battleCamera.DOOrthoSize(originalCameraSize, zoomDuration).SetEase(Ease.OutQuad);
            battleCamera.transform.DOMove(originalCameraPosition, zoomDuration).SetEase(Ease.OutQuad);
        }

        //궁극기 연출 초기화
        private void ResetUltimateEffect()
        {
            if (ultimateDim != null)
            {
                ultimateDim.DOKill();

                Color color = ultimateDim.color;
                color.a = 0f;
                ultimateDim.color = color;
            }

            if (battleCamera != null)
            {
                battleCamera.DOKill();
                battleCamera.transform.DOKill();

                battleCamera.orthographicSize = originalCameraSize;
                battleCamera.transform.position = originalCameraPosition;
            }

            
            RestoreUltimateUnit(); //궁극기 유닛의 Sorting 상태 복구
            highlightedUltimateUnit = null;
        }

        #endregion

        #region 전투 보상 UI

        //스테이지 승리 후 보상 UI 갱신
        public void UpdateRewardUI(StageInfo stageInfo)
        {
            if (stageInfo == null) return;

            //골드 보상
            bool hasGold = stageInfo.ClearGold > 0;
            rewardGoldPanel.SetActive(hasGold);
            if (hasGold) rewardGoldText.text = $"+ {stageInfo.ClearGold}";

            //다이아 보상
            bool hasDia = stageInfo.ClearDia > 0;
            rewardDiaPanel.SetActive(hasDia);
            if (hasDia) rewardDiaText.text = $"+ {stageInfo.ClearDia}";

            //무료뽑기권 보상
            bool hasTicket = stageInfo.ClearTicket > 0;
            rewardTicketPanel.SetActive(hasTicket);
            if (hasTicket) rewardTicketText.text = $"+ {stageInfo.ClearTicket}";

            //이번 스테이지에서 장비가 드롭된 경우에만 장비 보상 UI 표시
            if (rewardEquipPanel != null)
            {
                rewardEquipPanel.SetActive(isEquipDroppedThisStage);
            }

            // 다음 판을 위해 다시 초기화
            isEquipDroppedThisStage = false;
        }

        //장비 드롭 시 호출
        public void ShowDroppedEquipmentUI(EquipmentData equip)
        {
            if (equip == null || rewardEquipPanel == null) return;

            isEquipDroppedThisStage = true; // 장비 당첨 기록
            rewardEquipPanel.SetActive(true);

            if (rewardEquipText != null)
            {
                rewardEquipText.text = $"+ {equip.equipmentName}";
            }
        }

        #endregion
    }
}