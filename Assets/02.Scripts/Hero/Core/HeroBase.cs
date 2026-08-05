using UnityEngine;

[RequireComponent(typeof(HeroStats))]
public class HeroBase : MonoBehaviour
{
    public HeroInstance heroInstance { get; private set; }

    // 내 전투 스탯 컴포넌트
    public HeroStats stats { get; private set; }

    private void Awake()
    {
        stats = GetComponent<HeroStats>();
    }

    // 영웅이 전투 필드에 처음 소환될 때 호출하는 초기화 함수
    public void Init(HeroInstance instance)
    {
        this.heroInstance = instance;

        // 내 스탯 컴포넌트에게 데이터를 넘겨주며 초기화 세팅
        stats.Init(instance);

        Debug.Log($"[HeroBase] {instance.data.HeroName} 소환 완료! / 체력: {stats.maxHP} / 공격력: {stats.attack}");
    }

    // ========================
    // 전투 액션 로직
    // ========================

    // 기본 공격 실행
    public void NormalAttack(HeroBase target)
    {
        if (target == null || target.stats.currentHP <= 0) return;

        // 근거리면 바로 데미지, 원거리면 투사체 풀링에서 꺼내서 발사
        target.stats.TakeDamage(stats.attack);

        // 공격 시 에너지를 획득한다 (예: 때릴 때마다 20 회복)
        stats.AddEnergy(20);
    }

    // 궁극기 사용
    public void UseUltimate()
    {
        if (!stats.IsReadyUltimate()) return;

        Debug.Log($"[궁극기 발동!] {heroInstance.data.HeroName} - {heroInstance.data.UltimateSkillName}");

        // 직업별 궁극기 로직
        switch (heroInstance.data.JobType)
        {
            case JobType.Healer:
                // 치유의 빛: 자신의 공격력의 200%만큼 살아있는 모든 아군 회복
                int healAmount = stats.attack * 2;

                // 씬에 존재하는 아군들을 찾아서 회복
                HeroBase[] allHeroesInField = FindObjectsByType<HeroBase>(FindObjectsSortMode.None);
                foreach (var hero in allHeroesInField)
                {
                    // 적이 아닌 아군이라면 회복
                    if (hero.CompareTag("Ally"))
                    {
                        hero.stats.Heal(healAmount);

                        // 힐 이펙트 풀링에서 꺼내서 각 영웅 위치에 재생
                        GameObject healEffect = PoolManager.Instance.SpawnFromPool(heroInstance.data.UltimateEffectPrefab, hero.transform.position, Quaternion.identity);
                        healEffect.GetComponent<Poolable>().ReleaseAfter(1.5f);
                    }
                }
                break;

            case JobType.Mage:
                // 메테오 로직 (추후 구현)
                break;

            case JobType.Warrior:
                // 회전 공격 로직 (추후 구현)
                break;

            case JobType.Archer:
                // 집중 저격 로직 (추후 구현)
                break;

            case JobType.Tank:
                // 수호의 방패 로직 (추후 구현)
                break;
        }

        // 궁극기 사용 완료 후 에너지 0으로 초기화
        stats.ResetEnergy();
    }
}