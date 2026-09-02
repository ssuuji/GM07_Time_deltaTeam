using AFKHero.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class UISettingManager : MonoBehaviour
    {
        [Header("설정창")]
        [SerializeField] private GameObject settingPanel;
        [SerializeField] private Button backButton;

        [Header("사운드 설정")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("데이터 초기화")]
        [SerializeField] private GameObject deleteSavePopup;

        private void Start()
        {
            //현재 볼륨 값으로 슬라이더 초기화
            masterVolumeSlider.SetValueWithoutNotify(SoundManager.Instance.MasterVolume);
            bgmVolumeSlider.SetValueWithoutNotify(SoundManager.Instance.BGMVolume);
            sfxVolumeSlider.SetValueWithoutNotify(SoundManager.Instance.SFXVolume);

            //슬라이더 값 변경 이벤트 등록
            masterVolumeSlider.onValueChanged.AddListener(OnChangedMasterVolume);
            bgmVolumeSlider.onValueChanged.AddListener(OnChangedBGMVolume);
            sfxVolumeSlider.onValueChanged.AddListener(OnChangedSFXVolume);
        }

        #region 사운드 설정

        //전체 볼륨 변경
        private void OnChangedMasterVolume(float volume)
        {
            SoundManager.Instance.SetMasterVolume(volume);
        }

        //BGM 볼륨 변경
        private void OnChangedBGMVolume(float volume)
        {
            SoundManager.Instance.SetBGMVolume(volume);
        }

        //효과음 볼륨 변경
        private void OnChangedSFXVolume(float volume)
        {
            SoundManager.Instance.SetSFXVolume(volume);
        }

        #endregion

        #region 설정창

        //설정창 열기
        public void OnClickedOpenSetting()
        {
            //메인 탭이 열려있다면 먼저 닫기
            if (UIManager.Instance != null && UIManager.Instance.TryCloseMainTab())
            {
                return;
            }

            GuideManager.Instance?.PauseGuide();

            settingPanel.SetActive(true);
            backButton.gameObject.SetActive(true);
        }

        //설정창 닫기
        public void OnClickedCloseSetting()
        {
            deleteSavePopup.SetActive(false);
            settingPanel.SetActive(false);
            backButton.gameObject.SetActive(false);

            GuideManager.Instance?.ResumeGuide();
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