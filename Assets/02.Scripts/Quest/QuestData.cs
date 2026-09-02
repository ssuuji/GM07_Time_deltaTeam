using System.Collections.Generic;
using UnityEngine;

namespace AFKHero.Quest
{
    [CreateAssetMenu(fileName = "Quest", menuName = "Quest / Quest Data")]
    public class QuestData : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string questId;          //퀘스트 ID
        [SerializeField] private QuestType questType;     //퀘스트 종류
        [SerializeField] private string questName;        //퀘스트 이름

        [TextArea]
        [SerializeField] private string description;      //퀘스트 설명

        [SerializeField] private bool isEnabled = true;   //퀘스트 사용 여부

        [Header("완료 조건")]
        [SerializeField] private QuestConditionType conditionType; //퀘스트 완료 조건
        [SerializeField] private int targetCount = 1;              //목표 수치

        [Header("스테이지 클리어 조건")]
        [SerializeField] private int targetStageNumber = 1;        //목표 스테이지 번호
        [SerializeField] private int targetSectionNumber = 1;      //목표 구간 번호

        [Header("보상")]
        [SerializeField] private List<QuestReward> rewards = new(); //퀘스트 보상 목록 (여러개 지급 할 수 있으니 리스트로 만들어봄)

        [Header("메인 퀘스트")]
        [SerializeField] private int order;                                  //메인 퀘스트 진행 순서
        [SerializeField] private GuideTarget guideTarget = GuideTarget.None; //가이드 이동 목적지
        [SerializeField] private bool autoGuide;                             //퀘스트 진입 시 자동 가이드 여부





        public string QuestId => questId;         //퀘스트 ID
        public QuestType QuestType => questType;  //퀘스트 종류
        public string QuestName => questName;     //퀘스트 이름
        public string Description => description; //퀘스트 설명
        public bool IsEnabled => isEnabled;       //퀘스트 사용 여부
        public QuestConditionType ConditionType => conditionType;  //퀘스트 완료 조건
        public int TargetCount => targetCount;                     //목표 수치
        public int TargetStageNumber => targetStageNumber;         //목표 스테이지 번호
        public int TargetSectionNumber => targetSectionNumber;     //목표 구간 번호
        public List<QuestReward> Rewards => rewards;               //퀘스트 보상 목록
        public int Order => order;                                 //메인 퀘스트 진행 순서
        public GuideTarget GuideTarget => guideTarget;             //가이드 이동 목적지
        public bool AutoGuide => autoGuide;                        //자동 가이드 여부
    }
}

