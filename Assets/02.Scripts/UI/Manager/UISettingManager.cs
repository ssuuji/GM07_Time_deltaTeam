using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class UISettingManager : MonoBehaviour
    {
        [Header("설정창")]
        [SerializeField] private GameObject settingPanel;
        [SerializeField] private Button backButton;

        [Header("데이터 초기화")]
        [SerializeField] private GameObject deleteSavePopup;

        #region 설정창

        //설정창 열기
        public void OnClickedOpenSetting()
        {
            settingPanel.SetActive(true);
            backButton.gameObject.SetActive(true);
        }

        //설정창 닫기
        public void OnClickedCloseSetting()
        {
            deleteSavePopup.SetActive(false);
            settingPanel.SetActive(false);
            backButton.gameObject.SetActive(false);
        }

        #endregion

        #region 데이터 초기화

        //데이터 초기화 팝업 열기
        public void OnClickedOpenDeleteSavePopup()
        {
            deleteSavePopup.SetActive(true);
        }

        //데이터 초기화 팝업 닫기
        public void OnClickedCloseDeleteSavePopup()
        {
            deleteSavePopup.SetActive(false);
        }

        //데이터 초기화
        public void OnClickedDeleteSaveData()
        {
            GameSaveManager.Instance.DeleteSaveData();
        }

        #endregion
    }
}