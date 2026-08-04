using System;

[Serializable]
public class JobStats
{
    public int hp;             // 체력
    public int attack;         // 공격력
    public int defense;        // 방어력
    public float attackSpeed;  // 공격속도
    public float attackRange;  // 공격 사거리

    public JobStats(
        int hp,
        int attack,
        int defense,
        float attackSpeed,
        float attackRange)
    {
        this.hp = hp;
        this.attack = attack;
        this.defense = defense;
        this.attackSpeed = attackSpeed;
        this.attackRange = attackRange;
    }
}
