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

        private BattleUnit owner;
        private BattleManager battlemanager;

        public bool IsStunned => HasStatusEffect(StatusEffectType.Stun);

        public bool IsSilenced => HasStatusEffect(StatusEffectType.Silence);

        // 기절 처리 (이동 x, 공격 x)
        public bool CanMove => !IsStunned;
        public bool CanUseBasicAttack => !IsStunned;

        // 기절, 침묵 공통 처리 (궁극기 사용 x)
        public bool CanUseUltimate => !IsStunned && !IsSilenced;

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

            updateTargets.Clear();

            foreach (StatusEffectType type in remainingDurations.Keys)
            {
                updateTargets.Add(type);
            }

            for(int i = 0; i < updateTargets.Count; i++)
            {
                StatusEffectType type = updateTargets[i];

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
            if (owner == null ||
                owner.Stats == null ||
                !owner.Stats.IsAlive ||
                duration <= 0f)
            {
                return false;
            }

            if(remainingDurations.TryGetValue(type, out float currentDuration))
            {
                remainingDurations[type] = Mathf.Max(currentDuration, duration);
            }
            else
            {
                remainingDurations.Add(type, duration);
            }

            // 기절 시 현재 공격 대상 해제
            if(type == StatusEffectType.Stun)
            {
                owner.TargetFinder?.ClearTarget();
            }

            float remainingDuration = remainingDurations[type];

            StatusEffectApplied?.Invoke(owner, type, remainingDuration);

            Debug.Log($"[상태이상 적용] {owner.name} | {type} | " +
                $"{remainingDuration:0.00}초", owner);

            return true;
        }

        // 군중제어 즉시 해제
        public bool RemoveStatusEffect(StatusEffectType type)
        {
            if (!remainingDurations.Remove(type))
            {
                return false;
            }

            StatusEffectRemoved?.Invoke(owner, type);

            if(owner != null)
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