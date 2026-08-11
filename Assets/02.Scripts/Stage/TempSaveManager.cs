using System;
using System.IO;
using UnityEngine;


//현재 스테이지, 섹션 / 마지막으로 클리어한 스테이지, 섹션을 저장할 클래스입니다.
//추후에 저장할 내용을 통합 관리할 SaveData 클래스 구현 시 사용해주세요.
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
        StageSaveData saveData = StageManager.Instance.CreateStageSaveData();
        string json = JsonUtility.ToJson(saveData, true);

        File.WriteAllText(SavePath, json);
        Debug.Log($"Save Complete. path : {SavePath}");
    }

    public void LoadStage()
    {
       if(!HasSave())
        {
            Debug.Log($"Save file does not exit. path : {SavePath}");
            return;
        }

        string json = File.ReadAllText(SavePath);
        StageSaveData saveData = JsonUtility.FromJson<StageSaveData>(json);

        if(saveData == null)
        {
            Debug.LogWarning("Save data is null");
            return;
        }

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
}
