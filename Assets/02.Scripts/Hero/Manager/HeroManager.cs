using System.Collections.Generic;
using UnityEngine;

// 모든 영웅의 획득 상태와 성장을 총괄함.
public class HeroManager : MonoBehaviour
{
    public static HeroManager Instance { get; private set; }

    [Header("영웅 원본 데이터")]
    [SerializeField] private List<HeroData> allHeroDataList = new List<HeroData>();

    // 영웅 ID를 키로 사용하여 현재 상태를 빠르게 검색
    private Dictionary<int, HeroInstance> heroDictionary = new Dictionary<int, HeroInstance>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 유지
            InitializeHeroes(); // 영웅 초기화 함수 실행
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지를 위해 파괴
        }
    }

    private void InitializeHeroes()
    {
        foreach (var data in allHeroDataList) // 등록된 영웅 데이터를 순회
        {
            if (data == null) continue;

            // ID 1001번 영웅 딱 1명만 기본으로 지급하도록 변경
            bool isDefaultUnlocked = data.HeroID == 1001;

            // 딕셔너리에 영웅 인스턴스를 저장
            heroDictionary.Add(data.HeroID, new HeroInstance(data, isDefaultUnlocked));
        }
    }

    // =============================
    // 영웅 해금 및 레벨업 / 승급
    // =============================

    // 영웅 획득 시 호출함
    public bool UnlockHero(int heroID)
    {
        if (heroDictionary.TryGetValue(heroID, out HeroInstance hero))
        {
            if (!hero.isUnlocked)
            {
                // 최초 획득 시
                hero.isUnlocked = true;
                Debug.Log($"[HeroManager] {hero.data.HeroName} 최초 해금 완료!");
                return true;
            }
            else
            {
                // 이미 해금된 영웅일 경우 (중복 획득)
                hero.duplicateCount++;
                Debug.Log($"[HeroManager] {hero.data.HeroName} 중복 획득! (보유 중복 카드: {hero.duplicateCount}장)");
                return true;
            }
        }
        return false;
    }

    // 영웅 레벨업
    public bool LevelUpHero(int heroID)
    {
        // ID 검색 및 해금 상태 확인
        if (heroDictionary.TryGetValue(heroID, out HeroInstance hero) && hero.isUnlocked)
        {
            // 이미 최고 레벨인지 먼저 확인
            if (hero.level >= 50)
            {
                Debug.LogWarning($"{hero.data.HeroName}은(는) 이미 최고 레벨(50)입니다.");
                return false;
            }

            //  레벨업만 진행
            bool success = hero.LevelUp();
            if (success)
            {
                Debug.Log($"[HeroManager] {hero.data.HeroName} 레벨업 완료! (현재 LV.{hero.level})");
            }
            return success;
        }
        return false;
    }

    // 추가된 부분 : 영웅 등급업
    public bool UpgradeHeroGrade(int heroID)
    {
        if (heroDictionary.TryGetValue(heroID, out HeroInstance hero) && hero.isUnlocked)
        {
            // 중복 카드가 충분한지 검사하고 차감한 뒤 이 함수를 호출합니다.
            hero.UpgradeGrade();
            return true;
        }
        return false;
    }

    // 전체 영웅 리스트를 반환
    public List<HeroInstance> GetAllHeroes() => new List<HeroInstance>(heroDictionary.Values);

    // =============================
    // 데이터 조회 및 세이브/로드
    // =============================

    // 저장할 핵심 데이터에 duplicateCount 와 currentGrade추가
    public Dictionary<int, (int level, bool isUnlocked, int duplicateCount, HeroGrade currentGrade)> GetSaveData()
    {
        var saveData = new Dictionary<int, (int, bool, int, HeroGrade)>();
        foreach (var kvp in heroDictionary)
        {
            saveData.Add(kvp.Key, (kvp.Value.level, kvp.Value.isUnlocked, kvp.Value.duplicateCount, kvp.Value.currentGrade));
        }
        return saveData;
    }

    // 불러올 핵심 데이터에 duplicateCount 와 currentGrade추가
    public void LoadSaveData(Dictionary<int, (int level, bool isUnlocked, int duplicateCount, HeroGrade currentGrade)> savedData)
    {
        foreach (var kvp in savedData)
        {
            if (heroDictionary.TryGetValue(kvp.Key, out HeroInstance hero))
            {
                // 데이터 변조 방지를 위해 최대 레벨 제한 적용
                hero.level = Mathf.Clamp(kvp.Value.level, 1, 50);
                hero.isUnlocked = kvp.Value.isUnlocked;
                hero.duplicateCount = kvp.Value.duplicateCount; // 중복 카드 수량 로드 연동
                hero.currentGrade = kvp.Value.currentGrade;     // 현재 등급 로드 연동
            }
        }
    }

    // 파티 매니저가 저장된 파티를 불러올 때 영웅 ID로 인스턴스를 찾아주는 함수
    public HeroInstance GetHeroByID(int heroID)
    {
        if (heroDictionary.TryGetValue(heroID, out HeroInstance hero))
        {
            return hero;
        }
        return null;
    }
}