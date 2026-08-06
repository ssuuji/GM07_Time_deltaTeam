using UnityEngine;

[RequireComponent(typeof(Poolable))]
public class Projectile : MonoBehaviour
{
    [Header("투사체 설정")]
    public float speed = 15f; // 투사체 비행 속도

    private HeroBase target;
    private int damage;
    private Poolable poolable;

    private void Awake()
    {
        poolable = GetComponent<Poolable>();
    }

    // 투사체 발사 시 초기화
    public void Init(HeroBase target, int damage)
    {
        this.target = target;
        this.damage = damage;
    }

    private void Update()
    {
        // 타겟이 없거나 죽었다면 즉시 풀링 창고로 회수함
        if (target == null || target.stats.currentHP <= 0)
        {
            poolable.Release();
            return;
        }

        // 타겟을 향해 이동함
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // 타겟을 바라보도록 회전 처리
        transform.LookAt(target.transform);

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
        target.stats.TakeDamage(damage);

        // 타격을 마친 투사체는 창고로 회수함
        poolable.Release();
    }
}