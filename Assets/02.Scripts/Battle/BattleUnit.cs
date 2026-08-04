using UnityEngine;

namespace Battle
{
    public sealed class BattleUnit : MonoBehaviour
    {
        [Header("2D Depth")]

        [SerializeField] private SpriteRenderer[] spriteRenderers;

        // 배경과 겹치지 않게
        [SerializeField] private int sortingBaseOrder = 1000;

        // Sorting Order 세밀 조정
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


        public void Initialize(
            UnitData data,
            TeamType team,
            int formationSlotIndex)
        {
            if(data == null)
            {
                Debug.LogError("BattleUnit 초기화에 UnitData가 비어있습니다.", this);
                return;
            }

            Data = data;
            Stats = new UnitStats(data);
            Team = team;
            FormationSlotIndex = formationSlotIndex;
            IsInitialized = true;

            if(spriteRenderers == null || spriteRenderers.Length == 0)
            {
                spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            }

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
        // 컴포넌트를 처음 추가할 때 SpriteRenderer 자동 연결
        private void Reset()
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
#endif
    }
}