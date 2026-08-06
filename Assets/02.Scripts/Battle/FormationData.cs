using System.Collections.Generic;
using UnityEngine;

namespace AFKHero.Battle
{
    [CreateAssetMenu(
        fileName = "FormationData_Default",
        menuName = "Battle/Formation Data")]
    public sealed class FormationData : ScriptableObject
    {
        // 한 파티당 최대 유닛 수
        private const int MaxPartySize = 5;

        [Header("Ally: Front 2, back 3")]

        // 팀 진형 배치
        [SerializeField]
        private List<Vector2> allyPositions = new()
        {
            new Vector2(-0.8f, -1.3f),
            new Vector2(0.8f, -1.6f),

            new Vector2(-1.6f, -3.0f),
            new Vector2(0f, -3.0f),
            new Vector2(1.6f, -3.0f),
        };

        // 적 진형 배치
        [Header("Enemy: Front 2, back 3")]
        [SerializeField]
        private List<Vector2> enemyPositions = new()
        {
            new Vector2(0.8f,1.3f),
            new Vector2(-0.8f,1.6f),

            new Vector2(1.6f, 3.0f),
            new Vector2(0f,3.0f),
            new Vector2(-1.6f,3.0f)
        };

        // 슬롯 개수 반환
        public int GetSlotCount(TeamType team)
        {
            return GetPositions(team).Count;
        }

        // 실제 생성 위치 계산
        public Vector3 GetWolrdPosition(
            TeamType team,
            int slotIndex,
            Vector3 origin)
        {
            // 진영에 맞는 위치 목록
            IReadOnlyList<Vector2> positions = GetPositions(team);

            // 존재하지 않는 슬롯 번호가 들어오면 디버그
            if(slotIndex < 0 || slotIndex >= positions.Count)
            {
                throw new System.ArgumentOutOfRangeException(nameof(slotIndex),
                    $"{team} 진영의 {slotIndex}번 슬롯이 존재하지 않습니다.");
            }

            Vector2 offset = positions[slotIndex];

            // 전투 중심점과 상대 좌표를 더하여 최종 월드 위치를 반환
            return origin + new Vector3(offset.x, offset.y, 0f);
        }
        
        // 위치 목록 반환
        private IReadOnlyList<Vector2> GetPositions(TeamType team)
        {
            return team == TeamType.Ally ? allyPositions : enemyPositions;
        }
    }
}
