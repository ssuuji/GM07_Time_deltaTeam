using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class UIHeroInfoPopup : MonoBehaviour
    {
        [SerializeField] private GameObject backButton;
        [SerializeField] private GameObject heroInfoPopup;

        [Header("영웅 정보")]
        [SerializeField] private Transform heroPrefab;          //영웅 프리펩
        [SerializeField] private TMP_Text heroNametext;         //영웅 이름
        [SerializeField] private TMP_Text heroGradetext;        //영웅 등급텍스트
        [SerializeField] private Image heroGradeImage;          //영웅 등급이미지(색변화용)

        [Header("스탯")]
        [SerializeField] private TMP_Text heroLevelText;        //레벨
        [SerializeField] private TMP_Text heroAttackText;       //공격력
        [SerializeField] private TMP_Text heroDefenseText;      //방어력

        [Header("궁극기")]
        [SerializeField] private TMP_Text ultimateText;         //궁극기 설명

        [Header("버튼")]
        [SerializeField] private TMP_Text ButtonText;           //버튼텍스트
        private UIHeroSlotMode currentMode;

        private HeroInstance selectedHero;                      //선택한 영웅
        private GameObject currentHeroPrefab;                   //선택된 영웅 프리펩

        //정보창 열기
        public void InfoOpen(HeroInstance hero, UIHeroSlotMode mode)
        {
            if (hero == null) return;

            selectedHero = hero;            //선택한 영웅 저장
            currentMode = mode;             //현재 슬롯 모드 저장(어느탭에서 버튼 눌렀는지 확인용)

            SetHeroInfo(hero);              //영웅정보
            SetHeroStat(hero);              //스탯
            SetHeroSkill(hero);             //스킬
            SetButton(hero);                //버튼

            backButton.SetActive(true);     //뒤로가기버튼 : 배경 클릭
            heroInfoPopup.SetActive(true);  //팝업창 활성화
        }

        //영웅정보
        private void SetHeroInfo(HeroInstance hero)
        {
            SetHeroPrefab(hero); //영웅프리펩 표시

            heroNametext.text = hero.data.HeroName; //이름

            //등급설정
            heroGradeImage.color = HeroGradeColor.GetColor(hero.data.HeroGrade); //등급 배경색 설정
            switch (hero.data.HeroGrade)
            {
                case HeroGrade.Normal:     heroGradetext.text = "노멀";  break;
                case HeroGrade.NormalPlus: heroGradetext.text = "노멀+"; break;
                case HeroGrade.Rare:       heroGradetext.text = "레어";  break;
                case HeroGrade.RarePlus:   heroGradetext.text = "레어+"; break;
                case HeroGrade.Epic:       heroGradetext.text = "에픽";  break;
                case HeroGrade.EpicPlus:   heroGradetext.text = "에픽+"; break;
            }
        }

        private void SetHeroPrefab(HeroInstance hero)
        {
            if (currentHeroPrefab != null) Destroy(currentHeroPrefab); //기존 영웅 제거
            if (hero.data.HeroPrefab == null) return;

            currentHeroPrefab = Instantiate(hero.data.HeroPrefab, heroPrefab); //영웅표시
            currentHeroPrefab.transform.localPosition = Vector3.zero;
            currentHeroPrefab.transform.localRotation = Quaternion.identity;
            currentHeroPrefab.transform.localScale = Vector3.one * 300;

            //UI보다 위에 표시
            SortingGroup sortingGroup = currentHeroPrefab.GetComponentInChildren<SortingGroup>();

            if (sortingGroup != null)
            {
                sortingGroup.sortingLayerName = "UI";
                sortingGroup.sortingOrder = 110;
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
            //레어등급 이상만 궁극기 해금
            switch (hero.data.HeroGrade)
            {
                case HeroGrade.Rare:
                case HeroGrade.RarePlus:
                case HeroGrade.Epic:
                case HeroGrade.EpicPlus:
                    ultimateText.text = hero.data.UltimateSkillName; //일단 궁극기이름,,으로 표시
                    break;

                default:
                    ultimateText.text = "레어 등급 달성 시 해금";
                    break;
            }
        }

        //정보창 닫기
        public void InfoClose()
        {
            backButton.SetActive(false);
            heroInfoPopup.SetActive(false);
        }

        //버튼 상태 (현재 모드에 따른 버튼상태 설정)
        private void SetButton(HeroInstance hero)
        {
            switch (currentMode)
            {
                //파티탭
                case UIHeroSlotMode.Party:
                    if (PartyManager.Instance.IsHeroInParty(hero)) //선택한 영웅이 파티에 배치되어 있다면
                    {
                        ButtonText.text = "배치 해제";
                    }
                    else
                    {
                        ButtonText.text = "배치";
                    }
                    break;

                //공명탭
                case UIHeroSlotMode.Share:
                    ButtonText.text = "레벨업";
                    break;
            }
        }

        //버튼 연결
        public void OnClickedButton()
        {
            if (selectedHero == null) return;

            switch (currentMode)
            {
                case UIHeroSlotMode.Party:
                    PlaceButton();          //배치버튼
                    break;

                case UIHeroSlotMode.Share:
                    //공명 레벨업 기능 (아직 미구현)
                    break;
            }
        }

        //배치 버튼
        public void PlaceButton()
        {
            if (selectedHero == null) return;

            if (PartyManager.Instance.IsHeroInParty(selectedHero))    //배치 되어있는지 확인
            {
                UIPartyManager.Instance.RemoveHero(selectedHero);     //있으면 해제
            }
            else
            {
                UIPartyManager.Instance.StartPlaceHero(selectedHero); //없으면 배치시작
            }

            InfoClose();
        }
    }

}

