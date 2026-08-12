using AFKHero.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    //상점 - 소환제단의 레벨과 게이지 표시 UI
    public class UISummonLevel : MonoBehaviour
    {
        [Header("Summon")]
        [SerializeField] private HeroSummonManager heroSummonManager;

        [Header("UI")]
        [SerializeField] private TMP_Text levelText; //제단 레벨
        [SerializeField] private Slider expSlider;   //제단 게이지
        [SerializeField] private TMP_Text expText;   //게이지 텍스트

        private void OnEnable()
        {
            if (heroSummonManager == null) return;

            heroSummonManager.OnSummonInfoChanged += UpdateSummonUI; //제단정보 변경 이벤트 구독
        }

        private void Start()
        {
            UpdateSummonUI(); //제단정보 갱신
        }

        private void OnDisable()
        {
            if (heroSummonManager == null) return;

            heroSummonManager.OnSummonInfoChanged -= UpdateSummonUI; //구독 해제
        }

        //제단정보 갱신
        private void UpdateSummonUI()
        {
            if (heroSummonManager == null) return;

            levelText.text = $"Lv. {heroSummonManager.SummonLevel}";    //레벨
            expSlider.maxValue = heroSummonManager.MaxSummonExp;        //게이지 최대값
            expSlider.value = heroSummonManager.SummonExp;              //게이지 값
            expText.text = $"{expSlider.value} / {expSlider.maxValue}"; //게이지 텍스트
        }

    }
}

