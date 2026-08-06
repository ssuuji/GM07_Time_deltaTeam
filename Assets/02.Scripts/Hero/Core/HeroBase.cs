using UnityEngine;

public class HeroBase : MonoBehaviour
{
    // 영웅 데이터 인스턴스
    public HeroInstance heroInstance { get; private set; }

    // 만들 전투 스탯 클래스를 연동용으로 선언만
    public HeroStats stats { get; private set; }

    private void Awake()
    {
        stats = GetComponent<HeroStats>();
    }

    // 영웅 소환 시 초기화
    public void Init(HeroInstance instance)
    {
        this.heroInstance = instance;
        Debug.Log($"[HeroBase] {instance.data.HeroName} 궁극기 시스템 세팅 완료!");
    }


    // =========================
    // 투사체 풀링
    // =========================

    // 원거리 영웅 기본 공격을 구현할 때 호출할 수 있는 투사체 생성 함수
    public void SpawnProjectile(HeroBase target)
    {
        if (heroInstance.data.JobType == JobType.Archer || heroInstance.data.JobType == JobType.Mage)
        {
            if (heroInstance.data.ProjectilePrefab != null)
            {
                GameObject spawnedObj = PoolManager.Instance.SpawnFromPool
                    (heroInstance.data.ProjectilePrefab, transform.position, Quaternion.identity);

                Projectile projScript = spawnedObj.GetComponent<Projectile>();
                if (projScript != null)
                {
                    // 타겟과 데미지 정보를 정상적으로 전달함
                    projScript.Init(target, heroInstance.FinalAttack);
                }
            }
        }
    }

    // ===============================
    // 궁극기 로직 관리
    // ===============================

    public void ExecuteUltimateEffect()
    {
        Debug.Log($"[궁극기 효과 발동!] {heroInstance.data.HeroName} - {heroInstance.data.UltimateSkillName}");

        // 필드 내 모든 영웅 탐색
        HeroBase[] allHeroesInField = FindObjectsByType<HeroBase>(FindObjectsSortMode.None);

        // 계산해둔 시너지 적용 최종 스탯을 가져와 데미지를 계산함
        int finalAttack = heroInstance.FinalAttack;
        int finalMaxHp = heroInstance.FinalMaxHP;

        // 직업별 궁극기 분기
        switch (heroInstance.data.JobType)
        {
            case JobType.Healer:
                // 치유의 빛: 공격력의 200% 광역 힐
                int healAmount = finalAttack * 2;
                foreach (var hero in allHeroesInField)
                {
                    if (hero.CompareTag("Ally"))
                    {
                        hero.stats.Heal(healAmount); // 회복 함수 호출

                        // 힐 이펙트 풀링 및 해제
                        GameObject healEffect = PoolManager.Instance.SpawnFromPool
                            (heroInstance.data.UltimateEffectPrefab, hero.transform.position, Quaternion.identity);
                        healEffect.GetComponent<Poolable>().ReleaseAfter(1.5f);
                    }
                }
                break;

            case JobType.Warrior:
                // 회전 공격: 반경 3f 내 적에게 공격력 150% 피해
                int warriorDamage = Mathf.RoundToInt(finalAttack * 1.5f);

                // 회전 이펙트 풀링 및 해제
                GameObject spinEffect = PoolManager.Instance.SpawnFromPool
                    (heroInstance.data.UltimateEffectPrefab, transform.position, Quaternion.identity);
                spinEffect.GetComponent<Poolable>().ReleaseAfter(1.0f);

                foreach (var hero in allHeroesInField)
                {
                    if (hero.CompareTag("Enemy"))
                    {
                        float distance = Vector3.Distance(transform.position, hero.transform.position);
                        if (distance <= 3f)
                        {
                            hero.stats.TakeDamage(warriorDamage); //데미지 함수 호출
                        }
                    }
                }
                break;

            case JobType.Mage:
                // 메테오: 가장 가까운 적 주변 4f에 공격력 200% 피해
                int mageDamage = finalAttack * 2;
                HeroBase mageTarget = GetNearestEnemy(allHeroesInField);

                if (mageTarget != null)
                {
                    GameObject meteorEffect = PoolManager.Instance.SpawnFromPool
                        (heroInstance.data.UltimateEffectPrefab, mageTarget.transform.position, Quaternion.identity);
                    meteorEffect.GetComponent<Poolable>().ReleaseAfter(2.0f);

                    foreach (var hero in allHeroesInField)
                    {
                        if (hero.CompareTag("Enemy"))
                        {
                            float distToMeteor = Vector3.Distance(mageTarget.transform.position, hero.transform.position);
                            if (distToMeteor <= 4f)
                            {
                                hero.stats.TakeDamage(mageDamage);
                            }
                        }
                    }
                }
                break;

            case JobType.Archer:
                // 집중 저격: 가장 멀리 있는 적에게 공격력 300% 단일 피해
                int archerDamage = finalAttack * 3;
                HeroBase archerTarget = GetFurthestEnemy(allHeroesInField);

                if (archerTarget != null)
                {
                    GameObject snipeEffect = PoolManager.Instance.SpawnFromPool
                        (heroInstance.data.UltimateEffectPrefab, archerTarget.transform.position, Quaternion.identity);
                    snipeEffect.GetComponent<Poolable>().ReleaseAfter(1.0f);

                    archerTarget.stats.TakeDamage(archerDamage);
                }
                break;

            case JobType.Tank:
                // 수호의 방패: 최대 체력 20% 보호막 생성
                int shieldAmount = Mathf.RoundToInt(finalMaxHp * 0.2f);
                GameObject shieldEffect = PoolManager.Instance.SpawnFromPool
                    (heroInstance.data.UltimateEffectPrefab, transform.position, Quaternion.identity);
                shieldEffect.GetComponent<Poolable>().ReleaseAfter(2.0f);

                stats.AddShield(shieldAmount);
                break;
        }

    }

    // ===============================
    // 타겟 탐색 로직
    // ===============================

    // 마법사용
    private HeroBase GetNearestEnemy(HeroBase[] allHeroes)
    {
        HeroBase nearest = null;
        float minDist = float.MaxValue;
        foreach (var h in allHeroes)
        {
            if (h.CompareTag("Enemy")) 
            {
                float dist = Vector3.Distance(transform.position, h.transform.position);
                if (dist < minDist) { minDist = dist; nearest = h; }
            }
        }
        return nearest;
    }

    // 궁수용
    private HeroBase GetFurthestEnemy(HeroBase[] allHeroes)
    {
        HeroBase furthest = null;
        float maxDist = float.MinValue;
        foreach (var h in allHeroes)
        {
            if (h.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(transform.position, h.transform.position);
                if (dist > maxDist) { maxDist = dist; furthest = h; }
            }
        }
        return furthest;
    }
}