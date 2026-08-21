using System.Collections.Generic;
using AFKHero.UI;
using Newtonsoft.Json.Bson;
using Unity.VisualScripting;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class BattleSpawner : MonoBehaviour
    {
        // 한 파티 당 최대 유닛 수
        private const int MaxPartySize = 5;

        [Header("컴포넌트")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private FormationData formationData;
        [SerializeField] private Transform battleOrigin;
        [SerializeField] private Transform unitContainer;

        // 다음 스테이지 시작 시 이전 유닛 정리 용 리시트
        private readonly List<BattleUnit> spawnedUnits = new List<BattleUnit>();

        public bool SpawnBattle(
            IReadOnlyList<HeroInstance> allyParty,
            IReadOnlyList<HeroData> enemyParty,
            int enemyLevel,
            float battleTimeLimit)
        {
            if (!ValidateDependencies())
            {
                return false;
            }

            if (allyParty == null || enemyParty == null)
            {
                Debug.LogError("아군 또는 적군 데이터가 비어있습니다.", this);

                return false;
            }

            ClearSpawnedUnits();

            if (!battleManager.TrySetBattleTimeLimit(battleTimeLimit))
            {
                Debug.LogError("전투 제한시간 설정에 실패 했습니다.", this);

                return false;
            }

            int allyCount = SpawnAllyParty(allyParty);

            int enemyCount = SpawnEnemyParty(enemyParty, enemyLevel);

            if (allyCount == 0 || enemyCount == 0)
            {
                Debug.LogError("전투 생성 실패");

                ClearSpawnedUnits();
                return false;
            }

            battleManager.StartBattle();

            return battleManager.CurrentState == BattleState.Fighting;
        }

        private int SpawnAllyParty(IReadOnlyList<HeroInstance> party)
        {
            int spawnCount = Mathf.Min(party.Count, MaxPartySize, formationData.GetSlotCount(TeamType.Ally));

            int successCount = 0;

            for (int slotIndex = 0; slotIndex < spawnCount; slotIndex++)
            {
                HeroInstance hero = party[slotIndex];

                if (hero == null || hero.data == null)
                {
                    continue;
                }

                if (TrySpawnUnit(hero, TeamType.Ally, slotIndex))
                {
                    successCount++;
                }
            }
            return successCount;
        }

        private int SpawnEnemyParty(IReadOnlyList<HeroData> party, int enemyLevel)
        {
            int spawnCount = Mathf.Min(party.Count, MaxPartySize, formationData.GetSlotCount(TeamType.Enemy));

            int successCount = 0;

            for (int slotIndex = 0; slotIndex < spawnCount; slotIndex++)
            {
                HeroData enemyData = party[slotIndex];

                if (enemyData == null)
                {
                    continue;
                }

                HeroInstance enemyInstance = new HeroInstance(enemyData, true);

                enemyInstance.level = Mathf.Max(1, enemyLevel);

                if (TrySpawnUnit(enemyInstance, TeamType.Enemy, slotIndex))
                {
                    successCount++;
                }
            }
            return successCount;
        }

        private bool TrySpawnUnit(HeroInstance hero, TeamType team, int slotIndex)
        {
            HeroData heroData = hero.data;

            if (heroData.BattleUnitPrefab == null)
            {
                Debug.LogError($"[{heroData.HeroName}] Prefab이 비어 있습니다.", heroData);

                return false;
            }

            Vector3 spawnPosition = formationData.GetWolrdPosition(team, slotIndex, battleOrigin.position);

            BattleUnit unit = Instantiate(heroData.BattleUnitPrefab, spawnPosition, Quaternion.identity, unitContainer);

            GameObject unitObject = unit.gameObject;

            HeroBase heroBase = unitObject.GetComponent<HeroBase>();

            if (heroBase == null)
            {
                heroBase =
                    unitObject.AddComponent<HeroBase>();
            }


            unit.Initialize(hero, team, slotIndex, battleManager);

            if (!unit.IsInitialized)
            {
                Debug.LogError($"[{heroData.HeroName}] BattleUnit 초기화 실패", unit);

                DestroyUnitObejct(unit);
                return false;
            }
            heroBase.Init(hero, team == TeamType.Enemy);
            battleManager.RegisterUnit(unit);
            spawnedUnits.Add(unit);

            if (team == TeamType.Ally)
            {
                UIBattleManager.Instance?.SetBattleUnit(slotIndex, unit); //아군만 하단 전투 UI에 연결
            }

            return true;
        }

        private static BattleUnit EnsureBattleComponents(GameObject unitObject)
        {
            GetOrAddComponent<UnitTargetFinder>(unitObject);
            GetOrAddComponent<UnitMovement>(unitObject);
            GetOrAddComponent<UnitAttackController>(unitObject);
            GetOrAddComponent<UnitHealth>(unitObject);
            GetOrAddComponent<UnitEnergy>(unitObject);

            GetOrAddComponent<UnitUltimateController>(unitObject);

            GetOrAddComponent<UnitStatusEffectController>(unitObject);

            return GetOrAddComponent<BattleUnit>(unitObject);
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();

            if (component != null)
            {
                return component;
            }

            return target.AddComponent<T>();
        }

        // 이전 전투 유닛과 BattleManaer의 등록 정보를 정리
        // 다음 전투 초기화용
        public void ClearSpawnedUnits()
        {
            if (battleManager != null)
            {
                battleManager.ClearRegisteredUnits();
            }

            for (int i = spawnedUnits.Count - 1; i >= 0; i--)
            {
                BattleUnit unit = spawnedUnits[i];

                if (unit != null)
                {
                    DestroyUnitObejct(unit);
                }
            }

            spawnedUnits.Clear();
        }

        private static void DestroyUnitObejct(BattleUnit unit)
        {
            if (unit == null)
            {
                return;
            }

            GameObject unitObject = unit.gameObject;

            unitObject.SetActive(false);

            if (Application.isPlaying)
            {
                Destroy(unitObject);
            }
            else
            {
                DestroyImmediate(unitObject);
            }
        }

        // 유효성 검사용
        private bool ValidateDependencies()
        {
            if (battleManager == null || formationData == null)
            {
                Debug.LogError("BattleManager 또는 FormationData가 비어 있습니다.", this);

                return false;
            }

            if (battleOrigin == null)
            {
                battleOrigin = transform;
            }

            if (unitContainer == null)
            {
                unitContainer = transform;
            }

            return true;
        }

#if UNITY_EDITOR
        // 유닛 선택 시 기즈모
        private void OnDrawGizmos()
        {
            if (formationData == null)
            {
                return;
            }

            // battleOrigin(위치)이 연결되어 있으면 해당 위치 사용 아니면 임시 기준점 생성
            Vector3 origin = battleOrigin != null ? battleOrigin.position : transform.position;

            // 아군
            DrawFormationGizmos(TeamType.Ally, origin, Color.cyan);

            // 적군
            DrawFormationGizmos(TeamType.Enemy, origin, Color.red);
        }

        private void DrawFormationGizmos(
            TeamType team,
            Vector3 origin,
            Color color)
        {
            Gizmos.color = color;

            for (int i = 0; i < formationData.GetSlotCount(team); i++)
            {
                Vector3 position = formationData.GetWolrdPosition(team, i, origin);
                Gizmos.DrawWireSphere(position, 0.25f);
            }
        }
#endif
    }
}