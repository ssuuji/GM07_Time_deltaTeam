using UnityEngine;
using UnityEngine.Rendering;

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

        [SerializeField] private UnitUltimateController ultimateController;
        [SerializeField] private UnitStatusEffectController statusEffectController;

        [Header("2D Depth")]
        [SerializeField] private SortingGroup sortingGroup; //[SerializeField] private SpriteRenderer[] spriteRenderers;
        
        // 배경과 겹치지 않게
        [SerializeField] private int sortingBaseOrder = 1000;

        // Sorting Order 미세 조정
        [SerializeField] private int sortingPrecision = 100;
        
        [Header("유닛 이동 제한 범위")] 
        private Vector2 maximumBattlePosition = new Vector2(2.2f, 3.5f);
        private Vector2 minimumBattlePosition = new Vector2(-2.2f, -1.5f);

        // 원본 데이터
        public HeroData Data { get; private set; }
        // 히어로 인스턴스
        public HeroInstance HeroInstance { get; private set; }
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
        public UnitUltimateController UltimateController => ultimateController;
        public UnitStatusEffectController StatusEffects => statusEffectController;

        public void Initialize(
            HeroInstance heroInstance,
            TeamType team,
            int formationSlotIndex,
            BattleManager battleManager)
        {
            if(heroInstance == null || heroInstance.data == null)
            {
                Debug.LogError("heroInstance 또는 HeroData가 비어있습니다.", this);
                return;
            }
            if(battleManager == null)
            {
                Debug.LogError("BattleManager가 비어있습니다.", this);
                return;
            }

            HeroInstance = heroInstance;
            Data = heroInstance.data;
            Team = team;
            FormationSlotIndex = formationSlotIndex;

            //아군은 오른쪽을 바라보도록 방향 전환
            if (Team == TeamType.Ally)
            {
                transform.rotation = Quaternion.Euler(0f, -180f, 0f);
            }

            Stats = new UnitStats(heroInstance);

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

            statusEffectController.Initialize(this, battleManager);

            attackController.Initialize(this, battleManager, targetFinder, unitMovemnet);

            ultimateController.Initialize(this, battleManager);

            gameObject.name = $"{team}_{Data.HeroName}_{formationSlotIndex}";
            
            // 생성 직후 올바른 SortingOrder로 표시되도록 계산
            RefreshSortingOrder();
        }

        private void LateUpdate()
        {
            if (!IsInitialized)
            {
                return;
            }

            ClampToBattleArea();

            RefreshSortingOrder();
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

            if (unitEnergy == null)
            {
                unitEnergy = GetComponent<UnitEnergy>();
            }

            if(ultimateController == null)
            {
                ultimateController = GetComponent<UnitUltimateController>();
            }

            if(statusEffectController == null)
            {
                statusEffectController = GetComponent<UnitStatusEffectController>();
            }

            /*
            if (spriteRenderers == null || spriteRenderers.Length == 0)
            {
                spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }
            */
            if (sortingGroup == null)
            {
                sortingGroup = GetComponentInChildren<SortingGroup>(true);
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

            if(ultimateController == null)
            {
                Debug.LogError($"{name}에 UnitUltimateController가 비어있습니다.", this);

                isValid = false;
            }
            
            if(statusEffectController == null)
            {
                Debug.LogError($"{name}에 UnitStatusEffectController 비어있습니다.", this);

                isValid = false;
            }

            return isValid;
        }

        // 유닛의 중심 위치가 지정된 전투 영역을 벗어나지 않도록 제한
        private void ClampToBattleArea()
        {
            Vector3 currentPosition = transform.position;

            float clampedX = Mathf.Clamp(currentPosition.x, minimumBattlePosition.x, maximumBattlePosition.x);
            float clampedY = Mathf.Clamp(currentPosition.y, minimumBattlePosition.y, maximumBattlePosition.y);

            transform.position = new Vector3(clampedX, clampedY, currentPosition.z);
        }

        // Y 좌표가 낮으면 스프라이트 sorting order가 아래로 내려가도록 설정
        private void RefreshSortingOrder()
        {
            if (sortingGroup == null)
            {
                return;
            }

            int order = sortingBaseOrder - Mathf.RoundToInt(transform.position.y * sortingPrecision);
            sortingGroup.sortingOrder = order;
            
            /*
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
            */
        }

#if UNITY_EDITOR
        // 카메라 제한 범위 기즈모
        private void OnDrawGizmos()
        {
            Vector2 center = (minimumBattlePosition + maximumBattlePosition) * 0.5f;
            Vector2 size = maximumBattlePosition - minimumBattlePosition;

            Color previousColor = Gizmos.color;
            Gizmos.color = Color.red;

            Gizmos.DrawWireCube(
                new Vector3(center.x, center.y, transform.position.z),
                new Vector3(size.x, size.y, 0f));

            Gizmos.color = previousColor;
        }
        // 컴포넌트를 처음 추가할 때 자동 연결
        private void Reset()
        {
            targetFinder = GetComponent<UnitTargetFinder>();
            unitMovemnet = GetComponent<UnitMovement>();
            attackController = GetComponent<UnitAttackController>();
            unitHealth = GetComponent<UnitHealth>();
            unitEnergy = GetComponent<UnitEnergy>();
            ultimateController = GetComponent<UnitUltimateController>();
            statusEffectController = GetComponent<UnitStatusEffectController>();

            sortingGroup = GetComponentInChildren<SortingGroup>(true); //spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
#endif
    }
}