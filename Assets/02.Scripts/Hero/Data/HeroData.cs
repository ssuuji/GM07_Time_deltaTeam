using UnityEngine;

[CreateAssetMenu(fileName = "NewHeroData", menuName = "Game/Hero/Hero Data")]
public class HeroData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private int heroID;            // 영웅 ID
    [SerializeField] private string heroName;        // 영웅 이름
    [SerializeField] private HeroGrade heroGrade;    // 영웅 등급 (노멀, 레어, 에픽 등)
    [SerializeField] private JobType jobType;       // 직업 유형
    [SerializeField] private RaceType raceType;      // 종족 유형

    [Header("궁극기 및 이펙트 정보")]
    [SerializeField] private string ultimateSkillName;     // 궁극기 이름
    [SerializeField] private GameObject ultimateEffectPrefab; // 궁극기 연출/이펙트 프리팹
    [SerializeField] private GameObject projectilePrefab;    // 원거리 투사체 프리팹 (궁수/마법사용)

    [Header("이미지 및 프리팹")]
    [SerializeField] private Sprite heroImage;      // 영웅 이미지
    [SerializeField] private GameObject heroPrefab;  // 영웅 프리팹

    // 프로퍼티 (외부 참조용)
    public int HeroID => heroID;
    public string HeroName => heroName;
    public HeroGrade HeroGrade => heroGrade;
    public JobType JobType => jobType;
    public RaceType RaceType => raceType;
    public Sprite HeroIcon => heroImage;
    public GameObject HeroPrefab => heroPrefab;

    public string UltimateSkillName => ultimateSkillName;
    public GameObject UltimateEffectPrefab => ultimateEffectPrefab;
    public GameObject ProjectilePrefab => projectilePrefab;

    // ============================
    // 직업별 공통 능력치
    // ============================
    public JobStats GetJobStats()
    {
        switch (jobType)
        {
            // 전사
            case JobType.Warrior:
                return new JobStats(700, 90, 50, 1.0f, 1.5f);

            // 탱커
            case JobType.Tank:
                return new JobStats(1000, 50, 80, 0.8f, 1.5f);

            // 마법사
            case JobType.Mage:
                return new JobStats(450, 130, 25, 0.8f, 6.0f);

            // 궁수
            case JobType.Archer:
                return new JobStats(500, 100, 30, 1.3f, 7.0f);

            // 힐러
            case JobType.Healer:
                return new JobStats(550, 0, 35, 1.0f, 6.0f);

            // 직업이 정상적으로 설정되지 않은 경우
            default:
                Debug.LogError($"{heroName}의 직업 정보가 올바르지 않습니다.");
                return new JobStats(100, 10, 5, 1.0f, 1.5f);
        }
    }
}
