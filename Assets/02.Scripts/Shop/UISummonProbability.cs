using UnityEngine;
using TMPro;
using AFKHero.Shop;
using UnityEngine.UI; // HeroSummonManager를 가져오기 위해 필요합니다.

namespace AFKHero.UI
{
    public class UISummonProbability : MonoBehaviour
    {
        [Header("확률")]
        [SerializeField] private TMP_Text normalRateText;
        [SerializeField] private TMP_Text rareRateText;
        [SerializeField] private TMP_Text epicRateText;

        [Header("레벨 이동")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private GameObject leftArrow;
        [SerializeField] private GameObject rightArrow;
        private int previewLevel;

        [Header("뒤로가기")]
        [SerializeField] private GameObject backButton;

        private void OnEnable()
        {
            previewLevel = HeroSummonManager.Instance.SummonLevel;

            UpdateProbabilityText();
            backButton.SetActive(true);     
        }

        private void OnDisable()
        {
            backButton.SetActive(false);    
        }

        private void UpdateProbabilityText()
        {
            // 매니저가 아직 안 만들어졌다면 취소
            if (HeroSummonManager.Instance == null) return;

            /*
            // 현재 레벨의 확률 데이터를 가져옵니다.
            SummonLevelData currentData = HeroSummonManager.Instance.GetCurrentLevelData();

            if (currentData != null)
            {
                int currentLevel = HeroSummonManager.Instance.SummonLevel;

                probabilityText.text = $"<size=120%><b>[ LV.{currentLevel} 영웅 소환 확률 ]</b></size>\n\n" +
                                       $"<color=#A0A0A0>노멀 (Normal)</color> : {currentData.normalRate}%\n" +
                                       $"<color=#00BFFF>레어 (Rare)</color> : {currentData.rareRate}%\n" +
                                       $"<color=#BA55D3>에픽 (Epic)</color> : {currentData.epicRate}%";
            }
            */

            SummonLevelData levelData = HeroSummonManager.Instance.GetLevelData(previewLevel);
            if (levelData == null) return;

            levelText.text = $"LV. {previewLevel}";
            normalRateText.text = $"{levelData.normalRate}%";
            rareRateText.text = $"{levelData.rareRate}%";
            epicRateText.text = $"{levelData.epicRate}%";

            leftArrow.SetActive(previewLevel > 1);
            rightArrow.SetActive(previewLevel < HeroSummonManager.Instance.MaxSummonLevel);

        }

        //이전 레벨 확률
        public void OnClickedPreviousLevel()
        {
            if (previewLevel <= 1) return;

            previewLevel--;
            UpdateProbabilityText();
        }

        //다음 레벨 확률
        public void OnClickedNextLevel()
        {
            if (previewLevel >= HeroSummonManager.Instance.MaxSummonLevel) return;

            previewLevel++;
            UpdateProbabilityText();
        }
    }
}