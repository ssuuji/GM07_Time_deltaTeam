using AFKHero.Quest;
using AFKHero.Scene;
using AFKHero.Shop;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using AFKHeroPlayerManager = AFKHero.Player.PlayerManager;

[DefaultExecutionOrder(-100)]
public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance;

    [SerializeField] private string saveFileName = "save.json"; //저장파일
    private GameSaveData loadedSaveData;

    public GameSaveData LoadedSaveData => loadedSaveData;
    private string SavePath
    {
        get
        {
#if UNITY_EDITOR
            return Path.Combine(Application.persistentDataPath, saveFileName); //유니티 에디터에서는 기존 저장 위치 사용
#else
        string buildPath = Directory.GetParent(Application.dataPath).FullName; //빌드에서는 exe가 있는 폴더에 저장
        return Path.Combine(buildPath, saveFileName);
#endif
        }
    }

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
        }
    }

    private void Start() //임시 : 나중엔 타이틀씬에서 로드 -> 게임씬 불러오기로
    {
        //에디터에서 Game씬을 바로 실행해도 저장데이터 불러올수 있게끔
        if (IsGameScene(SceneManager.GetActiveScene()) && loadedSaveData == null)
        {
            LoadSaveData();
            ApplyLoadedData();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }


    private void OnApplicationQuit()
    {
        if (!IsGameScene(SceneManager.GetActiveScene())) return;

        SaveGame();
    }

    //씬 로드 완료 시 호출
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsGameScene(scene)) return;

        ApplyLoadedData();
    }

    #region 저장

    //게임전체저장
    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        GameSaveData saveData = CreateSaveData();

        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(SavePath, json);
        loadedSaveData = saveData;

        Debug.Log($"[GameSaveManager]저장 완료 : {SavePath}");
    }

    //저장데이터 생성
    private GameSaveData CreateSaveData()
    {
        GameSaveData saveData = new GameSaveData();

        //스테이지
        if (StageManager.Instance != null)
        {
            saveData.stageSaveData = StageManager.Instance.CreateStageSaveData();
        }
        
        //플레이어
        if (AFKHeroPlayerManager.Instance != null)
        {
            saveData.playerSaveData = AFKHeroPlayerManager.Instance.CreatePlayerSaveData();
        }

        //영웅
        if (HeroManager.Instance != null)
        {
            saveData.heroSaveData = new HeroManagerSaveData();

            saveData.heroSaveData.heroes = HeroManager.Instance.GetSaveData();
            saveData.heroSaveData.normalShards = HeroManager.Instance.normalShards;
            saveData.heroSaveData.rareShards = HeroManager.Instance.rareShards;
            saveData.heroSaveData.epicShards = HeroManager.Instance.epicShards;
        }

        //파티 슬롯
        if (PartyManager.Instance != null)
        {
            saveData.partySaveData = PartyManager.Instance.GetPartySaveData();
        }

        //공명 슬롯
        if (ResonanceManager.Instance != null)
        {
            saveData.resonanceSaveData = ResonanceManager.Instance.GetResonanceSaveData();
        }

        //상점 : 소환 제단
        if (HeroSummonManager.Instance != null)
        {
            saveData.heroSummonSaveData = HeroSummonManager.Instance.CreateHeroSummonSaveData();
        }

        //퀘스트
        if (QuestManager.Instance != null)
        {
            saveData.questSaveData = QuestManager.Instance.CreateQuestSaveData();
        }

        return saveData;
    }

    //저장 파일 존재 여부
    public bool HasSave()
    {
        return File.Exists(SavePath);
    }

    #endregion

    #region 불러오기

    //저장 파일 불러오기
    public void LoadSaveData()
    {
        if (!HasSave())
        {
            Debug.Log($"[GameSaveManager] 저장 파일 없음 : {SavePath}");
            loadedSaveData = null;
            return;
        }

        string json = File.ReadAllText(SavePath);

        loadedSaveData = JsonConvert.DeserializeObject<GameSaveData>(json);

        if (loadedSaveData == null)
        {
            Debug.LogWarning("[GameSaveManager] 저장 데이터가 없습니다.");
            return;
        }

        Debug.Log("[GameSaveManager] 저장 데이터 읽기 완료");
    }

    //불러온 데이터를 각 매니저에 적용
    public void ApplyLoadedData()
    {
        if (loadedSaveData == null)
        {
            Debug.Log("[GameSaveManager] 적용할 저장 데이터가 없습니다.");
            return;
        }

        //스테이지
        if (StageManager.Instance != null && loadedSaveData.stageSaveData != null)
        {
            StageManager.Instance.LoadStageSaveData(loadedSaveData.stageSaveData);
        }

        //플레이어
        if (AFKHeroPlayerManager.Instance != null && loadedSaveData.playerSaveData != null)
        {
            AFKHeroPlayerManager.Instance.LoadPlayerSaveData(loadedSaveData.playerSaveData);
        }

        //영웅
        if (HeroManager.Instance != null && loadedSaveData.heroSaveData != null)
        {
            if (loadedSaveData.heroSaveData.heroes != null)
            {
                HeroManager.Instance.LoadSaveData(loadedSaveData.heroSaveData.heroes);
            }

            HeroManager.Instance.normalShards = loadedSaveData.heroSaveData.normalShards;
            HeroManager.Instance.rareShards = loadedSaveData.heroSaveData.rareShards;
            HeroManager.Instance.epicShards = loadedSaveData.heroSaveData.epicShards;
        }

        //파티 슬롯
        if (PartyManager.Instance != null && loadedSaveData.partySaveData != null)
        {
            PartyManager.Instance.LoadPartyFromData(loadedSaveData.partySaveData);
        }

        if (ResonanceManager.Instance != null && loadedSaveData.resonanceSaveData != null)
        {
            ResonanceManager.Instance.LoadResonanceFromData(loadedSaveData.resonanceSaveData);
        }

        //상점 : 소환 제단
        if (HeroSummonManager.Instance != null && loadedSaveData.heroSummonSaveData != null)
        {
            HeroSummonManager.Instance.LoadHeroSummonSaveData(loadedSaveData.heroSummonSaveData);
        }

        //퀘스트
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.LoadQuestSaveData(loadedSaveData.questSaveData);
        }

        Debug.Log("[GameSaveManager] 저장 데이터 적용 완료");
    }

    #endregion

    #region 삭제

    //저장 데이터 초기화
    [ContextMenu("Delete Save")]
    public void DeleteSaveData()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("세이브 데이터 삭제 완료");
        }
        else
        {
            Debug.Log("삭제할 세이브 데이터가 없습니다.");
        }

        loadedSaveData = null;                                            //메모리에 남아있는 저장 데이터도 초기화
        SceneManager.LoadScene(SceneNames.GetSceneName(SceneType.Title)); //타이틀씬 부터 다시 로드
    }

    #endregion

    //씬 이름 체크
    private bool IsGameScene(Scene scene)
    {
        return scene.name == SceneNames.GetSceneName(SceneType.Game);
    }
}
