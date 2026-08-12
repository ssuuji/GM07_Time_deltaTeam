using System.Collections.Generic;
using UnityEngine;

// 종족별 직업 배치 및 직업 메커니즘을 총괄 관리하는 클래스
public static class RaceJobData
{
    // 종족별 보유 직업
    public static readonly Dictionary<RaceType, List<JobType>> RaceJobMapping = new Dictionary<RaceType, List<JobType>>()
    {
        { RaceType.Human,  new List<JobType> { JobType.Warrior, JobType.Tank, JobType.Archer, JobType.Mage, JobType.Healer } },
        { RaceType.Elf,    new List<JobType> { JobType.Warrior, JobType.Tank, JobType.Archer, JobType.Mage, JobType.Healer } },
        { RaceType.Orc,    new List<JobType> { JobType.Warrior, JobType.Tank, JobType.Archer, JobType.Mage, JobType.Healer } },
        { RaceType.Undead, new List<JobType> { JobType.Warrior, JobType.Tank, JobType.Archer, JobType.Mage, JobType.Healer } }
    };

    // 매개변수에 HeroGrade grade 추가
    public static JobStats GetStatsByJob(JobType jobType, HeroGrade grade)
    {
        // 공통으로 사용할 변수들 미리 선언
        int hp = 0;
        int attack = 0;
        int defense = 0;
        float attackSpeed = 1.0f;
        float attackRange = 1.5f;
        TargetPriority targetType = TargetPriority.NearestEnemy;
        string skillName = "";
        string jobDesc = "";

        switch (jobType)
        {
            // 전사: 가장 가까운 적 타겟 / 회전 공격
            case JobType.Warrior:
                targetType = TargetPriority.NearestEnemy;
                skillName = "회전 공격";
                jobDesc = "공격과 생존이 균형 잡힌 근접 딜러. 적에게 접근해 지속 피해를 주며 주변의 모든 적을 공격합니다.";
                attackSpeed = 1.0f;
                attackRange = 1.5f;

                // [수정된 부분 3] 태생 등급별로 완전히 다른 기본 스탯 부여
                if (grade == HeroGrade.Normal) { hp = 700; attack = 90; defense = 50; }
                else if (grade == HeroGrade.Rare) { hp = 1200; attack = 150; defense = 80; }
                else if (grade == HeroGrade.Epic) { hp = 2000; attack = 250; defense = 120; }
                break;

            // 탱커: 가장 가까운 적 타겟 / 피해 감쇄 및 보호막
            case JobType.Tank:
                targetType = TargetPriority.NearestEnemy;
                skillName = "수호의 방패";
                jobDesc = "높은 체력과 방어력으로 전열에서 아군을 보호하며, 궁극기로 보호막을 생성합니다.";
                attackSpeed = 0.8f;
                attackRange = 1.5f;

                if (grade == HeroGrade.Normal) { hp = 1000; attack = 50; defense = 80; }
                else if (grade == HeroGrade.Rare) { hp = 1800; attack = 80; defense = 130; }
                else if (grade == HeroGrade.Epic) { hp = 3000; attack = 120; defense = 200; }
                break;

            // 마법사: 가장 가까운 적 타겟 / 메테오(광역)
            case JobType.Mage:
                targetType = TargetPriority.NearestEnemy;
                skillName = "메테오";
                jobDesc = "체력은 낮지만 강력한 범위 마법을 사용하며, 메테오로 범위 내 다수의 적을 몰살합니다.";
                attackSpeed = 0.8f;
                attackRange = 6.0f;

                if (grade == HeroGrade.Normal) { hp = 450; attack = 130; defense = 25; }
                else if (grade == HeroGrade.Rare) { hp = 700; attack = 220; defense = 40; }
                else if (grade == HeroGrade.Epic) { hp = 1100; attack = 350; defense = 60; }
                break;

            // 궁수: 후방 적 우선 타겟 / 저격(원거리 폭딜)
            case JobType.Archer:
                targetType = TargetPriority.BacklineEnemy;
                skillName = "집중 저격";
                jobDesc = "후열의 적을 우선 타겟팅하여 높은 단일 피해로 치명타를 입히는 원거리 딜러입니다.";
                attackSpeed = 1.3f;
                attackRange = 7.0f;

                if (grade == HeroGrade.Normal) { hp = 500; attack = 100; defense = 30; }
                else if (grade == HeroGrade.Rare) { hp = 800; attack = 180; defense = 50; }
                else if (grade == HeroGrade.Epic) { hp = 1200; attack = 280; defense = 80; }
                break;

            // 힐러: 가장 가까운 적을 평타로 공격하며 에너지를 모은 뒤 / 전체 회복
            case JobType.Healer:
                targetType = TargetPriority.NearestEnemy;
                skillName = "치유의 빛";
                jobDesc = "공격 대신 체력이 가장 낮은 아군을 우선 회복시키며 아군의 생존력을 높입니다.";
                attackSpeed = 1.0f;
                attackRange = 6.0f;

                if (grade == HeroGrade.Normal) { hp = 550; attack = 40; defense = 35; }
                else if (grade == HeroGrade.Rare) { hp = 900; attack = 70; defense = 55; }
                else if (grade == HeroGrade.Epic) { hp = 1400; attack = 100; defense = 90; }
                break;

            default:
                Debug.LogError($"[RaceJobData] 직업 데이터 불일치: {jobType}");
                return new JobStats(100, 10, 5, 1.0f, 1.5f, TargetPriority.NearestEnemy, "기본 스킬", "설명 없음");
        }

        // 최종적으로 세팅된 값들을 모아서 JobStats 객체로 반환
        return new JobStats(hp, attack, defense, attackSpeed, attackRange, targetType, skillName, jobDesc);
    }

    // 종족-직업 조합 유효성 검사
    public static bool IsValidRaceJob(RaceType race, JobType job)
    {
        if (RaceJobMapping.TryGetValue(race, out var jobs))
        {
            return jobs.Contains(job);
        }
        return false;
    }
}