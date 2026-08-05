using System;

[Serializable]
public class JobStats
{
    public int hp;                     // 기본 체력
    public int attack;                 // 기본 공격력
    public int defense;                // 기본 방어력
    public float attackSpeed;          // 초당 공격 속도
    public float attackRange;          // 공격 사거리

    public TargetPriority targetType;  // 해당 직업의 기본 타겟팅 우선순위
    public string defaultSkillName;    // 기본 궁극기 이름
    public string jobDescription;      // 직업 역할 설명

    public JobStats(
        int hp,
        int attack,
        int defense,
        float attackSpeed,
        float attackRange,
        TargetPriority targetType,
        string defaultSkillName,
        string jobDescription)
    {
        this.hp = hp;
        this.attack = attack;
        this.defense = defense;
        this.attackSpeed = attackSpeed;
        this.attackRange = attackRange;
        this.targetType = targetType;
        this.defaultSkillName = defaultSkillName;
        this.jobDescription = jobDescription;
    }
}