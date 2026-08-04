using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Battle
{
    public sealed class BattleManager : MonoBehaviour
    {
        // 현재 전투에 등록된 아군 유닛
        private readonly List<BattleUnit> allyUnits = new();

        // 현재 전투에 등록된 적군 유닛
        private readonly List<BattleUnit> enemyUnits = new();

        // 현재 전투의 진행 상태
        public BattleState CurrentState { get; private set; } = BattleState.None;

        // 아군 유닛 목록
        public IReadOnlyList<BattleUnit> AllyUnits => allyUnits;
        // 적군 유닛 목록
        public IReadOnlyList<BattleUnit> EnemyUnits => enemyUnits;

        // 전투 상태 변경
        public event Action<BattleState> StateChanged;

        private void Awake()
        {
            ChangeState(BattleState.Preparing);
        }
        
        // 해당 진영 목록에 유닛 등록
        public void RegisterUnit(BattleUnit unit)
        {
            if(unit == null || !unit.IsInitialized)
            {
                Debug.LogError("초기화되지 않은 BattleUnit은 등록할 수 없습니다.",this);
                return;
            }

            // 유닛 진영에 따라 등록할 내부 목록 결정
            List<BattleUnit> targetList = unit.Team == TeamType.Ally ? allyUnits : enemyUnits;

            // 같은 유닛이 여러 번 등록되지 않도록 방지
            if (!targetList.Contains(unit))
            {
                targetList.Add(unit);
            }
        }

        // 반대 진영 목록 반환 (가장 가까운 적 탐색용)
        public IReadOnlyList<BattleUnit> GetOpponents(TeamType requesterTeam)
        {
            return requesterTeam == TeamType.Ally ? enemyUnits : allyUnits;
        }

        // 양 진영에 한 명 이상 등록되어 있다면 전투 시작
        public void StartBattle()
        {
            if(allyUnits.Count == 0 || enemyUnits.Count == 0)
            {
                Debug.LogError("양 진영에 유닛이 한 명 이상 있어야 전투를 시작할 수 있습니다.", this);
                return;
            }

            ChangeState(BattleState.Fighting);

            Debug.Log($"전투 시작!\n" +
                $"아군 [{allyUnits.Count}]명 / 적군 [{enemyUnits.Count}]명 ");
        }

        // 새로운 전투 시작 시 유닛 재정비
        public void ClearRegisteredUnits()
        {
            allyUnits.Clear();
            enemyUnits.Clear();

            ChangeState(BattleState.Preparing);
        }

        // 현재 전투 상태를 새로운 상태로 변경하고 이벤트 발생
        private void ChangeState(BattleState nextState)
        {
            if(CurrentState == nextState)
            {
                return;
            }

            CurrentState = nextState;

            // 상태 변경을 구독하고 있는 UI나 전투 시스템에 알림
            StateChanged?.Invoke(CurrentState);
        }
    }
}
