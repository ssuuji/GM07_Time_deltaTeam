using System.Collections.Generic;
using UnityEngine;

// 모든 영웅의 획득 상태와 성장을 총괄함.
public class HeroManager : MonoBehaviour
{
    public static HeroManager Instance { get; private set; }

    [Header("영웅 원본 데이터 (ScriptableObject)")]
    [SerializeField] private List<HeroData> allHeroDataList = new List<HeroData>();

    // 영웅 ID를 키로 사용하여 현재 상태를 빠르게 검색
    private Dictionary<int, HeroInstance> heroDictionary = new Dictionary<int, HeroInstance>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // 자신을 유일한 매니저로 등록함
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 유지함
            InitializeHeroes(); // 영웅 초기화 함수 실행함
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지를 위해 파괴함
        }
    }

    private void InitializeHeroes()
    {
        foreach (var data in allHeroDataList) // 등록된 영웅 데이터를 순회함
        {
            if (data == null) continue; // 빈 데이터는 건너뜀

            // ID 1~5번은 기본 지급 영웅으로 가정하여 해금(true) 처리함
            bool isDefaultUnlocked = data.HeroID <= 5;

            // 딕셔너리에 영웅 인스턴스를 저장함.
            heroDictionary.Add(data.HeroID, new HeroInstance(data, isDefaultUnlocked));
        }
    }

    // ==========================================
    // 영웅 해금 및 성장 로직
    // ==========================================

    // 영웅 획득 시 호출함 (성공 true, 실패 false 반환)
    public bool UnlockHero(int heroID)
    {
        // ID 검색 및 미해금 상태 확인함
        if (heroDictionary.TryGetValue(heroID, out HeroInstance hero) && !hero.isUnlocked)
        {
            hero.isUnlocked = true; // 해금 상태로 변경함
            Debug.Log($"[HeroManager] {hero.data.HeroName} 해금 완료!");
            return true;
        }
        return false; // 이미 해금됐거나 없는 ID면 false 반환
    }

    // 영웅 레벨업 시 호출
    public bool LevelUpHero(int heroID)
    {
        // ID 검색 및 해금 상태 확인
        if (heroDictionary.TryGetValue(heroID, out HeroInstance hero) && hero.isUnlocked)
        {
            return hero.LevelUp(); // 레벨업 로직 실행
        }
        return false;
    }

    // 전체 영웅 리스트를 반환
    public List<HeroInstance> GetAllHeroes() => new List<HeroInstance>(heroDictionary.Values);

    // 수지님용(Save): 저장할 핵심 데이터(레벨, 해금여부)만 추출함.
    public Dictionary<int, (int level, bool isUnlocked)> GetSaveData()
    {
        var saveData = new Dictionary<int, (int, bool)>();
        foreach (var kvp in heroDictionary)
        {
            saveData.Add(kvp.Key, (kvp.Value.level, kvp.Value.isUnlocked));
        }
        return saveData;
    }

    public void LoadSaveData(Dictionary<int, (int level, bool isUnlocked)> savedData)
    {
        foreach (var kvp in savedData)
        {
            if (heroDictionary.TryGetValue(kvp.Key, out HeroInstance hero))
            {
                hero.level = kvp.Value.level;
                hero.isUnlocked = kvp.Value.isUnlocked;
            }
        }
    }
}