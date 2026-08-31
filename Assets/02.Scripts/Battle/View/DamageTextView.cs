using System.Collections;
using TMPro;
using UnityEngine;

namespace AFKHero.Battle
{
    public sealed class DamageTextView : MonoBehaviour
    {
        [Header("텍스트")]
        [SerializeField] private TMP_Text damageText;

        [Header("연출")]
        [SerializeField, Min(0.1f)]
        private float duration = 0.7f;

        [SerializeField, Min(0f)]
        private float riseDistance = 0.8f;

        private Color defaultColor;
        private Color playbackColor;

        private void Awake()
        {
            if (damageText == null)
            {
                damageText = GetComponentInChildren<TMP_Text>(true);
            }

            if (damageText != null)
            {
                defaultColor = damageText.color;
                playbackColor = defaultColor;
            }
        }

        public void Play(int damageAmount, Vector3 worldPosition)
        {
            BeginPlayback($"{damageAmount}", worldPosition, defaultColor);
        }

        public void PlayText(string textMessage, Vector3 worldPosition, Color customColor)
        {
            BeginPlayback(textMessage, worldPosition, customColor);
        }

        private void BeginPlayback(
            string textMessage,
            Vector3 worldPosition,
            Color textColor)
        {
            if (damageText == null)
            {
                Debug.LogError("[DamageTextView] TMP_Text가 연결되지 않았습니다.", this);
                ReleaseOrDestroy();
                return;
            }

            // 풀에서 재사용될 때 이전 재생 상태가 남지 않도록 초기화
            StopAllCoroutines();

            transform.position = worldPosition;
            damageText.text = textMessage;

            playbackColor = textColor;
            playbackColor.a = 1f;
            damageText.color = playbackColor;

            StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            Vector3 startPosition = transform.position;
            Vector3 endPosition = startPosition + Vector3.up * riseDistance;

            float elapsedTime = 0f;

            while(elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                float progress = Mathf.Clamp01(elapsedTime / duration);

                transform.position = Vector3.Lerp(startPosition, endPosition , progress);

                Color currentColor = playbackColor;
                currentColor.a = 1f - progress;
                damageText.color = currentColor;

                yield return null;
            }

            ReleaseOrDestroy();
        }
        private void ReleaseOrDestroy()
        {
            Poolable poolable = GetComponent<Poolable>();

            // PoolManager를 통해 생성된 오브젝트만 풀로 반환
            if (PoolManager.Instance != null &&
                poolable != null &&
                !string.IsNullOrEmpty(poolable.poolKey))
            {
                poolable.Release();
                return;
            }

            // 독립 테스트처럼 PoolManager 없이 생성된 경우에는 기존 방식으로 제거
            Destroy(gameObject);
        }
    }
}