using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class UIHeroInfoPopup : MonoBehaviour
    {
        [SerializeField] private GameObject backButton;
        [SerializeField] private GameObject heroInfoPopup;

        [Header("영웅 정보")]
        [SerializeField] private Image heroImage;               //영웅 이미지 -> 프리펩으로해야되나?
        [SerializeField] private TMP_Text heroNametext;         //영웅 이름
        [SerializeField] private TMP_Text heroGradetext;        //영웅 등급텍스트
        [SerializeField] private Image heroGradeImage;          //영웅 등급이미지(색변화용)

        [Header("스탯")]
        [SerializeField] private TMP_Text heroLevelText;        //레벨
        [SerializeField] private TMP_Text heroAttackText;       //공격력
        [SerializeField] private TMP_Text heroDefenseText;      //방어력

        [Header("궁극기")]
        [SerializeField] private TMP_Text ultimateText;         //궁극기 설명

        [Header("배치 버튼")]
        [SerializeField] private TMP_Text placeButtonText;      //버튼텍스트

        private HeroInstance selectedHero;                      //선택한 영웅

        //정보창 열기
        public void InfoOpen(HeroInstance hero)
        {
            if (hero == null) return;

            selectedHero = hero; //선택한 영웅 저장

            backButton.SetActive(true);
            heroInfoPopup.SetActive(true); //팝업창 활성화

            SetHeroInfo(hero);  //영웅정보
            SetHeroStat(hero);  //스탯
            SetHeroSkill(hero); //스킬
            SetPlaceButton(hero); //배치 버튼 상태
        }

        //영웅정보
        private void SetHeroInfo(HeroInstance hero)
        {
            heroImage.sprite = hero.data.HeroIcon; //일단 아이콘.. 
            heroNametext.text = hero.data.HeroName; //이름

            //등급설정
            heroGradeImage.color = HeroGradeColor.GetColor(hero.data.HeroGrade); //등급 배경색 설정
            switch (hero.data.HeroGrade)
            {
                case HeroGrade.Normal:     heroGradetext.text = "노멀"; break;
                case HeroGrade.NormalPlus: heroGradetext.text = "노멀+"; break;
                case HeroGrade.Rare:       heroGradetext.text = "레어"; break;
                case HeroGrade.RarePlus:   heroGradetext.text = "레어+"; break;
                case HeroGrade.Epic:       heroGradetext.text = "에픽"; break;
                case HeroGrade.EpicPlus:   heroGradetext.text = "에픽+"; break;
            }
        }

        //영웅스탯
        private void SetHeroStat(HeroInstance hero)
        {
            heroLevelText.text = $"LV. {hero.level}";       //레벨
            heroAttackText.text = hero.Attack.ToString();   //공격력
            heroDefenseText.text = hero.Defense.ToString(); //방어력
        }

        //스킬설명
        private void SetHeroSkill(HeroInstance hero)
        {
            //에픽등급 이상만 궁극기 해금
            switch (hero.data.HeroGrade)
            {
                case HeroGrade.Epic:
                case HeroGrade.EpicPlus:
                    ultimateText.text = hero.data.UltimateSkillName; //일단 궁극기이름,,
                    break;

                default:
                    ultimateText.text = "에픽 등급 달성 시 해금";
                    break;
            }
        }

        //정보창 닫기
        public void InfoClose()
        {
            backButton.SetActive(false);
            heroInfoPopup.SetActive(false);
        }

        //배치 버튼 상태
        private void SetPlaceButton(HeroInstance hero)
        {
            if (PartyManager.Instance.IsHeroInParty(hero))
            {
                placeButtonText.text = "배치 해제";
            }
            else
            {
                placeButtonText.text = "배치";
            }
        }

        //배치 버튼
        public void OnClickedPlace()
        {
            if (selectedHero == null) return;

            if (PartyManager.Instance.IsHeroInParty(selectedHero)) //배치 되어있는지 확인
            {
                UIPartyManager.Instance.RemoveHero(selectedHero); //있으면 해제
            }
            //배치되어 있지 않은 영웅
            else
            {
                UIPartyManager.Instance.StartPlaceHero(selectedHero); //없으면 배치시작
            }

            InfoClose();
        }
    }

}

