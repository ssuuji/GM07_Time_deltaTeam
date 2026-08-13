using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public StageSaveData stageSaveData;
    public PlayerSaveData playerSaveData;
    public HeroManagerSaveData heroSaveData;
    public int[] partySaveData;
    public HeroSummonSaveData heroSummonSaveData;
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

