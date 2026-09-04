using AFKHero.Collection;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class UICollectionManager : MonoBehaviour
    {
        public static UICollectionManager Instance { get; private set; }

        [Header("도감 패널")]
        [SerializeField] private GameObject collectionPanel;
        [SerializeField] private GameObject backButton;

        [Header("수집 현황")]
        [SerializeField] private TMP_Text collectionStatusText;

        [Header("영웅 슬롯")]
        [SerializeField] private Transform content;
        [SerializeField] private UICollectionHeroSlot collectionHeroPrefab;

        [Header("필터 셀렉터")]
        [SerializeField] private RectTransform selector;
        [SerializeField] private float selectorMoveDuration = 0.2f;

        [Header("필터 버튼 텍스트")]
        [SerializeField] private TMP_Text allText;
        [SerializeField] private TMP_Text warriorText;
        [SerializeField] private TMP_Text tankText;
        [SerializeField] private TMP_Text mageText;
        [SerializeField] private TMP_Text archerText;
        [SerializeField] private TMP_Text healerText;

        private readonly Color clickedTextColor = new Color32(74, 40, 26, 255); // #4A281A
        private readonly Color unclickedTextColor = new Color32(114, 87, 71, 255); // #725747

        [Header("전체 수집 보상")]
        [SerializeField] private Button reward4Button;
        [SerializeField] private Button reward8Button;
        [SerializeField] private Button reward12Button;
        [SerializeField] private Button reward16Button;
        [SerializeField] private Button reward24Button;
        [SerializeField] private Button reward32Button;
        [SerializeField] private GameObject rewardGetImagePrefab; //보상 수령 체크 이미지

        [Header("보상 획득 연출")]
        [SerializeField] private RectTransform rewardEffectRoot; //최상위 Canvas 아래 연출 생성 위치
        [SerializeField] private GameObject diaImage;            //날아갈 다이아 이미지
        [SerializeField] private RectTransform diaTarget;        //상단 다이아 UI 위치
        [SerializeField] private float diaSpreadDistance = 60f;
        [SerializeField] private float diaSpreadDuration = 0.2f;
        [SerializeField] private float diaMoveDuration = 0.45f;

        private readonly List<UICollectionHeroSlot> heroSlots = new List<UICollectionHeroSlot>();
        private JobType? currentJobType = null;

        //셀렉터 위치
        private const float ALL_POS_X = -272f;
        private const float WARRIOR_POS_X = -167f;
        private const float TANK_POS_X = -57f;
        private const float MAGE_POS_X = 50f;
        private const float ARCHER_POS_X = 157f;
        private const float HEALER_POS_X = 268f;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            CreateHeroSlots();
            SyncCollectedHeroes();

            SetJobFilter(null, ALL_POS_X);
            RefreshUI();

            collectionPanel.SetActive(false);
            backButton.SetActive(false);
        }

        #region 패널

        //도감 열기
        public void OnClickedOpenCollection()
        {
            //메인 탭이 열려있다면 먼저 닫기
            if (UIManager.Instance != null && UIManager.Instance.TryCloseMainTab())
            {
                return;
            }

            GuideManager.Instance?.PauseGuide();

            SyncCollectedHeroes();

            collectionPanel.SetActive(true);
            backButton.SetActive(true);

            RefreshUI();
        }

        //도감 닫기
        public void OnClickedCloseCollection()
        {
            collectionPanel.SetActive(false);
            backButton.SetActive(false);

            GuideManager.Instance?.ResumeGuide();
        }

        #endregion

        #region 버튼

        //전체
        public void OnClickedAll()
        {
            SetJobFilter(null, ALL_POS_X);
            ChangeFilterTextColor(allText);
        }

        //전사
        public void OnClickedWarrior()
        {
            SetJobFilter(JobType.Warrior, WARRIOR_POS_X);
            ChangeFilterTextColor(warriorText);
        }

        //탱커
        public void OnClickedTank()
        {
            SetJobFilter(JobType.Tank, TANK_POS_X);
            ChangeFilterTextColor(tankText);
        }

        //마법사
        public void OnClickedMage()
        {
            SetJobFilter(JobType.Mage, MAGE_POS_X);
            ChangeFilterTextColor(mageText);
        }

        //궁수
        public void OnClickedArcher()
        {
            SetJobFilter(JobType.Archer, ARCHER_POS_X);
            ChangeFilterTextColor(archerText);
        }

        //힐러
        public void OnClickedHealer()
        {
            SetJobFilter(JobType.Healer, HEALER_POS_X);
            ChangeFilterTextColor(healerText);
        }

        //수집 보상
        public void OnClickedReward(int requiredCount)
        {
            SyncCollectedHeroes();

            if (!CollectionManager.Instance.CanClaimReward(requiredCount))
                return;

            Button rewardButton = GetRewardButton(requiredCount);

            CollectionManager.Instance.ClaimReward(requiredCount);

            if (rewardButton != null)
            {
                PlayDiaRewardEffect(rewardButton.transform.position);
            }

            RefreshUI();
        }

        #endregion

        #region 영웅 카드

        //전체 영웅 카드 최초 생성
        private void CreateHeroSlots()
        {
            heroSlots.Clear();

            foreach (HeroData heroData in HeroManager.Instance.AllHeroDataList)
            {
                UICollectionHeroSlot heroSlot = Instantiate(collectionHeroPrefab, content);
                heroSlot.SetSlot(heroData);
                heroSlots.Add(heroSlot);
            }
        }

        //HeroManager의 획득 상태를 도감 데이터에 동기화
        private void SyncCollectedHeroes()
        {
            foreach (HeroData heroData in HeroManager.Instance.AllHeroDataList)
            {
                HeroInstance hero = HeroManager.Instance.GetHeroByID(heroData.HeroID);

                if (hero != null && hero.isUnlocked)
                    CollectionManager.Instance.RegisterHero(heroData.HeroID);
            }
        }

        //직업 필터 변경
        private void SetJobFilter(JobType? jobType, float selectorPosX)
        {
            currentJobType = jobType;

            FilterHeroSlots();
            MoveSelector(selectorPosX);
        }

        //직업별 영웅 카드 필터링
        private void FilterHeroSlots()
        {
            foreach (UICollectionHeroSlot heroSlot in heroSlots)
            {
                bool isVisible = !currentJobType.HasValue || heroSlot.JobType == currentJobType.Value;
                heroSlot.gameObject.SetActive(isVisible);
            }
        }

        #endregion

        #region 셀렉터

        //선택된 직업 버튼 위치로 셀렉터 이동
        private void MoveSelector(float targetX)
        {
            selector.DOKill();
            selector.DOAnchorPosX(targetX, selectorMoveDuration).SetEase(Ease.OutQuad);
        }

        //필터 버튼 글씨 색상 변경
        private void ChangeFilterTextColor(TMP_Text selectedText)
        {
            allText.color = unclickedTextColor;
            warriorText.color = unclickedTextColor;
            tankText.color = unclickedTextColor;
            mageText.color = unclickedTextColor;
            archerText.color = unclickedTextColor;
            healerText.color = unclickedTextColor;

            selectedText.color = clickedTextColor;
        }

        #endregion

        #region UI 갱신

        //도감 UI 전체 갱신
        private void RefreshUI()
        {
            RefreshCollectionStatus();
            RefreshHeroSlots();
            RefreshRewardButtons();
        }

        //수집 현황 갱신
        private void RefreshCollectionStatus()
        {
            int collectedCount = 0;

            foreach (HeroData heroData in HeroManager.Instance.AllHeroDataList)
            {
                HeroInstance hero = HeroManager.Instance.GetHeroByID(heroData.HeroID);

                if (hero != null && hero.isUnlocked)
                    collectedCount++;
            }

            collectionStatusText.text = $"{collectedCount:00} / 32";
        }

        //영웅 카드 획득 상태 갱신
        private void RefreshHeroSlots()
        {
            foreach (UICollectionHeroSlot heroSlot in heroSlots)
                heroSlot.RefreshSlot();
        }

        //전체 수집 보상 버튼 갱신
        private void RefreshRewardButtons()
        {
            RefreshRewardButton(reward4Button, 4);
            RefreshRewardButton(reward8Button, 8);
            RefreshRewardButton(reward12Button, 12);
            RefreshRewardButton(reward16Button, 16);
            RefreshRewardButton(reward24Button, 24);
            RefreshRewardButton(reward32Button, 32);
        }

        //수집 보상 버튼 상태 갱신
        private void RefreshRewardButton(Button rewardButton, int requiredCount)
        {
            bool isClaimed = CollectionManager.Instance.IsRewardClaimed(requiredCount);

            if (isClaimed)
            {
                rewardButton.interactable = false;
                CreateRewardGetImage(rewardButton);
                return;
            }

            rewardButton.interactable = CollectionManager.Instance.CanClaimReward(requiredCount);
        }

        //보상 수령 이미지 생성
        private void CreateRewardGetImage(Button rewardButton)
        {
            Transform existingImage = rewardButton.transform.Find("reward_get_img");

            if (existingImage != null) return;

            GameObject rewardGetImage = Instantiate(rewardGetImagePrefab, rewardButton.transform);
            rewardGetImage.name = "reward_get_img";

            RectTransform rectTransform = rewardGetImage.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        //수집 개수에 해당하는 보상 버튼 반환
        private Button GetRewardButton(int requiredCount)
        {
            switch (requiredCount)
            {
                case 4:
                    return reward4Button;

                case 8:
                    return reward8Button;

                case 12:
                    return reward12Button;

                case 16:
                    return reward16Button;

                case 24:
                    return reward24Button;

                case 32:
                    return reward32Button;
            }

            return null;
        }

        #region 보상연출

        //보상 상자에서 다이아가 퍼진 후 상단 다이아 UI로 이동
        private void PlayDiaRewardEffect(Vector3 startPosition)
        {
            if (diaImage == null || rewardEffectRoot == null || diaTarget == null)
            {
                return;
            }

            int diaCount = Random.Range(3, 6);

            Vector2 startLocalPosition = rewardEffectRoot.InverseTransformPoint(startPosition);
            Vector2 targetLocalPosition = rewardEffectRoot.InverseTransformPoint(diaTarget.position);

            for (int i = 0; i < diaCount; i++)
            {
                GameObject diaEffect = Instantiate(diaImage, rewardEffectRoot);
                diaEffect.SetActive(true);

                RectTransform diaRect = diaEffect.GetComponent<RectTransform>();

                if (diaRect == null)
                {
                    Destroy(diaEffect);
                    continue;
                }

                diaRect.anchoredPosition = startLocalPosition;
                diaRect.localScale = Vector3.one;

                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                float randomDistance = Random.Range(30f, 60f);

                Vector2 spreadPosition = startLocalPosition + randomDirection * randomDistance;

                bool isLastDia = i == diaCount - 1;

                Sequence sequence = DOTween.Sequence();
                sequence.SetUpdate(true);

                sequence.Append(diaRect.DOAnchorPos(spreadPosition, diaSpreadDuration).SetEase(Ease.OutQuad));
                sequence.AppendInterval(0.05f + i * 0.03f);
                sequence.Append(diaRect.DOAnchorPos(targetLocalPosition, diaMoveDuration).SetEase(Ease.InQuad));
                sequence.Join(diaRect.DOScale(0.3f, diaMoveDuration).SetEase(Ease.InQuad));

                sequence.OnComplete(() =>
                {
                    if (isLastDia)
                    {
                        diaTarget.DOKill();

                        diaTarget.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0.5f).SetUpdate(true);
                    }

                    Destroy(diaEffect);
                });
            }
        }

        #endregion
        #endregion
    }
}