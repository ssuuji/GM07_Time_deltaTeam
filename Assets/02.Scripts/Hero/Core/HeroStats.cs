using UnityEngine;

public class HeroStats : MonoBehaviour
{
    private HeroInstance heroInstance;

    public int maxHP;
    public int currentHP;

    // 부활 세트 일회성 보장 변수
    private bool hasRevived = false;

    public void Init(HeroInstance instance)
    {
        this.heroInstance = instance;
        this.maxHP = instance.FinalMaxHP;
        this.currentHP = maxHP;
        this.hasRevived = false;
    }

    // 피격 시 호출
    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        // ----------------------------------
        // [세트 1] EvadeSet (회피) 체크
        // ----------------------------------
        if (heroInstance != null)
        {
            int evadeCount = heroInstance.GetSetCount("EvadeSet");
            float dodgeChance = 0f;

            if (evadeCount >= 4) dodgeChance = 0.30f;   // 4세트: 30% 확률
            else if (evadeCount >= 2) dodgeChance = 0.15f; // 2세트: 15% 확률

            if (dodgeChance > 0f && Random.value < dodgeChance)
            {
                Debug.Log($"<color=green>[회피 성공]</color> {gameObject.name}가 공격을 피했습니다!");
                return;
            }
        }

        // 실제 데미지 계산 (최소 1 피해 보장)
        int finalDamage = Mathf.Max(1, damage - heroInstance.FinalDefense);
        currentHP -= finalDamage;

        // -------------------------------------
        // [세트 2] ReviveSet (부활) 체크
        // -------------------------------------
        if (currentHP <= 0)
        {
            int reviveCount = (heroInstance != null) ? heroInstance.GetSetCount("ReviveSet") : 0;

            if (reviveCount >= 4 && !hasRevived)
            {
                hasRevived = true;
                currentHP = Mathf.RoundToInt(maxHP * 0.40f); // 40% 체력으로 1회 부활 끝
                Debug.Log($"<color=cyan>[부활 발동]</color> {gameObject.name}이 부활했습니다!");
            }
            else
            {
                Die();
            }
        }
    }

    // ------------------------------------
    // [세트 3] VampSet (흡혈) 효과
    // ------------------------------------
    public void OnDealDamage(int damageDealt)
    {
        if (currentHP <= 0 || heroInstance == null) return;

        int vampCount = heroInstance.GetSetCount("VampSet");
        float vampRate = 0f;

        if (vampCount >= 4) vampRate = 0.25f;   // 4세트: 25% 흡혈
        else if (vampCount >= 2) vampRate = 0.10f; // 2세트: 10% 흡혈

        if (vampRate > 0f)
        {
            int healAmount = Mathf.RoundToInt(damageDealt * vampRate);
            Heal(healAmount);
            Debug.Log($"<color=red>[흡혈 발동]</color> {gameObject.name}가 {healAmount}만큼 체력을 회복했습니다.");
        }
    }

    public void Heal(int amount)
    {
        if (currentHP <= 0) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }

    public void AddShield(int amount)
    {
        Debug.Log($"[{gameObject.name}] {amount} 만큼의 보호막 획득");
    }

    private void Die()
    {
        Debug.Log($"[{gameObject.name}] 사망함.");
        gameObject.SetActive(false);
    }
}