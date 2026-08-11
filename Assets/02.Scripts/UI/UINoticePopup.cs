using System.Collections;
using TMPro;
using UnityEngine;

namespace AFKHero.UI
{
    public class UINoticePopup : MonoBehaviour
    {
        public static UINoticePopup Instance { get; private set; }

        [SerializeField] private GameObject noticePanel; //검정배경
        [SerializeField] private TMP_Text noticeText;    //문구

        private Coroutine hideCoroutine; //3초


        private void Awake()
        {
            Instance = this;
            noticePanel.SetActive(false);
        }

        //알림 표시
        public void Show(string message)
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }

            noticeText.text = message;
            noticePanel.SetActive(true);
        }

        //알림 표시(시간)
        public void ShowTime(string message, float duration = 3f)
        {
            Show(message);

            hideCoroutine = StartCoroutine(HideTimeCo(duration));
        }

        //알림 닫기
        public void Hide()
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }

            noticePanel.SetActive(false);
        }

        IEnumerator HideTimeCo(float duration)
        {
            yield return new WaitForSeconds(duration);

            noticePanel.SetActive(false);
            hideCoroutine = null;
        }
    }
}