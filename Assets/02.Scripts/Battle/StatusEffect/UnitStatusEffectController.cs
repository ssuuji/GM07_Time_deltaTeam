using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitStatusEffectController : MonoBehaviour
    {
        private readonly Dictionary<StatusEffectType, float> remainingDurations = new();

        private readonly List<StatusEffectType> updateTargets = new();

        private const float MinimumKnockbackDuration = 0.05f;

        private const float KnockbackCompletionEpsilon = 0.0001f;

        private BattleUnit tauntSource;

        private Vector2 knockbackDirection;
        private float knockbackSpeed;
        private float remainingKnockbackDistance;

        private BattleUnit owner;
        private BattleManager battlemanager;

        public bool IsStunned => HasStatusEffect(StatusEffectType.Stun);

        public bool IsSilenced => HasStatusEffect(StatusEffectType.Silence);

        public bool IsKnockedBack => HasStatusEffect(StatusEffectType.Knockback);

        public bool IsTaunted => HasStatusEffect(StatusEffectType.Taunt);

        // 기절 처리 (이동 x, 공격 x)
        public bool CanMove => !IsStunned && !IsKnockedBack;
        public bool CanUseBasicAttack => !IsStunned && !IsKnockedBack;

        // 기절, 침묵 공통 처리 (궁극기 사용 x)
        public bool CanUseUltimate => !IsStunned && !IsSilenced && !IsKnockedBack;

        public event Action<BattleUnit, StatusEffectType, float> StatusEffectApplied;
        public event Action<BattleUnit, StatusEffectType> StatusEffectRemoved;

        public void Initialize(BattleUnit unitOwner, BattleManager manager)
        {
            owner = unitOwner;
            battlemanager = manager;

            // 풀링으로 재사용된 유닛에 이전 전투의 상태가 남지않음
            ClearAllStatusEffects();
        }

        private void Update()
        {
            if (!CanUpdateDuration())
            {
                return;
            }

            UpdateKnockbackMovement();

            updateTargets.Clear();

            foreach (StatusEffectType type in remainingDurations.Keys)
            {
                updateTargets.Add(type);
            }

            for(int i = 0; i < updateTargets.Count; i++)
            {
                StatusEffectType type = updateTargets[i];

                if (type == StatusEffectType.Taunt && !IsValidOpponentSource(tauntSource))
                {
                    RemoveStatusEffect(StatusEffectType.Taunt);
                    continue;
                }

                if (!remainingDurations.ContainsKey(type))
                {
                    continue;
                }

                float nextDuration = remainingDurations[type] - Time.deltaTime;

                if(nextDuration <= 0f)
                {
                    RemoveStatusEffect(type);
                    continue;
                }

                remainingDurations[type] = nextDuration;
            }
        }
        
        // 군중제어 효과 적용
        // 같은 효과가 이미 있다면 더 긴 남은 시간을 사용
        public bool ApplyStatusEffect(StatusEffectType type, float duration)
        {
            if (type == StatusEffectType.Knockback || type == StatusEffectType.Taunt)
            {
                return false;
            }

            return ApplyTimedStatusEffect(type,  duration, false);
        }

        public bool ApplyKnockback(BattleUnit source, float distance, float duration)
        {
            if (!CanApplySpecialEffect(source, duration) ||
                distance <= 0f)
            {
                return false;
            }

            float safeDuration =
                Mathf.Max(MinimumKnockbackDuration, duration);

            Vector2 sourcePosition = source.transform.position;

            Vector2 ownerPosition = owner.transform.position;

            Vector2 direction = ownerPosition - sourcePosition;

            // 두 유닛의 위치가 완전히 겹친 경우에는 진영 방향을 기준으로 밀려날 방향을 결정
            if (direction.sqrMagnitude <=
                KnockbackCompletionEpsilon)
            {
                direction = owner.Team == TeamType.Ally ? Vector2.down : Vector2.up;
            }
            else
            {
                direction.Normalize();
            }

            knockbackDirection = direction;
            remainingKnockbackDistance = distance;
            knockbackSpeed = distance / safeDuration;

            bool wasApplied =
                ApplyTimedStatusEffect(
                    StatusEffectType.Knockback,
                    safeDuration,
                    true);

            if (!wasApplied)
            {
                ResetKnockbackMovement();
            }

            return wasApplied;
        }

        // 도발 적용
        public bool ApplyTaunt(BattleUnit source, float duration)
        {
            if (!CanApplySpecialEffect(source, duration))
            {
                return false;
            }

            BattleUnit previousSource =
                tauntSource;

            tauntSource = source;

            bool wasApplied =
                ApplyTimedStatusEffect(
                    StatusEffectType.Taunt,
                    duration,
                    true);

            if (!wasApplied)
            {
                tauntSource = previousSource;
            }

            return wasApplied;
        }

        public bool TryGetTauntSource(out BattleUnit source)
        {
            source = null;

            if (!HasStatusEffect(StatusEffectType.Taunt))
            {
                return false;
            }

            if (!IsValidOpponentSource(tauntSource))
            {
                RemoveStatusEffect(StatusEffectType.Taunt);
                return false;
            }

            source = tauntSource;
            return true;
        }

        private bool ApplyTimedStatusEffect(
            StatusEffectType type,
            float duration,
            bool replaceDuration)
        {
            if (owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive ||
                duration <= 0f)
            {
                return false;
            }

            if (remainingDurations.TryGetValue(
                type,
                out float currentDuration))
            {
                remainingDurations[type] =
                    replaceDuration
                        ? duration
                        : Mathf.Max(
                            currentDuration,
                            duration);
            }
            else
            {
                remainingDurations.Add(
                    type,
                    duration);
            }

            // 행동을 강제로 끊거나 대상을 변경하는 효과는 기존 타깃을 해제해 다음 탐색에서 상태가 반영되게 합니다.
            if (type == StatusEffectType.Stun ||
                type == StatusEffectType.Knockback ||
                type == StatusEffectType.Taunt)
            {
                owner.TargetFinder?.ClearTarget();
            }

            float remainingDuration =
                remainingDurations[type];

            StatusEffectApplied?.Invoke(
                owner,
                type,
                remainingDuration);

            Debug.Log(
                $"[상태이상 적용] {owner.name} | " +
                $"{type} | {remainingDuration:0.00}초",
                owner);

            return true;
        }

        // 군중제어 즉시 해제
        public bool RemoveStatusEffect(StatusEffectType type)
        {
            if (!remainingDurations.Remove(type))
            {
                return false;
            }

            if (type == StatusEffectType.Knockback)
            {
                ResetKnockbackMovement();
            }
            else if (type == StatusEffectType.Taunt)
            {
                tauntSource = null;

                // 도발 종료 후에도 도발 시전자를 계속 공격하지 않도록 기존 타깃을 해제하고 정상 우선순위로 재탐색
                owner?.TargetFinder?.ClearTarget();
            }

            StatusEffectRemoved?.Invoke(owner, type);

            if (owner != null)
            {
                Debug.Log($"[상태이상 해제] {owner.name} | {type}", owner);
            }

            return true;
        }

        // 사망, 전투 끝, 정화 효과 후 모든 군중제어 제거
        public void ClearAllStatusEffects()
        {
            if(remainingDurations.Count == 0)
            {
                tauntSource = null;
                ResetKnockbackMovement();
                return;
            }

            updateTargets.Clear();

            foreach(StatusEffectType type in remainingDurations.Keys)
            {
                updateTargets.Add(type);
            }

            for(int i = 0; i <updateTargets.Count; i++)
            {
                RemoveStatusEffect(updateTargets[i]);
            }

            updateTargets.Clear();
        }

        public bool HasStatusEffect(StatusEffectType type)
        {
            return remainingDurations.ContainsKey(type);
        }

        public float GetRemainingDuration(StatusEffectType type)
        {
            return remainingDurations.TryGetValue(type, out float duration) ? duration : 0f;
        }

        private bool CanApplySpecialEffect(BattleUnit source, float duration)
        {
            if (owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive ||
                battlemanager == null ||
                duration <= 0f)
            {
                return false;
            }

            bool isBattleActive =
                battlemanager.CurrentState == BattleState.Fighting ||
                battlemanager.CurrentState == BattleState.UltimateSequence;

            return isBattleActive && IsValidOpponentSource(source);
        }

        private bool IsValidOpponentSource(BattleUnit source)
        {
            return source != null &&
                   source != owner &&
                   source.IsInitialized &&
                   source.Stats != null &&
                   source.Stats.IsAlive &&
                   source.Team != owner.Team;
        }

        private void UpdateKnockbackMovement()
        {
            if (!IsKnockedBack ||
                remainingKnockbackDistance <= 0f ||
                knockbackSpeed <= 0f)
            {
                return;
            }

            float moveDistance =
                Mathf.Min(
                    remainingKnockbackDistance,
                    knockbackSpeed *
                    Time.deltaTime);

            Vector3 currentPosition = transform.position;

            Vector3 movement =
                new Vector3(
                    knockbackDirection.x,
                    knockbackDirection.y,
                    0f) *
                moveDistance;

            transform.position = currentPosition + movement;

            remainingKnockbackDistance -= moveDistance;

            if (remainingKnockbackDistance <= KnockbackCompletionEpsilon)
            {
                RemoveStatusEffect(StatusEffectType.Knockback);
            }
        }

        private void ResetKnockbackMovement()
        {
            knockbackDirection = Vector2.zero;

            knockbackSpeed = 0f;
            remainingKnockbackDistance = 0f;
        }

        private bool CanUpdateDuration()
        {
            return remainingDurations.Count > 0 &&
                owner != null &&
                owner.Stats != null &&
                owner.Stats.IsAlive &&
                battlemanager != null &&
                battlemanager.CurrentState == BattleState.Fighting;
        }

        private void OnDestroy()
        {
            remainingDurations.Clear();
            updateTargets.Clear();
        }
    }
}