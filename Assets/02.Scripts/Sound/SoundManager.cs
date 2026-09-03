using AFKHero.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityScene = UnityEngine.SceneManagement.Scene;


namespace AFKHero.Sound
{
    [Serializable]
    public struct SoundInfo
    {
        public SoundKey Key;
        public AudioClip Clip;
        [Range(0.01f, 1f)] public float Volume;
    }

    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [SerializeField] private AudioSource bgmSource;

        [Header("BGM")]
        [SerializeField] private List<SoundInfo> bgmSounds = new();

        [Header("UI")]
        [SerializeField] private List<SoundInfo> uiSounds = new();

        [Header("Battle SFX")]
        [SerializeField] private List<SoundInfo> battleSfxSounds = new();

        private readonly Dictionary<SoundKey, AudioClip> soundData = new();
        private float currentBGMVolume = 1f;


        [Header("SFX Pool")]
        [SerializeField] private int sfxPoolSize = 10;

        private readonly Queue<AudioSource> sfxPool = new();


        [Header("볼륨 설정")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float bgmVolume = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;

        public float MasterVolume => masterVolume;
        public float BGMVolume => bgmVolume;
        public float SFXVolume => sfxVolume;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            AddSoundData(bgmSounds);
            AddSoundData(uiSounds);

            AddSoundData(battleSfxSounds);

            InitSFXPool();
        }

        private void Start()
        {
            PlaySceneBGM(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void AddSoundData(List<SoundInfo> sounds)
        {
            foreach (SoundInfo info in sounds)
            {
                if (soundData.ContainsKey(info.Key))
                {
                    Debug.LogError("<color=cyan>[SoundManager]</color> : 같은 Key로 사운드를 등록 시도하고 있습니다.");
                    continue;
                }

                soundData[info.Key] = info.Clip;
            }
        }

        //씬에 맞는 BGM 재생
        private void OnSceneLoaded(UnityScene scene, LoadSceneMode mode)
        {
            PlaySceneBGM(scene);
        }

        //씬에 맞는 BGM 설정
        private void PlaySceneBGM(UnityScene scene)
        {
            if (scene.name == SceneNames.GetSceneName(SceneType.Title))
            {
                PlayBGM(SoundKey.BGM_Title);
            }
            else if (scene.name == SceneNames.GetSceneName(SceneType.Game))
            {
                PlayBGM(SoundKey.BGM_Idle);
            }
        }

        #region SFX Pool

        private void InitSFXPool()
        {
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject obj = new GameObject($"SFX_{i}");
                obj.transform.SetParent(transform);

                AudioSource source = obj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;

                sfxPool.Enqueue(source);
            }
        }

        private AudioSource GetSFXSource()
        {
            if (sfxPool.Count > 0)
            {
                return sfxPool.Dequeue();
            }

            // 풀이 모두 사용 중이면 새로 생성
            GameObject obj = new GameObject($"SFX_{transform.childCount}");
            obj.transform.SetParent(transform);

            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;

            return source;
        }

        private IEnumerator ReturnToPool(AudioSource source)
        {
            yield return new WaitForSeconds(source.clip.length);

            source.clip = null;
            sfxPool.Enqueue(source);
        }

        #endregion

        #region SFX

        public void PlaySFX(SoundKey key)
        {
            if (!soundData.TryGetValue(key, out AudioClip clip))
            {
                Debug.LogError($"<color=cyan>[SoundManager]</color> : 재생할 효과음 {key}가 올바르게 설정되지 않았습니다.");
                return;
            }

            AudioSource source = GetSFXSource();

            source.volume = masterVolume * sfxVolume;

            source.clip = clip;
            source.Play();

            StartCoroutine(ReturnToPool(source));
        }

        //현재 키 값에 맞는 오디오 소스를 찾아서 플레이 중인지 확인
        public bool IsPlayingSFX(SoundKey key)
        {
            foreach (Transform child in transform)
            {
                AudioSource source = child.GetComponent<AudioSource>();

                if (source != null &&
                    source.isPlaying &&
                    source.clip != null &&
                    soundData.TryGetValue(key, out AudioClip clip) &&
                    source.clip == clip)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region BGM

        public void PlayBGM(SoundKey key)
        {
            if (!soundData.TryGetValue(key, out AudioClip clip))
            {
                Debug.LogError($"<color=cyan>[SoundManager]</color> : 재생할 배경음 {key}가 올바르게 설정되지 않았습니다.");
                return;
            }

            if (bgmSource.clip == clip && bgmSource.isPlaying)
                return;

            SoundInfo soundInfo = bgmSounds.Find(x => x.Key == key);
            currentBGMVolume = soundInfo.Volume;

            float finalBGMVolume = masterVolume * bgmVolume * currentBGMVolume;

            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.volume = finalBGMVolume;
            bgmSource.Play();
        }

        private void UpdateBGMVolume()
        {
            bgmSource.volume = masterVolume * bgmVolume * currentBGMVolume;
        }

        #endregion

        #region 볼륨 설정

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateBGMVolume();
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            UpdateBGMVolume();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        #endregion

        #region 저장 데이터

        //사운드 저장 데이터 생성
        public SoundSaveData CreateSoundSaveData()
        {
            return new SoundSaveData
            {
                masterVolume = masterVolume,
                bgmVolume = bgmVolume,
                sfxVolume = sfxVolume
            };
        }

        //사운드 저장 데이터 불러오기
        public void LoadSoundSaveData(SoundSaveData saveData)
        {
            if (saveData == null)
                return;

            masterVolume = Mathf.Clamp01(saveData.masterVolume);
            bgmVolume = Mathf.Clamp01(saveData.bgmVolume);
            sfxVolume = Mathf.Clamp01(saveData.sfxVolume);

            UpdateBGMVolume();
        }

        #endregion
    }
}