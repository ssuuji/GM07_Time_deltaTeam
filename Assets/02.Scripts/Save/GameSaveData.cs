using AFKHero.Collection;
using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public StageSaveData stageSaveData;
    public PlayerSaveData playerSaveData;
    public HeroManagerSaveData heroSaveData;
    public int[] partySaveData;
    public int[] resonanceSaveData;
    public HeroSummonSaveData heroSummonSaveData;
    public QuestSaveData questSaveData;
    public OfflineRewardSaveData offlineRewardSaveData;
    public CollectionSaveData collectionSaveData;
    public EquipmentSaveData equipmentSaveData;
    public SoundSaveData soundSaveData;
}

//스테이지
[Serializable]
public class StageSaveData
{
    public int currentStageNumber;
    public int currentSectionNumber;
    public int lastStageNumber;
    public int lastSectionNumber;
}

//플레이어
[Serializable]
public class PlayerSaveData
{
    public string playerName;
    public int gold;
    public int dia;
    public int freeTicket;
}

//영웅
[Serializable]
public class HeroManagerSaveData
{
    public Dictionary<int, (int level, bool isUnlocked, HeroGrade currentGrade)> heroes;

    public int normalShards;
    public int rareShards;
    public int epicShards;
}

//파티
//는 GetPartySaveData 에서 int[] 받아 그대로 저장

//상점 : 소환 제단
[Serializable]
public class HeroSummonSaveData
{
    public int summonLevel;
    public int summonExp;
}

//퀘스트
[Serializable]
public class QuestSaveData
{
    public string lastDailyResetDate;

    public List<QuestProgressSaveData> dailyQuestSaveData = new List<QuestProgressSaveData>();
    public List<QuestProgressSaveData> repeatQuestSaveData = new List<QuestProgressSaveData>();

    public MainQuestSaveData mainQuestSaveData = new MainQuestSaveData();
}

//일일 / 반복 퀘스트
[Serializable]
public class QuestProgressSaveData
{
    public string questId;
    public int currentCount;
    public bool isRewardClaimed;
}

//메인 퀘스트
[Serializable]
public class MainQuestSaveData
{
    public string currentQuestId;
    public int currentCount;
    public bool isAllCompleted;
}

//방치보상 - 
[Serializable]
public class OfflineRewardSaveData
{
    public string lastOnlineTime;
    public int rewardGold;
}

//도감
[Serializable]
public class CollectionSaveData
{
    public List<int> collectedHeroIDs = new List<int>();    
    public List<int> claimedRewardCounts = new List<int>(); 
}

//장비
[Serializable]
public class EquipmentSaveData
{
    // 가방에 있는 장비 리스트
    public List<EquipItemSaveData> inventoryEquips = new List<EquipItemSaveData>();

    //영웅별 장착 장비
    public List<HeroEquipmentSaveData> heroEquipments = new List<HeroEquipmentSaveData>();
}

[Serializable]
public class HeroEquipmentSaveData
{
    public int heroID;

    public EquipItemSaveData weapon;
    public EquipItemSaveData armor;
    public EquipItemSaveData pants;
    public EquipItemSaveData helmet;
}

[Serializable]
public class EquipItemSaveData
{
    public string equipmentID;
    public int grade;
    public int enhanceLevel;
    public int attack;
    public int defense;
    public int hp;
}

//사운드
[Serializable]
public class SoundSaveData
{
    public float masterVolume;
    public float bgmVolume;
    public float sfxVolume;
}