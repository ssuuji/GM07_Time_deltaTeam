using System;
using AFKHero.Battle;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Poolable))]
public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public float speed = 15f; // 투사체 비행 속도

    //private HeroBase target;
    //private int damage;
    private BattleUnit owner;
    private BattleUnit target;
    private Poolable poolable;

    // 기본 공격과 궁극기 투사체를 구분하기 위한 변수
    private float ultimateSpeedMultiplier = 1.5f;

    [Header("투사체 레이어")]
    [SerializeField] private int normalProjectileSortingOrder = 2000;

    private Action<BattleUnit, Vector3> ultimateHitCallback;
    private SortingGroup sortingGroup;
    private int defaultSortingLayerId;
    private int defaultSortingOrder;
    private float currentSpeed;

    // 궁극기 사용할 때 일반 투사체들도 멈추게 하기 위한 변수
    private BattleManager battleManager;
    private bool isUltimateProjectile;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();

        sortingGroup = GetComponent<SortingGroup>();

        if (sortingGroup != null)
        {
            defaultSortingLayerId = sortingGroup.sortingLayerID;
            defaultSortingOrder = sortingGroup.sortingOrder;
        }

        currentSpeed = speed;
    }

    // 투사체 발사 시 초기화
    public void Init(BattleUnit projectileOwner, BattleUnit projectileTarget)
    {
        owner = projectileOwner;
        target = projectileTarget;

        ultimateHitCallback = null;
        currentSpeed = speed;

        // 일반 투사체는 유닛보다 앞에 표시
        ApplyNormalSorting(projectileOwner);

        battleManager =
        projectileOwner != null
            ? projectileOwner.BattleManager
            : null;

        isUltimateProjectile = false;
    }

    public void InitUltimate(
    BattleUnit projectileOwner,
    BattleUnit projectileTarget,
    Action<BattleUnit, Vector3> onHit)
    {
        owner = projectileOwner;
        target = projectileTarget;
        ultimateHitCallback = onHit;

        // 궁극기 종료 전에 도착할 수 있도록 기본 투사체보다 빠르게 이동
        currentSpeed = Mathf.Max(speed, speed * ultimateSpeedMultiplier);

        // 궁극기 투사체는 궁극기 중에도 계속 이동
        battleManager =
            projectileOwner != null
           ? projectileOwner.BattleManager
           : null;

        isUltimateProjectile = true;

        ApplyUltimateSorting(projectileOwner);
    }

    private void Update()
    {
        // 타겟이 없거나 죽었다면 즉시 풀링 창고로 회수함
        //if (target == null || target.stats.currentHP <= 0)
        //{
        //    poolable.Release();
        //    return;
        //}
        if (!IsLivingUnit(owner) || !IsLivingUnit(target))
        {
            ReleaseProjectile();

            return;
        }

        // 궁극기 중에는 기본 투사체 이동 멈춤
        if (ShouldPauseDuringUltimate())
        {
            return;
        }

        // 타겟을 향해 이동함
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * currentSpeed * Time.deltaTime;

        // 타겟을 바라보도록 회전 처리
        //transform.LookAt(target.transform);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 타겟에 도달했는지 거리 검사
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance <= 0.5f)
        {
            HitTarget();
        }
    }

    // 타겟 명중 처리
    private void HitTarget()
    {
        //target.stats.TakeDamage(damage);

        //// 타격을 마친 투사체는 창고로 회수함
        //poolable.Release();


        if (!IsLivingUnit(owner) ||
            !IsLivingUnit(target) ||
            target.Health == null)
        {
            ReleaseProjectile();

            return;
        }

        if (ultimateHitCallback != null)
        {
            BattleUnit hitTarget = target;
            Vector3 hitPosition = target.transform.position;
            Action<BattleUnit, Vector3> callback = ultimateHitCallback;

            // 콜백 실행 중 이 투사체가 다시 사용되어도 기존 상태가 섞이지 않도록 먼저 반환
            ReleaseProjectile();
            callback.Invoke(hitTarget, hitPosition);

            return;
        }

        int finalDamage =
           DamageCalculator.CalculateBasicAttackDamage(
               owner.Stats,
               target.Stats);

        int appliedDamage =
            target.Health.TakeDamage(
                finalDamage,
                owner);

        if (appliedDamage > 0)
        {
            owner.Energy?.GainFromBasicAttack();
        }

        ReleaseProjectile();
    }

    private static bool IsLivingUnit(
        BattleUnit unit)
    {
        return unit != null &&
               unit.IsInitialized &&
               unit.Stats != null &&
               unit.Stats.IsAlive;
    }

    private bool ShouldPauseDuringUltimate()
    {
        return !isUltimateProjectile &&
               battleManager != null &&
               battleManager.IsUltimatePlaying;
    }

    // 일반 투사체를 유닛보다 앞에 표시합니다.
    private void ApplyNormalSorting(BattleUnit projectileOwner)
    {
        if (sortingGroup == null)
        {
            return;
        }

        SortingGroup ownerSortingGroup = FindOwnerSortingGroup(projectileOwner);

        // 투사체와 시전자가 서로 다른 Sorting Layer를 사용하면
        // Sorting Order 비교가 적용되지 않으므로 같은 레이어로 맞춤
        if (ownerSortingGroup != null)
        {
            sortingGroup.sortingLayerID = ownerSortingGroup.sortingLayerID;
        }

        sortingGroup.sortingOrder = normalProjectileSortingOrder;
    }

    // 궁극기 투사체는 암전 배경보다 위에 표시하지만
    // 궁극기를 사용하는 시전자보다는 한 단계 아래에 표시
    private void ApplyUltimateSorting(BattleUnit projectileOwner)
    {
        if (sortingGroup == null)
        {
            return;
        }

        SortingGroup ownerSortingGroup = FindOwnerSortingGroup(projectileOwner);

        if (ownerSortingGroup == null)
        {
            // 궁극기 강조용 SortingGroup을 찾지 못한 테스트 환경에서는
            // 최소한 일반 유닛보다 앞에 표시되도록 처리
            ApplyNormalSorting(projectileOwner);
            return;
        }

        sortingGroup.sortingLayerID = ownerSortingGroup.sortingLayerID;
        sortingGroup.sortingOrder = ownerSortingGroup.sortingOrder - 1;
    }

    // 실제 게임 씬에서는 궁극기 강조용 SortingGroup이 루트에 추가
    // 테스트 씬처럼 루트에 없는 경우에는 자식에 연결된 SortingGroup을 사용
    private static SortingGroup FindOwnerSortingGroup(BattleUnit projectileOwner)
    {
        if (projectileOwner == null)
        {
            return null;
        }

        SortingGroup ownerSortingGroup = projectileOwner.GetComponent<SortingGroup>();

        if (ownerSortingGroup != null)
        {
            return ownerSortingGroup;
        }

        return projectileOwner.GetComponentInChildren<SortingGroup>(true);
    }

    private void RestoreDefaultSorting()
    {
        if (sortingGroup == null)
        {
            return;
        }

        sortingGroup.sortingLayerID = defaultSortingLayerId;
        sortingGroup.sortingOrder = defaultSortingOrder;
    }

    private void ReleaseProjectile()
    {
        owner = null;
        target = null;

        ultimateHitCallback = null;
        currentSpeed = speed;
        RestoreDefaultSorting();

        battleManager = null;
        isUltimateProjectile = false;

        poolable?.Release();
    }

    private void OnDisable()
    {
        owner = null;
        target = null;

        ultimateHitCallback = null;
        currentSpeed = speed;
        RestoreDefaultSorting();

        battleManager = null;
        isUltimateProjectile = false;
    }
}
