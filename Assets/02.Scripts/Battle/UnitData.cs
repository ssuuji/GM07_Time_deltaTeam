using UnityEngine;
using UnityEngine.Rendering;

namespace AFKHero.Battle
{
    [CreateAssetMenu(
        fileName = "UnitData_",
        menuName = "Battle/Unit Data")]
    public sealed class UnitData : ScriptableObject
    {
        // 중복 방지용 고유 ID
        [Header("Identity")]
        [SerializeField] private string unitId;
        // 이름
        [SerializeField] private string displayName;
        // 초상화
        [SerializeField] private Sprite potrait;

        [Header("Prefab")]
        [SerializeField] private BattleUnit battlePrefab;

        [Header("Base Stats")]
        // 체력
        [Min(1)]
        [SerializeField] private int maxHealth = 100;
        // 공격력
        [Min(0)]
        [SerializeField] private int attackPower = 10;
        // 방어력
        [Min(0)]
        [SerializeField] private int defense = 2;
        // 사거리
        [Min(0.1f)]
        [SerializeField] private float attackRange = 1.2f;
        // 공격 쿨타임
        [Min(0.1f)]
        [SerializeField] private float attackInterval = 1f;
        // 이동 속도
        [Min(0f)]
        [SerializeField] private float moveSpeed = 2f;

        [Header("궁극기 에너지")]
        // 궁극기 에너지
        [Min(1)]
        [SerializeField] private int maxUltimateEnergy = 100;
        // 기본 공격 시 공격자가 얻는 에너지
        [Min(0)]
        [SerializeField] private int basicAttackEnergyGain = 10;
        // 피해를 받았을 때 방어자가 얻는 에너지
        [Min(0)]
        [SerializeField] private int damageTakenEnergyGain = 5;

        public string UnitId=> unitId;
        public string DisplayName => displayName;
        public Sprite Potrait => potrait;
        public BattleUnit BattlePrefab => battlePrefab;

        public int MaxHealth => maxHealth;
        public int AttackPower => attackPower;
        public int Defense => defense;
        public float AttackRange => attackRange;
        public float AttackInterval => attackInterval;
        public float MoveSpeed => moveSpeed;
        public int MaxUltimateEnergy => maxUltimateEnergy;
        public int BasicAttackEnergyGain => basicAttackEnergyGain;
        public int DamageTakenEnergyGain => damageTakenEnergyGain;

#if UNITY_EDITOR
        // 데이터 누락 확인
        private void OnValidate()
        {
            // ID확인
            if (string.IsNullOrWhiteSpace(unitId))
            {
                Debug.LogWarning($"[{name}] Unit ID가 비어있습니다.", this);
            }

            // 전투 프리팹 확인
            if(battlePrefab == null)
            {
                Debug.LogWarning($"[{name}] Battle Prefab이 비어있습니다.", this);
            }
        }
#endif
    }
        
}
