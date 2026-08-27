using System.Collections.Generic;
using UnityEngine;

//공명을 관리할 스크립트
//구현되어 있는 PartyManager와 유사한 구조로 작성 시도중

//TODO
//영웅들마다 원본 레벨이 어딘가에 저장되어있어야 함. => Dictionary 사용해봄.
//공명이 활성화되면 계산된 레벨을 적용하고, 공명이 비활성화되면 저장된 레벨을 다시 적용해야 함. => 메서드 작성됨
//영웅 쪽에서 공명 대상이 된 영웅들의 상태를 설정하고, 공명 상태라면 레벨업을 할 수 없게 설정해야 함. => 작성했으나 오류 있음.
//UI 상에서는 공명 대상인 영웅을 클릭했을 경우 레벨업을 제한하게 해야 함.

public class ResonanceManager : MonoBehaviour
{
    public static ResonanceManager Instance { get; private set; }

    [Header("공명 슬롯")]
    [SerializeField]private HeroInstance[] resonanceSlots = new HeroInstance[5];
    [Header("공명으로 적용할 레벨")]
    [SerializeField]private int resonanceLevel = 0;

    private bool isResonanceOn;

    //공명 이전의 원본 레벨을 저장할 딕셔너리
    private Dictionary<HeroInstance, int> originalLevelDict = new Dictionary<HeroInstance, int>();

   

    //프로퍼티
    //슬롯에 등록되어 있는 영웅들을 읽을 프로퍼티
    public HeroInstance[] ResonanceSlots => resonanceSlots;
    //현재 적용중인 공명 레벨을 읽을 프로퍼티
    public int ResonanceLevel => resonanceLevel;
    //공명 활성화시 이펙트를 출력하고자 한다면 사용할 프로퍼티
    public bool IsResonanceOn => isResonanceOn;

    public Dictionary<HeroInstance, int> OriginalLevelDict => originalLevelDict;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //private void OnApplicationQuit()
    //{
    //    DisableResonance();
    //}

    //영웅 배치, 영웅 해제, 공명 슬롯 상태 저장, 로드 이 4개의 메서드 정도는 public으로 두고,
    //그 외의 메서드들은 해당 클래스 내부에서만 실행될 가능성이 높으므로 private으로 변경을 고려한다.


    //영웅을 배열에 넣는 메서드
    public void PlaceHero(int slotIndex, HeroInstance hero)
    {
        if (slotIndex < 0 || slotIndex >= resonanceSlots.Length) return;
        if (hero == null | !hero.isUnlocked) return;

        //새로 배치 전, 공명 상태라면 현재 공명을 해지해야 한다.
        if(isResonanceOn)
        {
            DisableResonance();
            isResonanceOn = false;
        }

        //같은 영웅이 다른 슬롯에 등록되어 있는지 검사하고, 그렇다면 그 슬롯을 null로 바꾼다.
        for (int i = 0; i < resonanceSlots.Length; i++)
        {
            if (resonanceSlots[i] == hero) resonanceSlots[i] = null;
        }

        //선택한 슬롯에 영웅을 넣는다.
        resonanceSlots[slotIndex] = hero;

        //그 뒤, 다시 공명 상태인지 검사한다.
        UpdateResonance();
    }

    //영웅을 배열에서 빼는 메서드
    public void RemoveHero(HeroInstance heroToRemove)
    {
        if (heroToRemove == null) return;

        for (int i = 0; i < resonanceSlots.Length; i++)
        {
            if (resonanceSlots [i] == heroToRemove)
            {
                Debug.Log($"[ResonanceManager] {heroToRemove.data.HeroName} 공명 제단에서 해제됨");                

                resonanceSlots[i] = null;
                //공명 적용 메서드
                UpdateResonance();
                break;
            }
        }
    }

    //공명 활성화 조건인지 아닌지 판별하고, 맞다면 공명을 적용할 메서드.
    public void UpdateResonance()
    {
        for(int i =0; i < resonanceSlots.Length; i++)
        {
            //한 칸이라도 비어있다면 공명을 적용하지 않음.
            if (resonanceSlots[i] == null || resonanceSlots[i].data == null)
            {
                Debug.Log("[ResonanceManager : 빈 칸이 있어 공명이 해제됩니다.");

                isResonanceOn = false;
                //공명을 해제하고, 영웅들을 원래 레벨로 돌릴 메서드 실행
                DisableResonance();
                return;
            }
        }

        //여길 통과했다는 것은 모든 칸이 등록됐다는 것

        //첫 레벨을 임시로 0번 인덱스의 레벨과 동일하게 설정한다.
        resonanceLevel = resonanceSlots[0].level;

        //for문을 통해 공명으로 설정할 레벨의 최솟값을 검색한다.
        for (int i =0; i < resonanceSlots.Length;i++)
        {
            if (resonanceSlots[i].level <= resonanceLevel)
            {
                resonanceLevel = resonanceSlots[i].level;
            }
        }

        //for문을 통과하면 최저레벨 계산이 완료된다.

        //모든 영웅들이 들어있는 리스트를 가져오고 해금 상태를 검사한다.
        List<HeroInstance> heroes = HeroManager.Instance.GetAllHeroes();
        
        foreach(HeroInstance hero in heroes)
        {
            if (hero == null || hero.data == null) continue; //인스턴스가 없거나 데이터가 없으면 건너뛰기
            if (!hero.isUnlocked) continue; //해금되어있지 않으면 건너뛰기
  
            if(IsHeroInResonanceSlot(hero))
            {
                //슬롯 안에 있는 영웅이라면 공명 적용 X = false로 설정
                hero.isResonanced = false;
            }
            else
            {
                //딕셔너리에 키를 영웅으로, 값을 원래 레벨로 등록해둔 다음, 
                originalLevelDict.TryAdd(hero, hero.level);
       
                if (hero.level < resonanceLevel) //공명 레벨보다 낮을 때만 공명 레벨로 적용하되, 공명을 적용받게 합니다.
                {
                    hero.level = resonanceLevel;
                } 

                //슬롯 밖에 있는 영웅이라면 공명 적용 O = true로 설정
                hero.isResonanced = true;
            }   
        }
        Debug.Log("[ResonanceManager] : 공명이 적용됩니다.");
        isResonanceOn = true;
    }

