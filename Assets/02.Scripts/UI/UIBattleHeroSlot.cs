using AFKHero.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    //전투화면 하단 영웅 슬롯 UI
    public class UIBattleHeroSlot : MonoBehaviour
    {
        [Header("영웅 정보")]
        [SerializeField] private Image heroIcon;         //영웅 아이콘
        [SerializeField] private TMP_Text heroNameText;  //영웅 이름
        [SerializeField] private Slider hpSlider;        //HP 게이지
        [SerializeField] private Slider ultimateSlider;  //궁극기 게이지

        [Header("등급")]
        [SerializeField] private Image gradeImage;       //둥급 테두리
        [SerializeField] private Sprite normal;          //노멀
        [SerializeField] private Sprite rare;            //레어
        [SerializeField] private Sprite epic;            //에픽

        private BattleUnit battleUnit;                   //전투 유닛

        //영웅 정보 적용 
        public void SetHero(HeroInstance hero)
        {
            if (hero == null || hero.data == null)
            {
                Hide();
                return;
            }

            gameObject.SetActive(true);             

            heroIcon.sprite = hero.data.HeroIcon;    //아이콘
            heroNameText.text = hero.data.HeroName;  //이름
            SetHP(hero.FinalMaxHP, hero.FinalMaxHP); //HP 초기화
            SetUltimate(0f, 1f);                     //궁극기게이지 초기화
            SetHeroGrade(hero.currentGrade);         //등급 테두리 적용
        }

        //유닛 연결
        public void SetBattleUnit(BattleUnit unit)
        {
            ResetBattleUnit();

            battleUnit = unit;
            if (battleUnit == null) return;

            if (battleUnit.Health != null)
            {
                SetHP(unit.Health.CurrentHealth, unit.Health.MaxHealth);
                battleUnit.Health.HealthChanged += OnHealthChanged;
            }

            if (battleUnit.Energy != null)
            {
                SetUltimate(unit.Energy.CurrentEnergy, unit.Energy.MaxEnergy);
                battleUnit.Energy.EnergyChanged += OnEnergyChanged;
            }
        }

        //HP 변경
        private void OnHealthChanged(BattleUnit unit, int currentHealth, int maxHealth)
        {
            Debug.Log($"[BattleHeroSlot] {unit.name} HP UI 갱신 : {currentHealth}/{maxHealth}");
            SetHP(currentHealth, maxHealth);
        }

        //궁극기 게이지 변경
        private void OnEnergyChanged(BattleUnit unit, int currentEnergy, int maxEnergy)
        {
            SetUltimate(currentEnergy, maxEnergy);
        }

        //유닛 이벤트연결 해제
        private void ResetBattleUnit()
        {
            if (battleUnit == null) return;
            if (battleUnit.Health != null) battleUnit.Health.HealthChanged -= OnHealthChanged;
            if (battleUnit.Energy != null) battleUnit.Energy.EnergyChanged -= OnEnergyChanged;
        }

        //등급 테두리 설정
        private void SetHeroGrade(HeroGrade grade)
        {
            switch (grade)
            {
                case HeroGrade.Normal:
                case HeroGrade.NormalPlus:
                    gradeImage.sprite = normal; //노멀
                    break;

                case HeroGrade.Rare:
                case HeroGrade.RarePlus:
                    gradeImage.sprite = rare;   //레어
                    break;

                case HeroGrade.Epic:
                case HeroGrade.EpicPlus:
                    gradeImage.sprite = epic;   //에픽
                    break;
            }
        }

        //HP 갱신
        public void SetHP(float currentHP, float maxHP)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        //궁극기 게이지 갱신
        public void SetUltimate(float currentEnergy, float maxEnergy)
        {
            ultimateSlider.maxValue = maxEnergy;
            ultimateSlider.value = currentEnergy;
        }

        //빈자리 표시
        public void Hide()
        {
            ResetBattleUnit();
            battleUnit = null;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            ResetBattleUnit();
        }
    }
}

