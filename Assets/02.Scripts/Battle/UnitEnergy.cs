using System;
using UnityEngine;

namespace AFKHero.Battle
{
    public class UnitEnergy : MonoBehaviour
    {
        [Header("디버그 테스트 용")]
        [SerializeField] private bool logEnergyChanges;

        private BattleUnit owner;
        private BattleManager battleManager;

        public int CurrentEnergy => owner != null && owner.Stats != null ? owner.Stats.CurrentUltimateEnergy : 0;
        public int MaxEnergy => owner != null && owner.Stats != null ? owner.Stats.MaxUltimateEnergy : 0;
        public float NormalizedEnergy => MaxEnergy > 0 ? (float)CurrentEnergy / MaxEnergy : 0;
        public bool IsUltimateReady => MaxEnergy > 0  && CurrentEnergy >= MaxEnergy;

        // 유닛, 현재 에너지, 최대 에너지, 전달
        public event Action<BattleUnit, int, int> EnergyChanged;

        // 최대 에너지에 도달하면 발생
        public event Action<BattleUnit> UltimateReady;

        public void Initialize(BattleUnit unitOwner, BattleManager manager)
        {
            if(battleManager != null)
            {
                battleManager.StateChanged -= HandleBattleStateChanged;
            }
            owner = unitOwner;
            battleManager = manager;

            if(owner == null || owner.Stats == null)
            {
                Debug.LogError("BattleUnit 또는 UnitStats가 비어 있습니다.",this);
                return;
            }

            if(battleManager == null)
            {
                Debug.LogError("BattleManager가 비어 있습니다.", this);
                return;
            }

            battleManager.StateChanged += HandleBattleStateChanged;

            ResetEnergy();
        }
        public int GainFromBasicAttack()
        {
            if(owner == null || owner.Data == null)
            {
                return 0;
            }
            return GainEnergy(owner.Data.BasicAttackEnergyGain);
        }

        public int GainFromDamageTake()
        {
            if(owner == null || owner.Data == null)
            {
                return 0;
            }
            return GainEnergy(owner.Data.DamageTakenEnergyGain);
        }
        public int GainEnergy(int amount)
        {
            if(owner == null || owner.Stats == null || 
                !owner.Stats.IsAlive || amount <= 0 || IsUltimateReady)
            {
                return 0;
            }

            bool wasUltimateReady = IsUltimateReady;
            int gainedEnergy = owner.Stats.AddUltimateEnergy(amount);

            if(gainedEnergy <= 0)
            {
                return 0;
            }

            NotifyEnergyChanged();

            if(!wasUltimateReady && IsUltimateReady)
            {
                UltimateReady?.Invoke(owner);
            }

            if (logEnergyChanges)
            {
                Debug.Log($"에너지 획득 - [{owner.name}] +{gainedEnergy} | " +
                    $"[{CurrentEnergy}]/[{MaxEnergy}]",owner);
            }
            return gainedEnergy;
        }

        public bool TryConsumeUltimateEnergy()
        {
            if(owner == null || owner.Stats == null)
            {
                return false;
            }
            if (!owner.Stats.TryComsumeUltimateEnergy())
            {
                return false;
            }

            NotifyEnergyChanged();

            if (logEnergyChanges)
            {
                Debug.Log($"에너지 소비 - [{owner.name}] | [{CurrentEnergy}]/[{MaxEnergy}]", owner);
            }

            return true;
        }

        public void ResetEnergy()
        {
            if(owner == null || owner.Stats == null)
            {
                return;
            }

            owner.Stats.ResetUltimateEnergy();
            NotifyEnergyChanged();
        }

        private void HandleBattleStateChanged(BattleState nextState)
        {
            if(nextState == BattleState.Fighting)
            {
                ResetEnergy();
            }
        }

        private void NotifyEnergyChanged()
        {
            EnergyChanged?.Invoke(owner, CurrentEnergy, MaxEnergy);
        }

        private void OnDestroy()
        {
            if(battleManager != null)
            {
                battleManager.StateChanged -= HandleBattleStateChanged;
            }
        }
    }
}