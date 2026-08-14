using Mono.Cecil.Cil;
using UnityEngine;

[CreateAssetMenu(fileName = "NewHeroData", menuName = "Game/Hero/Hero Data")]
public class HeroData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private int heroID;           // 영웅 고유 식별 번호
    [SerializeField] private string heroName;       // 영웅 이름
    [SerializeField] private HeroGrade heroGrade;   // 영웅 등급
    [SerializeField] private RaceType raceType;     // 종족 (인간, 엘프, 오크, 언데드)
    [SerializeField] private JobType jobType;      // 직업 (전사, 탱커, 마법사, 궁수, 힐러)

    [Header("궁극기 및 연출 파티클")]
    [SerializeField] private string ultimateSkillName;    // 궁극기 스킬 명칭
    [SerializeField] private GameObject ultimateEffectPrefab; // 궁극기 시전 시 발동할 이펙트 프리팹
    [SerializeField] private GameObject projectilePrefab;    // 기본공격/투사체 프리팹 (궁수, 마법사 등)

    // 전투 설정용
    [Header("전투 설정")]
    [SerializeField, Min(0f)] private float moveSpeed = 2f;
    [SerializeField, Min(1)] int maxUltimateEnergy = 100;
    [SerializeField, Min(0)] int basicAttackEnergyGain = 10;
    [SerializeField, Min(0)] int damageTakenEnergyGain = 5;

    [Header("UI 및 리소스")]
    [SerializeField] private Sprite heroImage;     // 초상화 프레임 Sprite
    [SerializeField] private GameObject heroPrefab;  // 전투 인게임 생성 프리팹

    // Battle용 프리팹 추가
    [Header("전투 프리팹")]
    [SerializeField] private AFKHero.Battle.BattleUnit battleUnitPrefab;

    [Header("스킬(궁극기) 설정")]
    [Tooltip("체크하면 전투 중 에너지가 찼을 때 궁극기를 사용합니다.")]
    [SerializeField] private bool canUseUltimate = false;

    public bool CanUseUltimate => canUseUltimate;

    // 읽기 전용 캡슐화 프로퍼티
    public int HeroID => heroID;
    public string HeroName => heroName;
    public HeroGrade HeroGrade => heroGrade;
    public RaceType RaceType => raceType;
    public JobType JobType => jobType;
    public Sprite HeroIcon => heroImage;
    public GameObject HeroPrefab => heroPrefab;

    public AFKHero.Battle.BattleUnit BattleUnitPrefab => battleUnitPrefab;

    public string UltimateSkillName => string.IsNullOrEmpty(ultimateSkillName)
        ? GetJobStats().defaultSkillName : ultimateSkillName;
    public GameObject UltimateEffectPrefab => ultimateEffectPrefab;
    public GameObject ProjectilePrefab => projectilePrefab;

    // 전투용 프로퍼티
    public float MoveSpeed => moveSpeed;
    public int MaxUltimateEnergy => maxUltimateEnergy;
    public int BasicAttackEnergyGain => basicAttackEnergyGain;
    public int DamageTakenEnergyGain => damageTakenEnergyGain;

    public TargetPriority TargetRule => GetJobStats().targetType;

    public string JobDescription => GetJobStats().jobDescription;

    private void OnValidate()
    {
        if (!RaceJobData.IsValidRaceJob(raceType, jobType))
        {
            Debug.LogWarning($"[유효성 오류] '{heroName}': " +
                $"{raceType} 종족에 {jobType} 직업은 올바른 조합이 아닙니다.");
        }

        if (string.IsNullOrEmpty(ultimateSkillName))
        {
            ultimateSkillName = GetJobStats().defaultSkillName;
        }
    }

    public JobStats GetJobStats()
    {
    // 변경: 태생 등급도 같이 넘겨서 그에 맞는 스탯을 받아오게 함
        return RaceJobData.GetStatsByJob(jobType, heroGrade);
    }
}