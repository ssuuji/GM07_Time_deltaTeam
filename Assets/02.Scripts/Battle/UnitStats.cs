using System;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UnitStats
    {
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public int AttackPower { get; private set; }
        public int Defense { get; private set; }
        public float AttackRange { get; private set; }
        public float AttackInterval { get; private set; }
        public float MoveSpeed { get; private set; }

        public int MaxUltimateEnergy { get; private set; }
        public int CurrentUltimateEnergy { get; private set; }

        public bool IsAlive => CurrentHealth > 0;

        public UnitStats(HeroInstance source, float bonusHpRate, float bonusAttackRate)
        {
            if(source == null || source.data == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            MaxHealth = Mathf.RoundToInt(source.MaxHP * (1f + Mathf.Max(0f, bonusHpRate)));
            CurrentHealth = MaxHealth;

            AttackPower = Mathf.RoundToInt(source.Attack * (1f + Mathf.Max(0f, bonusAttackRate)));

            Defense = source.Defense;
            AttackRange = source.AttackRange;

            AttackInterval = 1f / Mathf.Max(0.01f, source.AttackSpeed);

            MoveSpeed = source.data.MoveSpeed;

            MaxUltimateEnergy = source.data.MaxUltimateEnergy;

            CurrentUltimateEnergy = 0;
        }

        public int ApplyDamage(int damage)
        {
            if(!IsAlive || damage <= 0)
            {
                return 0;
            }

            int previousHealth = CurrentHealth;

            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            return previousHealth - CurrentHealth;
        }

        public int AddUltimateEnergy(int amount)
        {
            if(!IsAlive || amount <= 0)
            {
                return 0;
            }

            int previousEnergy = CurrentUltimateEnergy;

            CurrentUltimateEnergy = Mathf.Min(MaxUltimateEnergy, CurrentUltimateEnergy + amount);

            return CurrentUltimateEnergy - previousEnergy;
        }

        public void ResetUltimateEnergy()
        {
            CurrentUltimateEnergy = 0;
        }

        public bool TryComsumeUltimateEnergy()
        {
            if(!IsAlive || CurrentUltimateEnergy < MaxUltimateEnergy)
            {
                return false;
            }

            CurrentUltimateEnergy = 0;
            return true;
        }
    }
}