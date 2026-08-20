using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// 직업마다 가지는 궁극기 스킬
namespace AFKHero.Battle
{
    public static class JobUltimateSkill
    {
        private const int EnhancedLevel = 15;
        private const HeroGrade BonusGrade = HeroGrade.Epic;

        private const float WarriorBaseMultiplier = 1.4f;
        private const float WarriorEnhancedMultiplier = 1.7f;
        private const float WarriorStunDuration = 1f;

        private const float TankBaseShieldRate = 0.15f;
        private const float TankEnhancedShieldRate = 0.2f;
        private const float TankBonusHealRate = 0.1f;

        private const float MageBaseMultiplier = 1.7f;
        private const float MageEnhancedMultiplier = 2f;
        private const float MageSilenceDuration = 2f;

        private const float ArcherBaseMultiplier = 2.5f;
        private const float ArcherEnhancedMultiplier = 3f;
        private const float ArcherDefenseIgnoreRate = 0.5f;

        private const float HealerBaseMultiplier = 1.5f;
        private const float HealerEnhancedMultiplier = 1.8f;

        public static bool TryExecute(BattleUnit caster, BattleManager battleManager)
        {
            if (!IsLivingUnit(caster) ||
                caster.Data == null ||
                caster.HeroInstance == null ||
                battleManager == null)
            {
                return false;
            }

            switch (caster.Data.JobType)
            {
                case JobType.Warrior:
                    return ExecuteWarriorUltimate(caster, battleManager);

                case JobType.Tank:
                    return ExecuteTankUltimate(caster, battleManager);

                case JobType.Mage:
                    return ExecuteMageUltimate(caster, battleManager);

                case JobType.Archer:
                    return ExecuteArcherUltimate(caster, battleManager);

                case JobType.Healer:
                    return ExecuteHealerUltimate(caster, battleManager);

                default:
                    Debug.LogWarning($"[{caster.name}] 지원하지 않는 직업입니다: {caster.Data.JobType}", caster);
                    return false;
            }
        }

        // 회전공격
        // 모든 적에게 피해를 주고 Epic 이상이면 적을 기절시킴
        private static bool ExecuteWarriorUltimate(BattleUnit caster, BattleManager battleManager)
        {
            IReadOnlyList<BattleUnit> opponents = battleManager.GetOpponents(caster.Team);

            float multiplier =
                IsLevelBonusUnlocked(caster) ? WarriorEnhancedMultiplier : WarriorBaseMultiplier;

            bool executed = false;

            for (int i = 0; i < opponents.Count; i++)
            {
                BattleUnit target = opponents[i];

                if (!IsLivingUnit(target))
                {
                    continue;
                }

                int appliedDamage =
                    ApplyUltimateDamage(
                        caster,
                        target,
                        multiplier);

                if (appliedDamage <= 0)
                {
                    continue;
                }

                executed = true;

                if (IsGradeBonusUnlocked(caster) &&
                    IsLivingUnit(target))
                {
                    target.StatusEffects?.ApplyStatusEffect(StatusEffectType.Stun, WarriorStunDuration);
                }
            }
            return executed;
        }

        // 수호의 방패
        // 모든 살아있는 아군에게 보호막 부여 Epic 이상이면 아군이 최대 체력을 기준으로 추가 회복
        private static bool ExecuteTankUltimate(BattleUnit caster, BattleManager battleManager)
        {
            IReadOnlyList<BattleUnit> teamMembers = GetTeamMembers(caster, battleManager);

            float shieldRate =
                IsLevelBonusUnlocked(caster) ? TankEnhancedShieldRate : TankBaseShieldRate;

            int shieldAmount = Mathf.Max(1, Mathf.RoundToInt(caster.Stats.MaxHealth * shieldRate));

            bool hasLivingTarget = false;

            for (int i = 0; i < teamMembers.Count; i++)
            {
                BattleUnit target = teamMembers[i];

                if (!IsLivingUnit(target) || target.Health == null)
                {
                    continue;
                }

                hasLivingTarget = true;

                target.Health.AddShieldFromUltimate(shieldAmount, caster);

                if (IsGradeBonusUnlocked(caster))
                {
                    int healingAmount = Mathf.Max(1, Mathf.RoundToInt(target.Stats.MaxHealth * TankBonusHealRate));

                    target.Health.RestoreHealthFromUltimate(
                        healingAmount,
                        caster);
                }
            }
            return hasLivingTarget;
        }

        // 메테오
        // 모든 적에게 피해를 주고 Epic 이상이면 살아남은 적을 침묵시킵니다.
        private static bool ExecuteMageUltimate(BattleUnit caster, BattleManager battleManager)
        {
            IReadOnlyList<BattleUnit> opponents = battleManager.GetOpponents(caster.Team);

            float multiplier = IsLevelBonusUnlocked(caster) ? MageEnhancedMultiplier : MageBaseMultiplier;

            bool executed = false;

            for (int i = 0; i < opponents.Count; i++)
            {
                BattleUnit target = opponents[i];

                if (!IsLivingUnit(target))
                {
                    continue;
                }

                int appliedDamage = ApplyUltimateDamage(caster, target, multiplier);

                if (appliedDamage <= 0)
                {
                    continue;
                }

                executed = true;

                if (IsGradeBonusUnlocked(caster) && IsLivingUnit(target))
                {
                    target.StatusEffects?.ApplyStatusEffect(StatusEffectType.Silence, MageSilenceDuration);
                }
            }

            return executed;
        }

