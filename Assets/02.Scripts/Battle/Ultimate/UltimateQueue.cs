using System;
using System.Collections.Generic;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class UltimateQueue
    {
        // 궁국기 대기 순서를 보관하는 리스트
        private readonly List<BattleUnit> waitingUnits = new();

        // 같은 유닛이 대기열에 중복 등록 검사
        private readonly HashSet<BattleUnit> queueUnits = new();

        // 서로 다른 프레임에 들어온 요청 순서 등록 프레임
        private readonly Dictionary<BattleUnit, int> queueFrames = new();

        // 현재 대기 중인 궁극기 개수
        public int Count => waitingUnits.Count;

        // 외부에 대기 순서 전달
        public IReadOnlyList<BattleUnit> WaitingUnits => waitingUnits;

        // 유닛이 대기열에 정상 등록되면 알림
        public event Action<BattleUnit> UnitEnqueue;

        public event Action QueueChanged;

        // 궁극기 준비된 유닛 대기열에 한번만 등록
        public bool TryEnqueue(BattleUnit unit, int queueFrame)
        {
            if(!CanEnqueue(unit) || queueUnits.Contains(unit))
            {
                return false;
            }

            queueUnits.Add(unit);
            queueFrames.Add(unit, queueFrame);

            int insertIndex = FindInsertIndex(unit, queueFrame);

            waitingUnits.Insert(insertIndex, unit);
            UnitEnqueue?.Invoke(unit);
            QueueChanged?.Invoke();

            return true;
        }

        // 큐에서 우선순위가 가장 높은 유닛을 큐에서 제거
        public bool TryDequeue(out BattleUnit unit)
        {
            if(waitingUnits.Count == 0)
            {
                unit = null;
                return false;
            }

            unit = waitingUnits[0];

            return AllRemoveAt(0, out unit);
        }

        // 사용자가 선택한 궁극기를 대기열에서 찾아 제거
        public bool TryDequeue(BattleUnit requestUnit, out BattleUnit unit)
        {
            int index = waitingUnits.IndexOf(requestUnit);

            if(index < 0)
            {
                unit = null;
                return false;
            }

            return AllRemoveAt(index, out unit);
        }

        // 현재 우선순위가 가장 높은 유닛을 꺼냄 수동모드에서는 적 궁극기는 자동 실행할 때 사용
        public bool TryDequeueFirst(TeamType team, out BattleUnit unit)
        {
            for(int i = 0; i < waitingUnits.Count; i++)
            {
                if (waitingUnits[i] != null && waitingUnits[i].Team == team)
                {
                    return AllRemoveAt(i, out unit);
                }
            }

            unit = null;
            return false;
        }

        public bool TryGetFirst(Predicate<BattleUnit> condition, out BattleUnit unit)
        {
            if(condition == null)
            {
                unit = null;
                return false;
            }

            for(int i = 0; i < waitingUnits.Count; i++)
            {
                BattleUnit waitingUnit = waitingUnits[i];

                if(waitingUnit != null && condition(waitingUnit))
                {
                    unit = waitingUnit;
                    return true;
                }
            }
            unit = null;
            return false;
        }

        // 지정한 유닛이 이미 궁극기 대기열에 있는지 확인
        public bool Contains(BattleUnit unit)
        {
            return unit != null && queueUnits.Contains(unit);
        }

        // 유닛이 죽거나 궁극기를 사용할 수 없는 유닛 대기열에서 제거
        public bool Remove(BattleUnit unit)
        {
            if(unit == null || !queueUnits.Remove(unit))
            {
                return false;
            }

            queueFrames.Remove(unit);
            waitingUnits.Remove(unit);
            QueueChanged?.Invoke();
            return true;
        }

        // 큐 비우기
        public void Clear()
        {
            bool hadWaitingUnit = waitingUnits.Count > 0;

            waitingUnits.Clear();
            queueUnits.Clear();
            queueFrames.Clear();

            if (hadWaitingUnit)
            {
                QueueChanged?.Invoke();
            }
        }

        private bool AllRemoveAt(int index, out BattleUnit unit)
        {
            if(index < 0 || index >= waitingUnits.Count)
            {
                unit = null;
                return false;
            }

            unit = waitingUnits[index];
            waitingUnits.RemoveAt(index);
            queueUnits.Remove(unit);
            queueFrames.Remove(unit);

            QueueChanged?.Invoke();
            return true;
        }
        
        private static bool CanEnqueue(BattleUnit unit)
        {
            return unit != null &&
                unit.IsInitialized &&
                unit.Stats != null &&
                unit.Stats.IsAlive &&
                unit.Energy != null &&
                unit.Energy.IsUltimateReady;
        }

        // 유닛이 큐 순서 어디에 들어갈 지 위치 찾기
        // 같은 프레임이면 아군(우선) -> 적
        // 같은 진영이면 편성 슬롯 번호가 빠른 유닛 우선
        private int FindInsertIndex(BattleUnit newUnit, int newQueueFrame)
        {
            for (int i = 0; i < waitingUnits.Count; i++) 
            {
                BattleUnit waitingUnit = waitingUnits[i];

                int waitingQueueFrame = queueFrames[waitingUnit];

                if(PriorityUltimate(newUnit, newQueueFrame, waitingUnit, waitingQueueFrame))
                {
                    return i;
                }
            }
            return waitingUnits.Count;
        }

        // 궁극기 우선순위 비교
        private static bool PriorityUltimate(
            BattleUnit newUnit, 
            int newQueueFrame, 
            BattleUnit waitingUnit, 
            int waitingQueueFrame)
        {
            if(newQueueFrame != waitingQueueFrame)
            {
                return newQueueFrame < waitingQueueFrame;
            }

            if(newUnit.Team != waitingUnit.Team)
            {
                return newUnit.Team == TeamType.Ally;
            }

            return newUnit.FormationSlotIndex < waitingUnit.FormationSlotIndex;
        }

    }
}
