using System;
using System.Collections.Generic;
using AFKHero.Battle;
using UnityEngine;
using UnityEngine.Rendering;

public class HeroBase : MonoBehaviour
{
    [Header("몬스터 전용 데이터")]
    public HeroData defaultEnemyData;

    // 영웅 데이터 인스턴스
    public HeroInstance heroInstance { get; private set; }

    public BattleUnit BattleUnit => battleUnit;

    public bool isEnemy { get; private set; }
    public bool isNormalMob { get; private set; }
    private string targetTag; // 피아식별용 태그

    private BattleUnit battleUnit;

    private void Awake()
    {
        battleUnit = GetComponent<BattleUnit>();
    }

    // 영웅 소환 시 초기화
    public void Init(HeroInstance instance, bool isEnemyTeam = false, bool isNormalMob = false)
    {
        if (instance == null ||
            instance.data == null)
        {
            return;
        }

        if (battleUnit == null)
        {
            battleUnit =
                GetComponent<BattleUnit>();
        }



        heroInstance = instance;
        isEnemy = isEnemyTeam;
        this.isNormalMob = isNormalMob;

        // 적군/아군 여부에 따라 본인의 Tag와 목표물의 targetTag를 자동 할당함
        gameObject.tag = isEnemyTeam ? "Enemy" : "Ally";
        //targetTag = isEnemyTeam ? "Ally" : "Enemy";

        Debug.Log($"[HeroBase] {instance.data.HeroName} 세팅 완료! (적군: {isEnemyTeam}, 일반몹: {isNormalMob})");
    }


    // =========================
    // 투사체 풀링
    // =========================

    // 원거리 영웅 기본 공격을 구현할 때 호출할 수 있는 투사체 생성 함수
    public bool SpawnProjectile(HeroBase target)
    {
        if (!IsLivingHero(this) ||
            !IsLivingHero(target) ||
            heroInstance.data.ProjectilePrefab == null ||
            PoolManager.Instance == null)
        {
            return false;
        }

        JobType jobType = heroInstance.data.JobType;

        if (jobType != JobType.Archer &&
            jobType != JobType.Mage &&
            jobType != JobType.Healer)
        {
            return false;
        }

        GameObject projectileObject =
            PoolManager.Instance.SpawnFromPool(
                heroInstance.data.ProjectilePrefab,
                transform.position,
                Quaternion.identity);

        Projectile projectile =
            projectileObject.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogError(
                $"[{projectileObject.name}] Projectile 컴포넌트가 없습니다.",
                projectileObject);

            return false;
        }

        projectile.Init(battleUnit, target.battleUnit);

        return true;

        //if (heroInstance.data.JobType == JobType.Archer || heroInstance.data.JobType == JobType.Mage)
        //{
        //    if (heroInstance.data.ProjectilePrefab != null)
        //    {
        //        GameObject spawnedObj = PoolManager.Instance.SpawnFromPool
        //            (heroInstance.data.ProjectilePrefab, transform.position, Quaternion.identity);

