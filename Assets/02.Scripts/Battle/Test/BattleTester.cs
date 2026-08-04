using UnityEngine;

/// <summary>
/// 아군과 적군을 전방·후방 진형으로 생성하는 테스트 코드입니다.
/// </summary>
public class SimpleSpawnTest : MonoBehaviour
{
    [Header("생성할 프리팹")]

    // 생성할 아군 프리팹입니다.
    [SerializeField] private GameObject allyPrefab;

    // 생성할 적군 프리팹입니다.
    [SerializeField] private GameObject enemyPrefab;


    [Header("유닛 수")]

    // 한 진영에서 생성할 전체 유닛 수입니다.
    [SerializeField, Min(1)]
    private int unitCount = 5;

    // 전체 유닛 중 전방에 배치할 유닛 수입니다.
    // unitCount가 5이고 frontUnitCount가 2라면
    // 전방 2명, 후방 3명으로 배치됩니다.
    [SerializeField, Min(1)]
    private int frontUnitCount = 2;


    [Header("진형 간격")]

    // 같은 행에 있는 유닛 사이의 좌우 간격입니다.
    [SerializeField, Min(0.1f)]
    private float columnSpacing = 1.5f;

    // 전방과 후방 사이의 간격입니다.
    [SerializeField, Min(0.1f)]
    private float rowSpacing = 1.5f;

    // 아군 진영 중심과 적군 진영 중심 사이의 거리입니다.
    [SerializeField, Min(0.1f)]
    private float teamDistance = 5f;


    /// <summary>
    /// 게임이 시작되면 아군과 적군을 자동으로 생성합니다.
    /// </summary>
    private void Start()
    {
        // 아군 진영은 화면 아래쪽에 있습니다.
        // frontDirection이 1이므로 전방 유닛은 위쪽으로 배치됩니다.
        SpawnTeam(
            allyPrefab,
            -teamDistance * 0.5f,
            1f,
            "Ally");

        // 적군 진영은 화면 위쪽에 있습니다.
        // frontDirection이 -1이므로 전방 유닛은 아래쪽으로 배치됩니다.
        SpawnTeam(
            enemyPrefab,
            teamDistance * 0.5f,
            -1f,
            "Enemy");
    }


    /// <summary>
    /// 한 진영의 유닛을 전방과 후방으로 나눠 생성합니다.
    /// </summary>
    /// <param name="prefab">생성할 유닛 프리팹</param>
    /// <param name="teamCenterY">진영의 Y축 중심 위치</param>
    /// <param name="frontDirection">전방 방향. 아군은 1, 적군은 -1</param>
    /// <param name="teamName">Hierarchy에 사용할 진영 이름</param>
    private void SpawnTeam(
        GameObject prefab,
        float teamCenterY,
        float frontDirection,
        string teamName)
    {
        // Inspector에 프리팹이 연결됐는지 확인합니다.
        if (prefab == null)
        {
            Debug.LogError(
                $"{teamName} 프리팹이 연결되지 않았습니다.",
                this);

            return;
        }

        // 전방 유닛 수가 전체 유닛 수를 넘지 않도록 제한합니다.
        int actualFrontCount = Mathf.Min(
            frontUnitCount,
            unitCount);

        // 전체 유닛 수에서 전방 유닛 수를 빼면
        // 후방 유닛 수를 구할 수 있습니다.
        int backUnitCount =
            unitCount - actualFrontCount;

        for (int i = 0; i < unitCount; i++)
        {
            // 현재 유닛이 전방에 속하는지 확인합니다.
            bool isFrontRow =
                i < actualFrontCount;

            // 현재 유닛이 속한 행의 인원수를 구합니다.
            int currentRowCount = isFrontRow
                ? actualFrontCount
                : backUnitCount;

            // 현재 행 안에서 사용할 순번입니다.
            int rowIndex = isFrontRow
                ? i
                : i - actualFrontCount;

            // 각 행이 가운데 정렬되도록 X 좌표를 계산합니다.
            float xPosition =
                (rowIndex - (currentRowCount - 1) * 0.5f)
                * columnSpacing;

            // 전방 또는 후방의 Y 좌표를 계산합니다.
            float yPosition = isFrontRow
                ? teamCenterY + frontDirection * rowSpacing * 0.5f
                : teamCenterY - frontDirection * rowSpacing * 0.5f;

            Vector3 spawnPosition =
                transform.position +
                new Vector3(
                    xPosition,
                    yPosition,
                    0f);

            // 계산한 위치에 유닛 프리팹을 생성합니다.
            GameObject spawnedUnit = Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity);

            // 전방과 후방을 Hierarchy에서 확인할 수 있도록
            // 생성된 오브젝트의 이름을 설정합니다.
            string rowName = isFrontRow
                ? "Front"
                : "Back";

            spawnedUnit.name =
                $"{teamName}_{rowName}_{rowIndex + 1}";
        }
    }


    /// <summary>
    /// Scene 창에서 실제 생성 위치를 미리 보여줍니다.
    /// BattleTest 오브젝트를 선택하면 표시됩니다.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 아군 생성 위치는 파란색으로 표시합니다.
        Gizmos.color = Color.cyan;

        DrawTeamGizmos(
            -teamDistance * 0.5f,
            1f);

        // 적군 생성 위치는 빨간색으로 표시합니다.
        Gizmos.color = Color.red;

        DrawTeamGizmos(
            teamDistance * 0.5f,
            -1f);
    }


    /// <summary>
    /// 한 진영의 전방·후방 생성 위치를 Gizmo로 표시합니다.
    /// </summary>
    private void DrawTeamGizmos(
        float teamCenterY,
        float frontDirection)
    {
        int actualFrontCount = Mathf.Min(
            frontUnitCount,
            unitCount);

        int backUnitCount =
            unitCount - actualFrontCount;

        for (int i = 0; i < unitCount; i++)
        {
            bool isFrontRow =
                i < actualFrontCount;

            int currentRowCount = isFrontRow
                ? actualFrontCount
                : backUnitCount;

            int rowIndex = isFrontRow
                ? i
                : i - actualFrontCount;

            float xPosition =
                (rowIndex - (currentRowCount - 1) * 0.5f)
                * columnSpacing;

            float yPosition = isFrontRow
                ? teamCenterY + frontDirection * rowSpacing * 0.5f
                : teamCenterY - frontDirection * rowSpacing * 0.5f;

            Vector3 position =
                transform.position +
                new Vector3(
                    xPosition,
                    yPosition,
                    0f);

            Gizmos.DrawWireSphere(
                position,
                0.3f);
        }
    }
}