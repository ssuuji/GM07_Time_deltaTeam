using System.Collections.Generic;
using UnityEngine;

// 모든 영웅의 획득 상태와 성장을 총괄함.
public class HeroManager : MonoBehaviour
{
    public static HeroManager Instance { get; private set; }

    [Header("영웅 원본 데이터")]
    [SerializeField] private List<HeroData> allHeroDataList = new List<HeroData>();

    public IReadOnlyList<HeroData> AllHeroDataList => allHeroDataList;
    // 영웅 ID를 키로 사용하여 현재 상태를 빠르게 검색
    private Dictionary<int, HeroInstance> heroDictionary = new Dictionary<int, HeroInstance>();

    // =========================================
    // 추가된 부분 : 공용 조각
    // =========================================
    [Header("공용 조각 인벤토리")]
    public int normalShards = 0; // 노말 공용 조각
    public int rareShards = 0;   // 레어 공용 조각
    public int epicShards = 0;   // 에픽 공용 조각

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
                // 안전 장치 추가! ResonanceManager가 있을 때만 실행
                if (ResonanceManager.Instance != null)
                {
                    ResonanceManager.Instance.UpdateResonance();
                }
                return true;
            }
            else
            {
                // 중복 획득 시 개별 조각 대신 공용 조각 1개 증가
                if (hero.data.HeroGrade == HeroGrade.Normal) normalShards++;
                else if (hero.data.HeroGrade == HeroGrade.Rare) rareShards++;
                else if (hero.data.HeroGrade == HeroGrade.Epic) epicShards++;

                Debug.Log($"[HeroManager] {hero.data.HeroName} 중복 획득! {hero.data.HeroGrade} 공용 조각 1개 획득!");
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
                // 0824 수정 : 레벨업 성공 시 공명 상황을 갱신합니다.
                ResonanceManager.Instance.UpdateResonance();
                Debug.Log($"[HeroManager] {hero.data.HeroName} 레벨업 완료! (현재 LV.{hero.level})");
            }
            return success;
        }
        return false;
    }

    // 추가된 부분 : 영웅 등급업 (공용 조각 차감 로직 적용)
    public bool UpgradeHeroGrade(int heroID)
    {
        if (heroDictionary.TryGetValue(heroID, out HeroInstance hero) && hero.isUnlocked)
        {
            if (hero.currentGrade >= HeroGrade.EpicPlus)
            {
                Debug.LogWarning("이미 최고 등급입니다.");
                return false;
            }

            // 영웅 인스턴스에서 필요한 조각 종류와 개수 가져오기
            HeroGrade requiredGrade = hero.GetRequiredShardGrade();
            int requiredCount = hero.GetRequiredShardCount();
            bool canUpgrade = false;

            // 지갑에 공용 조각이 충분한지 검사하고 차감하기
            if (requiredGrade == HeroGrade.Normal && normalShards >= requiredCount)
            {
                normalShards -= requiredCount;
                canUpgrade = true;
            }
            else if (requiredGrade == HeroGrade.Rare && rareShards >= requiredCount)
            {
                rareShards -= requiredCount;
                canUpgrade = true;
            }
            else if (requiredGrade == HeroGrade.Epic && epicShards >= requiredCount)
            {
                epicShards -= requiredCount;
                canUpgrade = true;
            }

            // 조건 충족 시 승급 진행
            if (canUpgrade)
            {
                hero.UpgradeGrade();
                return true;
            }
            else
            {
                Debug.LogWarning($"[{hero.data.HeroName}] 승급 실패! {requiredGrade} 공용 조각이 {requiredCount}개 필요합니다.");
                return false;
            }
        }
        return false;
    }

    // 전체 영웅 리스트를 반환
    public List<HeroInstance> GetAllHeroes() => new List<HeroInstance>(heroDictionary.Values);

    // =============================
    // 데이터 조회 및 세이브/로드
    // =============================

    // 수정된 부분 :  duplicateCount 삭제
    public Dictionary<int, (int level, bool isUnlocked, HeroGrade currentGrade)> GetSaveData()
    {
        var saveData = new Dictionary<int, (int, bool, HeroGrade)>();
        foreach (var kvp in heroDictionary)
        {
            // 0824 수정 : 공명 적용중인 영웅이라면 원본 레벨 꺼내서 원본 레벨을 저장하게 합니다.
            if(kvp.Value.isResonanced)
            {
                ResonanceManager.Instance.OriginalLevelDict.TryGetValue(kvp.Value, out int originalLevel);
                saveData.Add(kvp.Key, (originalLevel, kvp.Value.isUnlocked, kvp.Value.currentGrade));
            }//아니면 기존대로 저장합니다.
            else
            {
                saveData.Add(kvp.Key, (kvp.Value.level, kvp.Value.isUnlocked, kvp.Value.currentGrade));
            }
        }
        return saveData;
    }

    // 수정된 부분 : duplicateCount 삭제
    public void LoadSaveData(Dictionary<int, (int level, bool isUnlocked, HeroGrade currentGrade)> savedData)
    {
        foreach (var kvp in savedData)
        {
            if (heroDictionary.TryGetValue(kvp.Key, out HeroInstance hero))
            {
                // 데이터 변조 방지를 위해 최대 레벨 제한 적용
                hero.level = Mathf.Clamp(kvp.Value.level, 1, 50);
                hero.isUnlocked = kvp.Value.isUnlocked;
                hero.currentGrade = kvp.Value.currentGrade;
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