using UnityEngine;
using System;

public class HeroStats : MonoBehaviour
{
    [Header("전투 실시간 스탯")]
    public int currentHP;
    public int maxHP;
    public int attack;
    public int defense;
    public float attackSpeed;
    public float attackRange;

    [Header("궁극기 에너지")]
    public int currentEnergy;
    public const int MAX_ENERGY = 100; // 에너지가 100이 되면 궁극기 사용

    public Action<int, int> OnHpChanged;
    public Action<int, int> OnEnergyChanged;
    public Action OnDeath;

    public void Init(HeroInstance heroInstance)
    {
        // 파티 시너지가 발려있는 Final 스탯
        maxHP = heroInstance.FinalMaxHP;
        currentHP = maxHP;

        attack = heroInstance.FinalAttack;
        defense = heroInstance.Defense;
        attackSpeed = heroInstance.AttackSpeed;
        attackRange = heroInstance.AttackRange;

        currentEnergy = 0; // 시작 시 에너지는 0
    }

    // ===============================
    // 전투 관련 핵심 로직
    // ===============================

    // 피격 처리
    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        // 아주 간단한 데미지 공식: (적의 공격력 - 내 방어력). 최소 1의 피해는 받도록 설정
        int finalDamage = Mathf.Max(1, damage - defense);
        currentHP -= finalDamage;

        OnHpChanged?.Invoke(currentHP, maxHP);

        // 피격 시 에너지를 획득 (예: 맞을 때마다 10 회복)
        AddEnergy(10);

        // 사망 처리
        if (currentHP <= 0)
        {
            currentHP = 0;
            OnDeath?.Invoke();
        }
    }
    //힐러의 궁극기를 위한 회복 로직
    public void Heal(int amount)
    {
        // 이미 죽은 영웅은 회복 불가
        if (currentHP <= 0) return;

        currentHP += amount;

        // 최대 체력을 넘지 않도록 제한
        if (currentHP > maxHP) currentHP = maxHP;

        OnHpChanged?.Invoke(currentHP, maxHP);
    }

    // 에너지 충전 공격할 때, 피격당할 때
    public void AddEnergy(int amount)
    {
        if (currentEnergy >= MAX_ENERGY) return;

        currentEnergy += amount;
        if (currentEnergy > MAX_ENERGY) currentEnergy = MAX_ENERGY;

        OnEnergyChanged?.Invoke(currentEnergy, MAX_ENERGY); // 에너지바 업데이트 신호
    }

    // 궁극기 사용 가능 여부 확인
    public bool IsReadyUltimate()
    {
        return currentEnergy >= MAX_ENERGY;
    }

    // 궁극기 사용 후 에너지 초기화
    public void ResetEnergy()
    {
        currentEnergy = 0;
        OnEnergyChanged?.Invoke(currentEnergy, MAX_ENERGY);
    }
}