        // 집중 저격
        // 현재 체력이 가장 낮은 적 한 명에게 강한 피해를 줍니다. Epic 이상이면 대상 방어력의 50%를 무시합니다.
        private static bool ExecuteArcherUltimate(
            BattleUnit caster,
            BattleManager battleManager)
        {
            BattleUnit target = FindLowestHealthOpponent(caster, battleManager);

            if (target == null)
            {
                return false;
            }

            float multiplier = IsLevelBonusUnlocked(caster) ? ArcherEnhancedMultiplier : ArcherBaseMultiplier;

            float defenseIgnoreRate = IsGradeBonusUnlocked(caster) ? ArcherDefenseIgnoreRate : 0f;

            int appliedDamage = ApplyUltimateDamage(caster, target, multiplier, defenseIgnoreRate);

            return appliedDamage > 0;
        }

        // 치유의 빛
        // 시전자의 공격력을 기준으로 모든 살아 있는 아군을 회복 Epic 이상이면 기절과 침묵을 함께 해제
        private static bool ExecuteHealerUltimate(BattleUnit caster, BattleManager battleManager)
        {
            IReadOnlyList<BattleUnit> teamMembers = GetTeamMembers(caster, battleManager);

            float multiplier = IsLevelBonusUnlocked(caster) ? HealerEnhancedMultiplier : HealerBaseMultiplier;

            int healingAmount = DamageCalculator.CalculateUltimateHealing(caster.Stats, multiplier);

            bool hasLivingTarget = false;

            for (int i = 0; i < teamMembers.Count; i++)
            {
                BattleUnit target = teamMembers[i];

                if (!IsLivingUnit(target) || target.Health == null)
                {
                    continue;
                }

                hasLivingTarget = true;

                target.Health.RestoreHealthFromUltimate(healingAmount, caster);

                if (IsGradeBonusUnlocked(caster) &&
                    target.StatusEffects != null)
                {
                    target.StatusEffects.RemoveStatusEffect(StatusEffectType.Stun);

                    target.StatusEffects.RemoveStatusEffect(StatusEffectType.Silence);
                }
            }
            return hasLivingTarget;
        }

        // 궁극기 데미지 계산
        private static int ApplyUltimateDamage(
            BattleUnit caster,
            BattleUnit target,
            float attackMultiplier,
            float defenseIgnoreRate = 0f)
        {
            if (!IsLivingUnit(caster) || !IsLivingUnit(target) || caster.UltimateController == null)
            {
                return 0;
            }

            int finalDamage =
                DamageCalculator.CalculateUltimateDamage(
                    caster.Stats,
                    target.Stats,
                    attackMultiplier,
                    defenseIgnoreRate);

            return caster.UltimateController.ApplyUltimateDamage(target, finalDamage);
        }

        // 팀 영웅 가져오기
        private static IReadOnlyList<BattleUnit> GetTeamMembers(BattleUnit caster, BattleManager battleManager)
        {
            return caster.Team == TeamType.Ally ? battleManager.AllyUnits : battleManager.EnemyUnits;
        }

        // 체력이 가장 낮은 적 찾기
        private static BattleUnit FindLowestHealthOpponent(BattleUnit caster, BattleManager battleManager)
        {
            IReadOnlyList<BattleUnit> opponents = battleManager.GetOpponents(caster.Team);

            BattleUnit selectedTarget = null;

            for (int i = 0; i < opponents.Count; i++)
            {
                BattleUnit candidate = opponents[i];

                if (!IsLivingUnit(candidate))
                {
                    continue;
                }

                if (selectedTarget == null ||
                    candidate.Stats.CurrentHealth <
                    selectedTarget.Stats.CurrentHealth)
                {
                    selectedTarget = candidate;
                    continue;
                }

                // 현재 체력이 같다면 앞쪽 슬롯의 적을 먼저 선택하여
                // 실행할 때마다 대상이 달라지는 것을 방지
                if (candidate.Stats.CurrentHealth == selectedTarget.Stats.CurrentHealth &&
                    candidate.FormationSlotIndex <
                        selectedTarget.FormationSlotIndex)
                {
                    selectedTarget = candidate;
                }
            }

            return selectedTarget;
        }

        // 레벨 20이상이면 추가 스킬 해제
        private static bool IsLevelBonusUnlocked(
            BattleUnit caster)
        {
            return caster.HeroInstance != null && caster.HeroInstance.level >= EnhancedLevel;
        }

        // 영웅 등급 Epic 이상이면 스킬 해제
        private static bool IsGradeBonusUnlocked(
            BattleUnit caster)
        {
            return caster.HeroInstance != null && caster.HeroInstance.currentGrade >= BonusGrade;
        }

        // 살아있는 유닛 검색
        private static bool IsLivingUnit(BattleUnit unit)
        {
            return unit != null &&
                   unit.IsInitialized &&
                   unit.Stats != null &&
                   unit.Stats.IsAlive;
        }


    }
}
