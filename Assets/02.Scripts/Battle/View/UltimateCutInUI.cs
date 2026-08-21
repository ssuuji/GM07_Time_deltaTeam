using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.Battle
{
    public sealed class UltimateCutInUI : MonoBehaviour
    {
        [Header("매니저")]
        [SerializeField] private BattleManager battleManager;

        [Header("컷인 UI")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image heroPortrait;
        [SerializeField] private TMP_Text heroNameText;
        [SerializeField] private TMP_Text skillNameText;

        [Header("컷인 시간")]
        [SerializeField, Min(0.1f)] private float cutInDuration = 0.8f;

        private Coroutine hideRoutine;

        private void Awake()
        {
            if(canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            SetCutInVisible(false);
        }

        private void OnEnable()
        {
            if(battleManager == null)
            {
                battleManager = FindObjectOfType<BattleManager>();
            }

            if(battleManager != null)
            {
                battleManager.UltimateStarted += HandleUltimateStarted;
            }
        }

        private void OnDisable()
        {
            if(battleManager != null)
            {
                battleManager.UltimateStarted -= HandleUltimateStarted;
            }

            if(hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
                hideRoutine = null;
            }
        }

        // 궁쓴 유닛의 데이터를 UI에 표시하고 HeroData에 등록된 이펙트를 생성
        private void HandleUltimateStarted(BattleUnit unit)
        {
            if(unit == null || unit.Data == null)
            {
                return;
            }

            if(heroPortrait != null)
            {
                heroPortrait.sprite = unit.Data.HeroIcon;
            }

            if(heroNameText != null)
            {
                heroNameText.text = unit.Data.HeroName;
            }

            if(skillNameText != null)
            {
                skillNameText.text = unit.Data.UltimateSkillName;
            }

            SetCutInVisible(true);

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }

            hideRoutine = StartCoroutine(HideCutInRoutine());
        }

        private IEnumerator HideCutInRoutine()
        {
            yield return new WaitForSecondsRealtime(cutInDuration);

            SetCutInVisible(false);
            hideRoutine = null;
        }

        private void SetCutInVisible(bool isVisible)
        {
            if(canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = isVisible ? 1f : 0f;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}