        //        Projectile projScript = spawnedObj.GetComponent<Projectile>();
        //        if (projScript != null)
        //        {
        //            // 타겟과 데미지 정보를 정상적으로 전달함
        //            projScript.Init(target, heroInstance.FinalAttack);
        //        }
        //    }
        //}
    }

    private bool SpawnUltimateProjectile(HeroBase target, Action<HeroBase, Vector3> onHit)
    {
        if (!IsLivingHero(this) ||
            !IsLivingHero(target) ||
            onHit == null ||
            heroInstance.data.ProjectilePrefab == null ||
            PoolManager.Instance == null)
        {
            return false;
        }

        GameObject projectileObject =
            PoolManager.Instance.SpawnFromPool(
                heroInstance.data.ProjectilePrefab,
                transform.position,
                Quaternion.identity);

        Projectile projectile =
            projectileObject.GetComponent<Projectile>();

        if (projectile == null)
        {
            Debug.LogError(
                $"[{projectileObject.name}] Projectile 컴포넌트가 없습니다.",
                projectileObject);

            projectileObject.GetComponent<Poolable>()?.Release();

            return false;
        }

        projectile.InitUltimate(
            battleUnit,
            target.battleUnit,
            (hitUnit, hitPosition) =>
            {
                // 발사한 대상과 실제 충돌 대상이 같고 아직 살아 있을 때만 효과를 적용
                if (!IsLivingHero(target) ||
                    hitUnit != target.battleUnit)
                {
                    return;
                }

                onHit.Invoke(target, hitPosition);
            });

        return true;
    }

    // ===============================
    // 궁극기 로직 관리
    // ===============================

    public bool ExecuteUltimateEffect()
    {
        if (!IsLivingHero(this) ||
            heroInstance == null ||
            heroInstance.data == null)
        {
            return false;
        }

        battleUnit.UltimateController?.LogUltimateDebug();

        // 필드 내 모든 영웅 탐색
        HeroBase[] allHeroesInField = FindObjectsByType<HeroBase>(FindObjectsSortMode.None);

        // 계산해둔 시너지 적용 최종 스탯을 가져와 데미지를 계산함
        //int finalAttack = heroInstance.FinalAttack;
        //int finalMaxHp = heroInstance.FinalMaxHP;

        // 직업별 궁극기 분기ㅇ
        switch (heroInstance.data.JobType)
        {
            case JobType.Healer:
                return ExecuteHealerUltimate(
                    allHeroesInField);

            case JobType.Warrior:
                return ExecuteWarriorUltimate(
                    allHeroesInField);

            case JobType.Mage:
                return ExecuteMageUltimate(
                    allHeroesInField);

            case JobType.Archer:
                return ExecuteArcherUltimate(
                    allHeroesInField);

            case JobType.Tank:
                return ExecuteTankUltimate(
                    allHeroesInField);

            default:
                Debug.LogWarning(
                    $"[{name}] 지원하지 않는 직업입니다: " +
                    $"{heroInstance.data.JobType}",
                    this);

                return false;
                // 치유의 빛: 공격력의 200% 광역 힐
                //int healAmount = finalAttack * 2;
                //foreach (var hero in allHeroesInField)
                //{
                //    if (hero.CompareTag("Ally"))
                //    {
                //        hero.stats.Heal(healAmount); // 회복 함수 호출

                //        // 힐 이펙트 풀링 및 해제
                //        if (heroInstance.data.UltimateEffectPrefab != null)
                //        {
                //            GameObject healEffect = PoolManager.Instance.SpawnFromPool
                //                (heroInstance.data.UltimateEffectPrefab, hero.transform.position, Quaternion.identity);
                //            healEffect.GetComponent<Poolable>().ReleaseAfter(1.5f);
                //        }
                //    }
                //}
                //break;

                //// 회전 공격: 반경 3f 내 적에게 공격력 150% 피해
                //int warriorDamage = Mathf.RoundToInt(finalAttack * 1.5f);

                //// 회전 이펙트 풀링 및 해제
                //if (heroInstance.data.UltimateEffectPrefab != null)
                //{
                //    GameObject spinEffect = PoolManager.Instance.SpawnFromPool
                //        (heroInstance.data.UltimateEffectPrefab, transform.position, Quaternion.identity);
                //    spinEffect.GetComponent<Poolable>().ReleaseAfter(1.0f);
                //}

                //foreach (var hero in allHeroesInField)
                //{
                //    if (hero.CompareTag("Enemy"))
                //    {
                //        float distance = Vector3.Distance(transform.position, hero.transform.position);
                //        if (distance <= 3f)
                //        {
                //            hero.stats.TakeDamage(warriorDamage); //데미지 함수 호출
                //        }
                //    }
                //}
                //break;

                //case JobType.Mage:
                //    // 메테오: 가장 가까운 적 주변 4f에 공격력 200% 피해
                //    int mageDamage = finalAttack * 2;
                //    HeroBase mageTarget = GetNearestEnemy(allHeroesInField);

                //    if (mageTarget != null)
                //    {
                //        if (heroInstance.data.UltimateEffectPrefab != null)
                //        {
                //            GameObject meteorEffect = PoolManager.Instance.SpawnFromPool
                //                (heroInstance.data.UltimateEffectPrefab, mageTarget.transform.position, Quaternion.identity);
                //            meteorEffect.GetComponent<Poolable>().ReleaseAfter(2.0f);
                //        }

                //        foreach (var hero in allHeroesInField)
                //        {
                //            if (hero.CompareTag("Enemy"))
                //            {
                //                float distToMeteor = Vector3.Distance(mageTarget.transform.position, hero.transform.position);
                //                if (distToMeteor <= 4f)
                //                {
                //                    hero.stats.TakeDamage(mageDamage);
                //                }
                //            }
                //        }
                //    }
                //    break;

                //case JobType.Archer:
                //    // 집중 저격: 가장 멀리 있는 적에게 공격력 300% 단일 피해
                //    int archerDamage = finalAttack * 3;
                //    HeroBase archerTarget = GetFurthestEnemy(allHeroesInField);

                //    if (archerTarget != null)
                //    {
                //        if (heroInstance.data.UltimateEffectPrefab != null)
                //        {
                //            GameObject snipeEffect = PoolManager.Instance.SpawnFromPool
                //                (heroInstance.data.UltimateEffectPrefab, archerTarget.transform.position, Quaternion.identity);
                //            snipeEffect.GetComponent<Poolable>().ReleaseAfter(1.0f);
                //        }

                //        archerTarget.stats.TakeDamage(archerDamage);
                //    }
                //    break;

                //case JobType.Tank:
                //    // 수호의 방패: 최대 체력 20% 보호막 생성
                //    int shieldAmount = Mathf.RoundToInt(finalMaxHp * 0.2f);
                //    if (heroInstance.data.UltimateEffectPrefab != null)
                //    {
                //        GameObject shieldEffect = PoolManager.Instance.SpawnFromPool
                //            (heroInstance.data.UltimateEffectPrefab, transform.position, Quaternion.identity);
                //        shieldEffect.GetComponent<Poolable>().ReleaseAfter(2.0f);
                //    }

                //    stats.AddShield(shieldAmount);
                //    break;
        }

    }

    // 치유의 빛: 공격력의 200% 광역 힐
    private bool ExecuteHealerUltimate(
        HeroBase[] allHeroes)
    {
        const float baseHealingMultiplier = 2f;

        float finalMultiplier =
            baseHealingMultiplier +
            JobUltimateSkill.GetLevelMultiplierBonus(
                battleUnit);

        int healingAmount =
            DamageCalculator.CalculateUltimateHealing(
                battleUnit.Stats,
                finalMultiplier);

        bool hasLivingTarget = false;

        foreach (HeroBase hero in allHeroes)
        {
            if (!IsLivingHero(hero) ||
                !IsSameTeam(hero))
            {
                continue;
            }

            hasLivingTarget = true;

            hero.battleUnit.Health.RestoreHealthFromUltimate(
                healingAmount,
                battleUnit);

            JobUltimateSkill.ApplyHealerGradeEffect(
                battleUnit,
                hero.battleUnit);

            SpawnUltimateEffect(
                hero.transform.position,
                1.5f);
        }

        return hasLivingTarget;
    }

    // 회전 공격: 반경 3f 내 적에게 공격력 150% 피해
    private bool ExecuteWarriorUltimate(
        HeroBase[] allHeroes)
    {
        const float attackRadius = 1.5f;
        const float baseDamageMultiplier = 1.5f;

        float finalMultiplier =
            baseDamageMultiplier +
            JobUltimateSkill.GetLevelMultiplierBonus(
                battleUnit);

        float attackRadiusSqr =
            attackRadius *
            attackRadius;

        bool appliedAnyDamage = false;

        SpawnUltimateEffect(
            transform.position,
            1f);

        foreach (HeroBase hero in allHeroes)
        {
            if (!IsLivingHero(hero) ||
                !IsOpponent(hero))
            {
                continue;
            }

            float distanceSqr =
                (hero.transform.position -
                 transform.position).sqrMagnitude;

            if (distanceSqr > attackRadiusSqr)
            {
                continue;
            }

            int appliedDamage =
                ApplyUltimateDamage(
                    hero,
                    finalMultiplier);

            if (appliedDamage <= 0)
            {
                continue;
            }

            appliedAnyDamage = true;

            JobUltimateSkill.ApplyWarriorGradeEffect(
                battleUnit,
                hero.battleUnit);
        }

        return appliedAnyDamage;
    }

    // 메테오: 가장 가까운 적 주변 4f에 공격력 200% 피해
    private bool ExecuteMageUltimate(
        HeroBase[] allHeroes)
    {
        const float attackRadius = 1.5f;
        const float baseDamageMultiplier = 2f;

        HeroBase centerTarget = GetNearestOpponent(allHeroes);

        if (centerTarget == null)
        {
            return false;
        }

        float finalMultiplier =
            baseDamageMultiplier +
            JobUltimateSkill.GetLevelMultiplierBonus(battleUnit);

        float attackRadiusSqr = attackRadius * attackRadius;

        return SpawnUltimateProjectile(
            centerTarget,
            (_, hitPosition) =>
            {
                // 투사체가 도착한 순간에 마법 이펙트를 생성
                SpawnUltimateEffect(hitPosition, 2f);

                foreach (HeroBase hero in allHeroes)
                {
                    if (!IsLivingHero(hero) ||
                        !IsOpponent(hero))
                    {
                        continue;
                    }

                    float distanceSqr =
                        (hero.transform.position - hitPosition).sqrMagnitude;

                    if (distanceSqr > attackRadiusSqr)
                    {
                        continue;
                    }

                    int appliedDamage =
                        ApplyUltimateDamage(
                            hero,
                            finalMultiplier);

                    if (appliedDamage <= 0)
                    {
                        continue;
                    }

                    JobUltimateSkill.ApplyMageGradeEffect(
                        battleUnit,
                        hero.battleUnit);
                }
            });
    }

    // 집중 저격: 가장 멀리 있는 적에게 공격력 300% 단일 피해
    private bool ExecuteArcherUltimate(
        HeroBase[] allHeroes)
    {
        const float baseDamageMultiplier = 3f;

        HeroBase target = GetFarthestOpponent(allHeroes);

        if (target == null)
        {
            return false;
        }

        float finalMultiplier =
            baseDamageMultiplier +
            JobUltimateSkill.GetLevelMultiplierBonus(battleUnit);

        float defenseIgnoreRate =
            JobUltimateSkill.GetArcherDefenseIgnoreRate(battleUnit);

        bool projectileLaunched =
            SpawnUltimateProjectile(
                target,
                (hitTarget, _) =>
                {
                    int appliedDamage =
                        ApplyUltimateDamage(
                            hitTarget,
                            finalMultiplier,
                            defenseIgnoreRate);

                    if (appliedDamage <= 0)
                    {
                        return;
                    }

                    JobUltimateSkill.ApplyArcherKnockback(
                        battleUnit,
                        hitTarget.battleUnit);
                });

        if (!projectileLaunched)
        {
            return false;
        }

        // 궁수 스킬 이펙트는 궁극기를 사용하는 궁수 위치에 표시
        SpawnUltimateEffect(transform.position, 1f);

        return true;
    }

    // 수호의 방패: 최대 체력 20% 보호막 생성
    private bool ExecuteTankUltimate(
        HeroBase[] allHeroes)
    {
        const float baseShieldRate = 0.20f;

        const int maximumHealingTargetCount = 2;

        float finalShieldRate =
            baseShieldRate +
            JobUltimateSkill.GetLevelMultiplierBonus(
                battleUnit);

        int shieldAmount =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    battleUnit.Stats.MaxHealth *
                    finalShieldRate));

        battleUnit.Health.AddShieldFromUltimate(
            shieldAmount,
            battleUnit);

        SpawnUltimateEffect(
            transform.position,
            2f);

        foreach (HeroBase hero in allHeroes)
        {
            if (!IsLivingHero(hero) || !IsOpponent(hero))
            {
                continue;
            }

            JobUltimateSkill.ApplyTankTaunt(battleUnit, hero.battleUnit);
        }

        float bonusHealRate =
            JobUltimateSkill.GetTankBonusHealRate(
                battleUnit);

        if (bonusHealRate > 0f)
        {
            List<HeroBase> healingTargets = new();

            foreach (HeroBase hero in allHeroes)
            {
                if (!IsLivingHero(hero) || !IsSameTeam(hero))
                {
                    continue;
                }

                // 이미 최대 체력인 아군은 회복 대상에서 제외
                if (hero.battleUnit.Stats.CurrentHealth >= hero.battleUnit.Stats.MaxHealth)
                {
                    continue;
                }

                healingTargets.Add(hero);
            }

            // 현재 체력 비율이 낮은 아군부터 정렬
            healingTargets.Sort(CompareCurrentHealthRatio);

            int healingTargetCount = Mathf.Min(maximumHealingTargetCount, healingTargets.Count);

            for (int i = 0; i < healingTargetCount; i++)
            {
                HeroBase healingTarget = healingTargets[i];
                int healingAmount = Mathf.Max(1, Mathf.RoundToInt(healingTarget.battleUnit.Stats.MaxHealth * bonusHealRate));

                healingTarget.battleUnit.Health.RestoreHealthFromUltimate(healingAmount, battleUnit);
            }
        }
        return true;
    }

    private static int CompareCurrentHealthRatio(HeroBase left, HeroBase right)
    {
        long leftHealthRatio = (long)left.battleUnit.Stats.CurrentHealth * right.battleUnit.Stats.MaxHealth;
        long rightHealthRatio = (long)right.battleUnit.Stats.CurrentHealth * left.battleUnit.Stats.MaxHealth;

        int ratioComparison = leftHealthRatio.CompareTo(rightHealthRatio);

        if (ratioComparison != 0)
        {
            return ratioComparison;
        }

        // 체력 비율이 같으면 결과가 매번 바뀌지 않도록 편성 슬롯 순서로 결정합니다.
        return left.battleUnit.FormationSlotIndex.CompareTo(right.battleUnit.FormationSlotIndex);
    }


    private int ApplyUltimateDamage(
       HeroBase target,
       float attackMultiplier,
       float defenseIgnoreRate = 0f)
    {
        if (!IsLivingHero(this) ||
            !IsLivingHero(target) ||
            battleUnit.UltimateController == null)
        {
            return 0;
        }

        int finalDamage =
            DamageCalculator.CalculateUltimateDamage(
                battleUnit.Stats,
                target.battleUnit.Stats,
                attackMultiplier,
                defenseIgnoreRate);

        return battleUnit.UltimateController.ApplyUltimateDamage(
            target.battleUnit,
            finalDamage);
    }

    // ===============================
    // 타겟 탐색 로직
    // ===============================

    //// 마법사용
    //private HeroBase GetNearestEnemy(HeroBase[] allHeroes)
    //{
    //    HeroBase nearest = null;
    //    float minDist = float.MaxValue;
    //    foreach (var h in allHeroes)
    //    {
    //        if (h.CompareTag("Enemy")) 
    //        {
    //            float dist = Vector3.Distance(transform.position, h.transform.position);
    //            if (dist < minDist) { minDist = dist; nearest = h; }
    //        }
    //    }
    //    return nearest;
    //}

    //// 궁수용
    //private HeroBase GetFurthestEnemy(HeroBase[] allHeroes)
    //{
    //    HeroBase furthest = null;
    //    float maxDist = float.MinValue;
    //    foreach (var h in allHeroes)
    //    {
    //        if (h.CompareTag("Enemy"))
    //        {
    //            float dist = Vector3.Distance(transform.position, h.transform.position);
    //            if (dist > maxDist) { maxDist = dist; furthest = h; }
    //        }
    //    }
    //    return furthest;
    //}

    // 마법사용 탐색 로직
    private HeroBase GetNearestOpponent(
        HeroBase[] allHeroes)
    {
        HeroBase nearest = null;
        float minimumDistanceSqr = float.MaxValue;

        foreach (HeroBase hero in allHeroes)
        {
            if (!IsLivingHero(hero) ||
                !IsOpponent(hero))
            {
                continue;
            }

            float distanceSqr =
                (hero.transform.position -
                 transform.position).sqrMagnitude;

            if (distanceSqr < minimumDistanceSqr)
            {
                minimumDistanceSqr = distanceSqr;
                nearest = hero;
            }
        }

        return nearest;
    }

    // 궁수용 탐색 로직
    private HeroBase GetFarthestOpponent(
        HeroBase[] allHeroes)
    {
        HeroBase farthest = null;
        float maximumDistanceSqr = float.MinValue;

        foreach (HeroBase hero in allHeroes)
        {
            if (!IsLivingHero(hero) ||
                !IsOpponent(hero))
            {
                continue;
            }

            float distanceSqr =
                (hero.transform.position -
                 transform.position).sqrMagnitude;

            if (distanceSqr > maximumDistanceSqr)
            {
                maximumDistanceSqr = distanceSqr;
                farthest = hero;
            }
        }

        return farthest;
    }

    private bool IsSameTeam(
        HeroBase other)
    {
        return IsLivingHero(other) &&
               other.battleUnit.Team ==
               battleUnit.Team;
    }

    private bool IsOpponent(
        HeroBase other)
    {
        return IsLivingHero(other) &&
               other.battleUnit.Team !=
               battleUnit.Team;
    }

    private static bool IsLivingHero(
        HeroBase hero)
    {
        return hero != null &&
               hero.battleUnit != null &&
               hero.battleUnit.IsInitialized &&
               hero.battleUnit.Stats != null &&
               hero.battleUnit.Stats.IsAlive &&
               hero.battleUnit.Health != null;
    }

    private void ApplyUltimateEffectSorting(GameObject effectObject)
    {
        if (effectObject == null)
        {
            return;
        }

        SortingGroup ownerSortingGroup =
            GetComponent<SortingGroup>();

        SortingGroup effectSortingGroup =
            effectObject.GetComponent<SortingGroup>();

        if (ownerSortingGroup == null ||
            effectSortingGroup == null)
        {
            return;
        }

        effectSortingGroup.sortingLayerID =
            ownerSortingGroup.sortingLayerID;

        effectSortingGroup.sortingOrder =
            ownerSortingGroup.sortingOrder + 1;
    }

    private void SpawnUltimateEffect(
        Vector3 position,
        float lifetime)
    {
        if (heroInstance == null ||
            heroInstance.data == null ||
            heroInstance.data.UltimateEffectPrefab == null ||
            PoolManager.Instance == null)
        {
            return;
        }

        GameObject effectObject =
            PoolManager.Instance.SpawnFromPool(
                heroInstance.data.UltimateEffectPrefab,
                position,
                Quaternion.identity);

        ApplyUltimateEffectSorting(effectObject);

        Poolable poolable =
            effectObject.GetComponent<Poolable>();

        if (poolable != null)
        {
            poolable.ReleaseAfter(
                lifetime);
        }
    }
}