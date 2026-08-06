using System.Collections.Generic;
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

        [Header("파티 - 전방 2, 후방 3")]
        [SerializeField] private List<UnitData> allyParty = new();
        [SerializeField] private List<UnitData> enemyParty = new();

        [Header("시작 옵션")]
        [Header("시작 시 자동으로 양 진영의 유닛 생성")]
        [SerializeField] private bool spawnOnStart = true;
        [Header("유닛 생성 후 자동으로 전투 시작")]
        [SerializeField] private bool startBattleAfterSpawn = true;

        private readonly List<BattleUnit> spawnedUnits =new();

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnAllUnits();
            }
        }
        [ContextMenu("모든 유닛 스폰")]
        public void SpawnAllUnits()
        {
            // 참조가 정상적으로 설정되어있는 지 확인
            if (!ValidateDependencies())
            {
                return;
            }

            // 유닛과 등록 정보 정리
            ClearSpawnedUnits();

            // 아군 유닛을 아군 진형에 생성
            SpawnParty(allyParty, TeamType.Ally);

            // 적군 유닛을 적군 진형에 생성
            SpawnParty(enemyParty, TeamType.Enemy);

            // 자동 시작 옵션이 켜져있으면 전투 바로 시작
            if (startBattleAfterSpawn)
            {
                battleManager.StartBattle();
            }
        }

        private void SpawnParty(IReadOnlyList<UnitData> party, TeamType team)
        {
            // 진형 슬롯 수 가져옮
            int formationSlotCount = formationData.GetSlotCount(team);

            // 파티 크기, 최대 유닛 수, 실제 슬롯 수 초과하지 않게 생성
            int spawnCount = Mathf.Min(party.Count, MaxPartySize, formationSlotCount);

            // 각 진형 슬롯에 배치
            for (int slotIndex = 0; slotIndex < spawnCount; slotIndex++)
            {
                UnitData unitData = party[slotIndex];

                // 중간에 데이터가 비어 있으면 해당 슬롯 건너 뛰기
                if(unitData == null)
                {
                    continue;
                }

                // UnitData에 전투용 프리팹  있는지 검사
                if(unitData.BattlePrefab == null)
                {
                    Debug.LogError($"[{unitData.name}] Battle Prefab이 비어 있습니다.");
                    continue;
                }

                // 좌표 계산
                Vector3 spawnPosition = formationData.GetWolrdPosition(
                    team,
                    slotIndex,
                    battleOrigin.position);

                // 프리팹 생성
                BattleUnit unit = Instantiate(
                    unitData.BattlePrefab, 
                    spawnPosition,
                    Quaternion.identity,
                    unitContainer);

                // BattleUnit에 데이터, 진영, 슬롯 번호 전달
                unit.Initialize(unitData, team, slotIndex, battleManager);

                // 초기화에 실패한 유닛은 등록하지 않고 정리
                if (!unit.IsInitialized)
                {
                    Debug.LogError($"[{unitData.name}] 유닛 초기화에 실패하였습니다.",unit);

                    DestroyUnitObject(unit);
                    continue;
                }

                // BattleManager 해당 진영 목록에 유닛 등록
                battleManager.RegisterUnit(unit);

                // 다시 생성 때 제거할 수 있게 생성 목록에 저장
                spawnedUnits.Add(unit);
            }
        }
        // 등록 목록 정리
        [ContextMenu("유닛 Clear")]
        public void ClearSpawnedUnits()
        {
            if(battleManager != null)
            {
                battleManager.ClearRegisteredUnits();
            }

            for (int i = spawnedUnits.Count - 1;  i >= 0; i--) 
            {
                if (spawnedUnits[i] == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(spawnedUnits[i].gameObject);
                }
                else
                {
                    DestroyImmediate(spawnedUnits[i].gameObject);
                }
            }
            spawnedUnits.Clear();
        }

        private static void DestroyUnitObject(BattleUnit unit)
        {
            if(unit == null)
            {
                return;
            }    

            GameObject unitObject = unit.gameObject;
            unitObject.SetActive(false);

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(unitObject);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(unitObject);
            }
        }

        // 유닛 생성에 필요한 BattleManager 와 FormationData 검사
        private bool ValidateDependencies()
        {
            if(battleManager == null || formationData == null)
            {
                Debug.LogError("BattleSpawner 또는 FormationData가 비어 있습니다.");
                return false;
            }

            if(battleOrigin == null)
            {
                battleOrigin = transform;
            }

            if(unitContainer == null)
            {
                unitContainer = transform;
            }

            return true;
        }

#if UNITY_EDITOR
        // 유닛 선택 시 기즈모
        private void OnDrawGizmos()
        {
            if(formationData == null)
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

            for(int i =0; i < formationData.GetSlotCount(team); i++)
            {
                Vector3 position = formationData.GetWolrdPosition(team, i, origin);
                Gizmos.DrawWireSphere(position, 0.25f);
            }
        }
#endif


        }
}