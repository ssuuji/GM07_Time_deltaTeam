using UnityEngine;
using TMPro;
using AFKHero.Shop; // HeroSummonManager를 가져오기 위해 필요합니다.

namespace AFKHero.UI
{
    public class UISummonProbability : MonoBehaviour
    {
        [SerializeField] private TMP_Text probabilityText;

        private void OnEnable()
        {
            UpdateProbabilityText();
        }

        private void UpdateProbabilityText()
        {
            // 매니저가 아직 안 만들어졌다면 취소
            if (HeroSummonManager.Instance == null) return;

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
        }
    }
}