    //현재 검사중인 영웅이 공명 슬롯에 들어와있는지 검사할 메서드.
    public bool IsHeroInResonanceSlot(HeroInstance hero)
    {
        if (hero == null) return false;

        for (int i = 0; i < resonanceSlots.Length; i++) //for문을 돌면서, 슬롯에 해당 영웅이 등록되어 있다면 true를 반환하고 메서드 종료.
        {
            if (resonanceSlots[i] == hero)
            {
                return true;
            }
        }
        return false;
    }


    //공명 상태를 해제하고, 영웅들의 레벨을 원래대로 복구할 메서드
    //이걸 우선 OnApplicationQuit에서 한 번 호출해봐야 하나?
    private void DisableResonance()
    {
        List<HeroInstance> heroes = HeroManager.Instance.GetAllHeroes();

        foreach(HeroInstance hero in heroes)
        {
            if(hero == null || hero.data == null) continue; 
            if (!hero.isUnlocked) continue;


            //영웅이 기존까지 공명 상태에 걸려있었는지 확인한다.
            if(hero.isResonanced)
            {
                if(originalLevelDict.TryGetValue(hero, out int originalLevel))
                {
                     hero.level = originalLevel;
                     hero.isResonanced = false;
                }
            }
        }
        //지금까지 들어있던 저장 레벨들을 모두 비우고, 공명으로 설정할 레벨도 0으로 초기화한다.
        originalLevelDict.Clear();
        resonanceLevel = 0;
    }

    //TODO
    //공명슬롯 저장
    //공명슬롯 로드

    //PartyManager와는 달리 레벨을 직접 건드는 거라서, 다른 저장/불러오기 방식이 필요하다.
    //가령, ID와 원본 레벨을 저장해서, 불러오게 한다던가.
    //UpdateResonance의 기능 분리가 필요할 수도 있음. 원본 레벨을 저장해서 불러올 때, 뭐가 먼저 실행되느냐에 따라
    //레벨 설정이 꼬일 가능성이 있음.


    /*
    public ResonanceSaveData CreateResonanceSaveData()
    {    
        //슬롯 안에 있는 영웅들 저장
        for (int i = 0; i < resonanceSlots.Length; i++)
        {
            //슬롯과 슬롯에 있는 data가 null이 아니면 거기에 HeroID를 넣고, null이면 -1을 넣는다.

            saveData.resonanceSlots[i] = (resonanceSlots[i] != null && resonanceSlots[i].data != null) ? resonanceSlots[i].data.HeroID : -1;

        }

        //슬롯 밖에 있는 영웅들 원본 레벨 저장
        List<HeroInstance> heroes = HeroManager.Instance.GetAllHeroes();
        foreach (HeroInstance hero in heroes)
        {
            if (hero == null || hero.data == null) continue;
            if (!hero.isUnlocked) continue;
            if (!hero.isResonanced) continue;


            //딕셔너리에 인스턴스를 넣어 원본 레벨을 추출하고,
            //새로운 딕셔너리를 만들어서 아이디와 원본 레벨을 저장한다
            //이게 맞나?
           

            //영웅이 기존까지 공명 상태에 걸려있었는지 확인한다.
            if (hero.isResonanced)
            {
                if (originalLevelDict.TryGetValue(hero, out int originalLevel))
                {
                    hero.level = originalLevel;
                    hero.isResonanced = false;
                }
            }
        }

        return saveData;
    }

    */

    public int[] GetResonanceSaveData()
    {
        int[] savedIDs = new int[5];
        for (int i = 0; i < 5; i++)
        {
            savedIDs[i] = (resonanceSlots[i] != null && resonanceSlots[i].data != null) ? resonanceSlots[i].data.HeroID : -1;
        }
        return savedIDs;
    }


    public void LoadResonanceFromData(int[] savedIDs)
    {
        //유효한 세이브가 아니라면 return;
        if (savedIDs == null || savedIDs.Length != resonanceSlots.Length) return;

        for (int i = 0; i < savedIDs.Length;i++)
        {
            //저장 데이터 슬롯에서 영웅 ID를 가져온다.
            int heroID = savedIDs[i];

            if(heroID != -1)
            {
                //아이디를 통해 영웅 인스턴스를 가져온다.
                HeroInstance hero = HeroManager.Instance.GetHeroByID(heroID);

                //null이 아니고, 해금되어 있다면 슬롯에 등록한다. (공명중인지 상태도 체크해야 할지 고민이네.)
                if (hero != null && hero.isUnlocked)
                {
                    resonanceSlots[i] = hero;
                }
            }
            else
            {
                resonanceSlots[i] = null;
            }
        }
        UpdateResonance();
    }
}