using TMPro;
using UnityEngine;

namespace AFKHero.UI
{
    public class UINoticePopup : MonoBehaviour
    {
        public static UINoticePopup Instance { get; private set; }

        [SerializeField] private GameObject noticePanel; //검정배경
        [SerializeField] private TMP_Text noticeText;    //문구

        private void Awake()
        {
            Instance = this;
            noticePanel.SetActive(false);
        }

        //알림 표시
        public void Show(string message)
        {
            noticeText.text = message;
            noticePanel.SetActive(true);
        }

        //알림 숨기기
        public void Hide()
        {
            noticePanel.SetActive(false);
        }
    }
}