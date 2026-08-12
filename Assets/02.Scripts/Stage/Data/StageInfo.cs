using AFKHero.Battle;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageInfo", menuName = "Game Data / Stage Info")]
public class StageInfo : ScriptableObject
{
    [Header("스테이지 정보")]

    [Header("스테이지 번호")]
    [SerializeField] private int stageNumber = 1;       // 스테이지 번호
    [Header("스테이지 내부 구간 번호")]
    [SerializeField] private int sectionNumber = 1;     // 스테이지 내부 구간 번호
    [Header("보스 스테이지 인지 확인")]
    [SerializeField] private bool isBossStage;

    [Header("전투 설정")]

    [Header("해당 구간의 전투 제한 시간")]
    [SerializeField] private float timeLimit = 120.0f;
    [Header("해당 구간에 등장하는 적 목록")]
    [SerializeField] private List<UnitData> enemies = new List<UnitData>();

    [Header("클리어 보상")]
    [SerializeField] private int clearGold;




    public int StageNumber => stageNumber;
    public int SectionNumber => sectionNumber;
    public bool IsBossStage => isBossStage;
    public float TimeLimit => timeLimit;
    public int ClearGold => clearGold;
    public List<UnitData> Enemies => enemies;
 
}
