
using System;
using System.IO;
using UnityEngine;

//모든 세이브 데이터를 통합 관리할 클래스입니다.
//아래에 만들어질 각각의 SaveData클래스를 필드로 가져야 합니다.
//인벤토리나 보유 영웅들을 저장하기 위해서는 List로 관리해야 할 것 같은데 이 부분은 조금 더 연구해보겠습니다.
[Serializable]
public class GameSaveData
{
    public StageSaveData stageSaveData;
}



//현재 스테이지, 섹션 / 마지막으로 클리어한 스테이지, 섹션을 저장할 클래스입니다.
//세이브가 필요한 부분이 있다면, 필요한 값들을 복사할 클래스를 아래와 유사하게 만들어주세요.
[Serializable]
public class StageSaveData
{
    public int currentStageNumber;
    public int currentSectionNumber;
    public int lastStageNumber;
    public int lastSectionNumber;
}

//임시용 세이브 매니저입니다
//싱글톤이 적용되어 있으며, SaveStage() 메서드는 StageManager 내부에서 호출하기에 수정하게 될 경우 유의 바랍니다.

public class TempSaveManager : MonoBehaviour
{
    public static TempSaveManager Instance;

    [SerializeField] private string saveFileName = "save.json";

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        LoadStage();
    }

    private void OnApplicationQuit()
    {
        SaveStage();
    }



    public void SaveStage()
    {
        //이 부분도 마찬가지로 GameSaveData와 같은 모든 세이브를 관리할 클래스 명의로 호출해야 하고,
        //SaveGame으로 명칭이 바뀌어야 한다 => StageManager에서 호출부 수정할 것.
        //GameSaveData 내부는 StageSaveData, PlayerSaveData 이런 클래스만 필드로 갖고,
        //실제로 세이브를 할 때는 그 필드에 값을 넣는 식으로.

        //GameSaveData saveData = CreateSaveData();이런 식으로 바꾸기만 하면 될 듯?
        StageSaveData saveData = StageManager.Instance.CreateStageSaveData();

        string json = JsonUtility.ToJson(saveData, true);

        File.WriteAllText(SavePath, json);
        Debug.Log($"Save Complete. path : {SavePath}");
    }


    //json파일을 읽어오기 때문에, 이 부분은 LoadGame으로 명칭이 바뀌어야 한다. 어차피 수정할 부분 두 곳밖에 없음.
    public void LoadStage()
    {
       if(!HasSave())
        {
            Debug.Log($"Save file does not exit. path : {SavePath}");
            return;
        }

        string json = File.ReadAllText(SavePath);

        //이것도 모든 것들을 한 번에 불러오려면, GameSaveData클래스 형으로 받아야 한다.
        //GameSaveData saveData = JsonUtility.FromJson<GameSaveData(json);
        StageSaveData saveData = JsonUtility.FromJson<StageSaveData>(json);

        if(saveData == null)
        {
            Debug.LogWarning("Save data is null");
            return;
        }

        //이 부분을 ApplyLoadedData 이런 메서드 만들고 그걸 호출해야 한다.
        StageManager.Instance.LoadStageSaveData(saveData);
        Debug.Log($"Load complete. path : {SavePath}");
    }

    public bool HasSave()
    {
        return File.Exists(SavePath);
    }

    [ContextMenu("Delete Save")]
    public void DeleteSave()
    {
        if (!HasSave()) return;

        File.Delete(SavePath);
        Debug.Log($"Save deleted. path : {SavePath}");
    }


    //게임 실행 중 세이브가 필요한 모든 것들을 한 번에 저장해버릴 메서드
    //저장이 필요한 부분이 있다면, 위의 StageSaveData처럼 클래스 하나 만들고
    //GameSaveData 내부에 필드로 갖게 하고
    //저장이 호출되어야 하는 매니저 내부에 Create무엇SaveData(); 메서드를 구현해야합니다.
    //여기에서 saveData 필드에 저장 데이터를 할당하면 될 것 같습니다?
    //만일 특정 상황에서 어떤 매니저가 파괴되거나 한다면, NullReferenceException이 뜰 수도 있습니다.
    public GameSaveData CreateSaveData()
    {
        GameSaveData saveData = new GameSaveData();

        saveData.stageSaveData = StageManager.Instance.CreateStageSaveData();



        return saveData;
    }


    //게임에서 필요한 모든 값들을 한 번에 로드할 메서드.
    //GameSaveData내부에 추가된 필드를 사용하여, StageManager호출한 것처럼 하시면 될 것 같습니다.
    //로드가 필요한 매니저 내부에 Load무엇SaveData 메서드를 구현해야합니다.
    //다만, 저장이 필요한 매니저들은 모두 싱글톤이어야 완전히 똑같은 모양새로 작성될 것입니다.
    public void ApplyLoadedData(GameSaveData saveData)
    {
        StageManager.Instance.LoadStageSaveData(saveData.stageSaveData);
    }


}
