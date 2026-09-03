using UnityEngine;

namespace AFKHero.UI
{
    public class UIBattleSpeedManager : MonoBehaviour
    {
        [Header("배속 이펙트")]
        [SerializeField] private GameObject highlightEffect; // 테두리를 달릴 빛 (GlowDot)
        [SerializeField] private float moveSpeed = 400f;     // 빛이 이동하는 속도
        [SerializeField] private float pathSize = 50f;       // 버튼의 절반 크기 (테두리 범위)

        private bool isDoubleSpeed = false;
        private RectTransform effectRect;
        private int currentEdge = 0; // 0:위(우로이동), 1:오른쪽(아래로), 2:아래(좌로), 3:왼쪽(위로)

        private void Start()
        {
            if (highlightEffect != null)
            {
                effectRect = highlightEffect.GetComponent<RectTransform>();
                // 시작할 때 빛의 위치를 좌측 상단 모서리로 초기화
                effectRect.anchoredPosition = new Vector2(-pathSize, pathSize);
            }
            SetNormalSpeed();
        }

        private void Update()
        {
            // 2배속이 켜져 있을 때만 사각형 궤도를 따라 이동
            if (isDoubleSpeed && effectRect != null)
            {
                MoveAlongSquare();
            }
        }

        // 사각형 테두리를 따라 이동하는 함수
        private void MoveAlongSquare()
        {
            Vector2 pos = effectRect.anchoredPosition;
            float step = moveSpeed * Time.unscaledDeltaTime;

            if (currentEdge == 0) // 위쪽 테두리 (오른쪽으로 이동)
            {
                pos.x += step;
                if (pos.x >= pathSize) { pos.x = pathSize; currentEdge = 1; }
            }
            else if (currentEdge == 1) // 오른쪽 테두리 (아래로 이동)
            {
                pos.y -= step;
                if (pos.y <= -pathSize) { pos.y = -pathSize; currentEdge = 2; }
            }
            else if (currentEdge == 2) // 아래쪽 테두리 (왼쪽으로 이동)
            {
                pos.x -= step;
                if (pos.x <= -pathSize) { pos.x = -pathSize; currentEdge = 3; }
            }
            else if (currentEdge == 3) // 왼쪽 테두리 (위로 이동)
            {
                pos.y += step;
                if (pos.y >= pathSize) { pos.y = pathSize; currentEdge = 0; }
            }

            effectRect.anchoredPosition = pos;
        }

        public void OnClickedSpeedToggle()
        {
            isDoubleSpeed = !isDoubleSpeed;

            if (isDoubleSpeed) SetDoubleSpeed();
            else SetNormalSpeed();
        }

        private void SetNormalSpeed()
        {
            Time.timeScale = 1.0f;
            if (highlightEffect != null) highlightEffect.SetActive(false);
        }

        private void SetDoubleSpeed()
        {
            Time.timeScale = 2.0f;
            if (highlightEffect != null) highlightEffect.SetActive(true);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1.0f;
        }
    }
}