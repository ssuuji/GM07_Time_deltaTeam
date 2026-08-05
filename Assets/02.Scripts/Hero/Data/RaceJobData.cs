using System.Collections.Generic;
using UnityEngine;

// 종족별 직업 배치 및 직업 메커니즘을 총괄 관리하는 클래스
public static class RaceJobData
{
    // 종족별 보유 직업 (4개 종족 x 3개 직업)
    public static readonly Dictionary<RaceType, List<JobType>> RaceJobMapping = new Dictionary<RaceType, List<JobType>>()
    {
        { RaceType.Human,  new List<JobType> { JobType.Warrior, JobType.Tank, JobType.Archer } },  // 인간: 전사, 탱커, 궁수
        { RaceType.Elf,    new List<JobType> { JobType.Archer, JobType.Healer, JobType.Mage } },   // 엘프: 궁수, 힐러, 마법사
        { RaceType.Orc,    new List<JobType> { JobType.Warrior, JobType.Tank, JobType.Mage } },   // 오크: 전사, 탱커, 마법사
        { RaceType.Undead, new List<JobType> { JobType.Warrior, JobType.Archer, JobType.Healer } } // 언데드: 전사, 궁수, 힐러
    };

    // 직업별 기본 능력치 및 전투 로직(타겟팅, 궁극기) 데이터 반환
    public static JobStats GetStatsByJob(JobType jobType)
    {
        switch (jobType)
        {
            // 전사: 가장 가까운 적 타겟 / 회전 공격
            case JobType.Warrior:
                return new JobStats(
                    700, 90, 50, 1.0f, 1.5f,
                    TargetPriority.NearestEnemy,
                    "회전 공격",
                    "공격과 생존이 균형 잡힌 근접 딜러. 적에게 접근해 지속 피해를 주며 주변의 모든 적을 공격합니다."
                );

            // 탱커: 가장 가까운 적 타겟 / 피해 감쇄 및 보호막
            case JobType.Tank:
                return new JobStats(
                    1000, 50, 80, 0.8f, 1.5f,
                    TargetPriority.NearestEnemy,
                    "수호의 방패",
                    "높은 체력과 방어력으로 전열에서 아군을 보호하며, 궁극기로 보호막을 생성합니다."
                );

            // 마법사: 가장 가까운 적 타겟 / 메테오(광역)
            case JobType.Mage:
                return new JobStats(
                    450, 130, 25, 0.8f, 6.0f,
                    TargetPriority.NearestEnemy,
                    "메테오",
                    "체력은 낮지만 강력한 범위 마법을 사용하며, 메테오로 범위 내 다수의 적을 몰살합니다."
                );

            // 궁수: 후방 적 우선 타겟 / 저격(원거리 폭딜)
            case JobType.Archer:
                return new JobStats(
                    500, 100, 30, 1.3f, 7.0f,
                    TargetPriority.BacklineEnemy,
                    "집중 저격",
                    "후열의 적을 우선 타겟팅하여 높은 단일 피해로 치명타를 입히는 원거리 딜러입니다."
                );

            // 힐러: 가장 가까운 적을 평타로 공격하며 에너지를 모은 뒤 / 전체 회복
            case JobType.Healer:
                return new JobStats(
                    550, 40, 35, 1.0f, 6.0f,
                    TargetPriority.NearestEnemy,
                    "치유의 빛",
                    "공격 대신 체력이 가장 낮은 아군을 우선 회복시키며 아군의 생존력을 높입니다."
                );

            default:
                Debug.LogError($"[RaceJobData] 직업 데이터 불일치: {jobType}");
                return new JobStats(100, 10, 5, 1.0f, 1.5f, TargetPriority.NearestEnemy, "기본 스킬", "설명 없음");
        }
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