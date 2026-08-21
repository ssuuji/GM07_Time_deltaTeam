using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// 직업마다 가지는 15레벨 효과 및 에픽 등급 각성 스킬
namespace AFKHero.Battle
{
    public static class JobUltimateSkill
    {
        // 레벨 업 시 보정 효과
        private const int EnhancedLevel = 15;

        // 에픽 등급 이상일 때 각성 스킬
        private const HeroGrade BonusGrade = HeroGrade.Epic;

        private const float WarriorLevelBonus = 0.2f;
        private const float TankLevelBonus = 0.05f;
        private const float MageLevelBonus = 0.3f;
        private const float ArcherLevelBonus = 0.5f;
        private const float HealerLevelBonus = 0.3f;

        private const float WarriorStunDuration = 1f;
        private const float MageSilenceDuration = 2f;

        private const float ArcherDefenseIgnoreRate = 0.5f;
        private const float TankBonusHealRate = 0.10f;

        // 레벨 15 이상일 때 직업별 기본 배율
        public static float GetLevelMultiplierBonus(
            BattleUnit caster)
        {
            if (!IsLevelBonusUnlocked(caster) ||
                caster.Data == null)
            {
                return 0f;
            }

            switch (caster.Data.JobType)
            {
                case JobType.Warrior:
                    return WarriorLevelBonus;

                case JobType.Tank:
                    return TankLevelBonus;

                case JobType.Mage:
                    return MageLevelBonus;

                case JobType.Archer:
                    return ArcherLevelBonus;

                case JobType.Healer:
                    return HealerLevelBonus;

                default:
                    return 0f;
            }
        }
        
        // Epic 궁수: 대상 방어력의 50% 무시
        public static float GetArcherDefenseIgnoreRate(
           BattleUnit caster)
        {
            return IsGradeBonusUnlocked(caster)
                ? ArcherDefenseIgnoreRate
                : 0f;
        }

        // Epic 탱커: 대상 최대 체력의 10% 만큼 추가 회복 
        public static float GetTankBonusHealRate(
            BattleUnit caster)
        {
            return IsGradeBonusUnlocked(caster)
                ? TankBonusHealRate
                : 0f;
        }

        // Epic 전사: 적이 스킬에 맞을 시 기절 적용
        public static void ApplyWarriorGradeEffect(
            BattleUnit caster,
            BattleUnit target)
        {
            if (!IsGradeBonusUnlocked(caster) ||
                !IsLivingUnit(target) ||
                target.StatusEffects == null)
            {
                return;
            }

            target.StatusEffects.ApplyStatusEffect(
                StatusEffectType.Stun,
                WarriorStunDuration);
        }

        // Epic 마법사: 적이 스킬에 맞을 시 침묵 적용
        public static void ApplyMageGradeEffect(
            BattleUnit caster,
            BattleUnit target)
        {
            if (!IsGradeBonusUnlocked(caster) ||
                !IsLivingUnit(target) ||
                target.StatusEffects == null)
            {
                return;
            }

            target.StatusEffects.ApplyStatusEffect(
                StatusEffectType.Silence,
                MageSilenceDuration);
        }

        // Epic 힐러: 스킬 사용 시 아군 유닛의 해로운 효과 제거
        public static void ApplyHealerGradeEffect(
            BattleUnit caster,
            BattleUnit target)
        {
            if (!IsGradeBonusUnlocked(caster) ||
                !IsLivingUnit(target) ||
                target.StatusEffects == null)
            {
                return;
            }

            target.StatusEffects.RemoveStatusEffect(
                StatusEffectType.Stun);

            target.StatusEffects.RemoveStatusEffect(
                StatusEffectType.Silence);
        }

        private static bool IsLevelBonusUnlocked(
            BattleUnit caster)
        {
            return caster != null &&
                   caster.HeroInstance != null &&
                   caster.HeroInstance.level >=
                   EnhancedLevel;
        }

        private static bool IsGradeBonusUnlocked(
            BattleUnit caster)
        {
            return caster != null &&
                   caster.HeroInstance != null &&
                   (int)caster.HeroInstance.currentGrade >=
                   (int)BonusGrade;
        }

        private static bool IsLivingUnit(
            BattleUnit unit)
        {
            return unit != null &&
                   unit.IsInitialized &&
                   unit.Stats != null &&
                   unit.Stats.IsAlive;
        }
    }
}
