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
        public UnitStats(UnitData source)
        {
            if(source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            MaxHealth= source.MaxHealth;
            CurrentHealth = MaxHealth;

            AttackPower = source.AttackPower;
            Defense = source.Defense;
            
            AttackRange = source.AttackRange;
            AttackInterval = source.AttackInterval;
            MoveSpeed = source.MoveSpeed;

            MaxUltimateEnergy = source.MaxUltimateEnergy;
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
    }
}