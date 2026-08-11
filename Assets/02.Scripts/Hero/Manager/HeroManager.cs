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

            // ID 1~5번은 기본 지급 영웅으로 가정하여 해금처리
            bool isDefaultUnlocked = data.HeroID <= 5;

            // 딕셔너리에 영웅 인스턴스를 저장
            heroDictionary.Add(data.HeroID, new HeroInstance(data, isDefaultUnlocked));
        }
    }

    // =============================
    // 영웅 해금 및 레벨업
    // =============================

    // 영웅 획득 시 호출함
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

            // 현재 레벨업에 필요한 골드 확인
            int cost = hero.LevelUpCost;

            // CurrencyManager를 통해 골드 차감 시도
            if (CurrencyManager.Instance != null && CurrencyManager.Instance.UseGold(cost))
            {
                // 골드 차감이 성공했다면 실제 레벨업 진행
                bool success = hero.LevelUp();
                if (success)
                {
                    Debug.Log($"[HeroManager] {hero.data.HeroName} 레벨업 완료! (LV.{hero.level}) / 소모 골드: {cost}");
                }
                return success;
            }
            else
            {
                // 돈이 부족해서 false를 반환한 경우 레벨업은 취소
                return false;
            }
        }
        return false;
    }

    // 전체 영웅 리스트를 반환
    public List<HeroInstance> GetAllHeroes() => new List<HeroInstance>(heroDictionary.Values);

    // 저장할 핵심 데이터(레벨, 해금여부)만 추출함.
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
                // 데이터 변조 방지를 위해 최대 레벨 제한 적용
                hero.level = Mathf.Clamp(kvp.Value.level, 1, 50);
                hero.isUnlocked = kvp.Value.isUnlocked;
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