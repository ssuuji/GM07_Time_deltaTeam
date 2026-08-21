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

        // HeroInstance에서 계산된 스텟(레벨,등급,장비,세트,파티 시너지) -> 전투용 스텟으로 복사
        public UnitStats(HeroInstance source)
        {
            if(source == null || source.data == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            MaxHealth = Mathf.Max(
               1,
               source.FinalMaxHP);

            CurrentHealth = MaxHealth;

            AttackPower = Mathf.Max(
                0,
                source.FinalAttack);

            Defense = Mathf.Max(
                0,
                source.FinalDefense);

            AttackRange = Mathf.Max(
                0f,
                source.AttackRange);

            AttackInterval =
                1f / Mathf.Max(
                    0.01f,
                    source.AttackSpeed);

            MoveSpeed = Mathf.Max(
                0f,
                source.data.MoveSpeed);

            MaxUltimateEnergy = Mathf.Max(
                1,
                source.data.MaxUltimateEnergy);

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

        public int RestoreHealth(int amount)
        {
            if (!IsAlive || amount <= 0)
            {
                return 0;
            }

            int previousHealth = CurrentHealth;

            CurrentHealth = Mathf.Min(
                MaxHealth,
                CurrentHealth + amount);

            return CurrentHealth - previousHealth;
        }

        public int Revive(int healthAmount)
        {
            if(IsAlive ||
                healthAmount <= 0 ||
                MaxHealth <= 0)
            {
                return 0;
            }

            CurrentHealth = Mathf.Clamp(
                healthAmount,
                1,
                MaxHealth);

            return CurrentHealth;
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