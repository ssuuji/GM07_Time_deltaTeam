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

        private Color originalColor;

        private void Awake()
        {
            if(damageText == null)
            {
                damageText = GetComponentInChildren<TMP_Text>(true);
            }

            if(damageText != null)
            {
                originalColor = damageText.color;
            }
        }

        public void Play(int damageAmount, Vector3 worldPosition)
        {
            if(damageText == null)
            {
                Debug.LogError("[DamageTextView] TMP_Text가 연결되지 않았습니다.", this);

                Destroy(gameObject);
                return;
            }

            transform.position = worldPosition;
            damageText.text = $"{damageAmount}";
            damageText.color = originalColor;

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

                Color currentColor = originalColor;
                currentColor.a = 1f - progress;
                damageText.color = currentColor;

                yield return null;
            }

            Destroy(gameObject);
        }

    }
}