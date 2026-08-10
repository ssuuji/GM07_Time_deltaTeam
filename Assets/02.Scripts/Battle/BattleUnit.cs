using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class BattleUnit : MonoBehaviour
    {
        [Header("유닛 탐색")]
        [SerializeField] private UnitTargetFinder targetFinder;
        [Header("유닛 이동 및 공격")]
        [SerializeField] private UnitMovement unitMovemnet;
        [SerializeField] private UnitAttackController attackController;
        [SerializeField] private UnitHealth unitHealth;
        [SerializeField] private UnitEnergy unitEnergy;

        [Header("2D Depth")]
        [SerializeField] private SpriteRenderer[] spriteRenderers;

        // 배경과 겹치지 않게
        [SerializeField] private int sortingBaseOrder = 1000;

        // Sorting Order 미세 조정
        [SerializeField] private int sortingPrecision = 100;

        // 원본 데이터
        public UnitData Data { get; private set; }
        // 능력치
        public UnitStats Stats { get; private set; }
        // 소속 진영
        public TeamType Team { get; private set; }
        // 슬롯 번호
        public int FormationSlotIndex { get; private set; }
        // 초기화 됐는지
        public bool IsInitialized { get; private set; }


        public UnitTargetFinder TargetFinder => targetFinder;
        public UnitMovement Movement => unitMovemnet;
        public UnitAttackController AttackController => attackController;
        public UnitHealth Health => unitHealth;
        public UnitEnergy Energy => unitEnergy;

        public void Initialize(
            UnitData data,
            TeamType team,
            int formationSlotIndex,
            BattleManager battleManager)
        {
            if(data == null)
            {
                Debug.LogError("UnitData가 비어있습니다.", this);
                return;
            }
            if(battleManager == null)
            {
                Debug.LogError("BattleManager가 비어있습니다.", this);
                return;
            }

            Data = data;
            Stats = new UnitStats(data);
            Team = team;
            FormationSlotIndex = formationSlotIndex;

            FindMissingComponents();

            if (!ValidateBattleComponents())
            {
                return;
            }

            IsInitialized = true;

            targetFinder.Initialize(this, battleManager);
            unitMovemnet.Initialize(this, battleManager, targetFinder);
            unitHealth.Initialize(this, battleManager);
            unitEnergy.Initialize(this, battleManager);

            attackController.Initialize(this, battleManager, targetFinder, unitMovemnet);

            gameObject.name = $"{team}_{data.DisplayName}_{formationSlotIndex}";
            
            // 생성 직후 올바른 SortingOrder로 표시되도록 계산
            RefreshSortingOrder();
        }

        private void LateUpdate()
        {
            if (IsInitialized)
            {
                RefreshSortingOrder();
            }
        }

        private void FindMissingComponents()
        {
            if(targetFinder == null)
            {
                targetFinder = GetComponent<UnitTargetFinder>();
            }

            if(unitMovemnet == null)
            {
                unitMovemnet = GetComponent<UnitMovement>();
            }

            if(attackController == null)
            {
                attackController = GetComponent<UnitAttackController>();
            }

            if(unitHealth == null)
            {
                unitHealth = GetComponent<UnitHealth>();
            }

            if(unitEnergy == null)
            {
                unitEnergy = GetComponent<UnitEnergy>();
            }

            if (spriteRenderers == null || spriteRenderers.Length == 0)
            {
                spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }
        }

        private bool ValidateBattleComponents()
        {
            bool isValid = true;

            if (targetFinder == null)
            {
                Debug.LogError($"{name}에 UnitTargetFinder가 비어있습니다.", this);

                isValid = false;
            }

            if (unitMovemnet == null)
            {
                Debug.LogError($"{name}에 UnitMovement가 비어있습니다.", this);

                isValid = false;
            }

            if(attackController == null)
            {
                Debug.LogError($"{name}에 AttackController가 비어있습니다.", this);
                
                isValid = false;
            }
            
            if(unitHealth == null)
            {
                Debug.LogError($"{name}에 UnitHealth가 비어있습니다.", this);

                isValid = false;
            }

            if(unitEnergy == null)
            {
                Debug.LogError($"{name}에 UnitEnergy가 비어있습니다.", this);

                isValid = false;
            }

            return isValid;
        }

        // Y 좌표가 낮으면 스프라이트 sorting order가 아래로 내려가도록 설정
        private void RefreshSortingOrder()
        {
            if(spriteRenderers == null)
            {
                return;
            }

            int order = sortingBaseOrder - Mathf.RoundToInt(transform.position.y * sortingPrecision);

            for(int i =0; i<spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].sortingOrder = order + i;
                }
            }
        }

#if UNITY_EDITOR
        // 컴포넌트를 처음 추가할 때 자동 연결
        private void Reset()
        {
            targetFinder = GetComponent<UnitTargetFinder>();
            unitMovemnet = GetComponent<UnitMovement>();
            attackController = GetComponent<UnitAttackController>();
            unitHealth = GetComponent<UnitHealth>();
            unitEnergy = GetComponent<UnitEnergy>();

            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
#endif
    }
}