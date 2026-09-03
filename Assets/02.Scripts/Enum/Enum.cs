
/////////////////////////////////
////////// 스테이지 /////////////

public enum EnemySpawnSlot // 적 소환될 위치 
{
   FrontLeft,       // 전열 왼쪽
   FrontRight,      // 전열 오른쪽
   BackLeft,        // 후열 왼쪽 
   BackCenter,      // 후열 중앙
   BackRight        // 후열 오른쪽
}

public enum StageState //스테이지 매니저의 상태 측정용
{    
    None,
    Idle,
    Working,
    Result
}

/////////////////////////////////



/////////////////////////////////
//////////// 사운드 /////////////
public enum SoundKey 
{
    // BGM
    BGM_Title,
    BGM_Idle,
    BGM_Stage,
    BGM_Stage2,
    BGM_Stage3,
    BGM_Stage4,
    BGM_Stage5,
    BGM_Boss,

    //UI
    UI_ButtonSelect,
    UI_ButtonClose,

    //Battle
    SFX_Ultimate_Start,
    SFX_Ultimate_Warrior,
    SFX_Ultimate_Tank,
    SFX_Ultimate_Mage,
    SFX_Ultimate_Archer,
    SFX_Ultimate_Healer,
    SFX_Ultimate_Attack,
    SFX_Ultimate_Damaged
}

/////////////////////////////////



/////////////////////////////////
//////////// Scene //////////////
namespace AFKHero.Scene
{
    //씬 타입
    public enum SceneType
    {
        Title,
        Game
    }
}
/////////////////////////////////



/////////////////////////////////
////////////// UI ///////////////
namespace AFKHero.UI
{
    //메인화면 탭 종류
    public enum UIMainTab
    {
        None = -1,
        Hero,   //영웅
        Battle, //전투
        Shop    //상점
    }

    //영웅 내부의 탭 종류
    public enum UIHeroTab
    {
        Party,   //파티
        Upgrade, //성장
        Share    //공명
    }

    //영웅슬롯 표시 필터
    public enum UIHeroSlotType
    {
        All,        //모든영웅 표시
        SameGrade,  //같은등급 영웅표시
        SameHero    //같은영웅만 표시
    }

    //슬롯 클릭 모드 : 어디에서 눌렀는지 확인용
    public enum UIHeroSlotMode
    {
        Party,           //파티탭 : 정보창 -> 배치
        Upgrade,         //성장탭 : 상단 영웅 선택
        UpgradeMaterial, //성장탭 합성버튼 : 영웅카드 재료 선택
        Share            //공명탭 : 정보창 -> 레벨업
    }
}
////////////////////////////////////



/////////////////////////////////
//////////// Quest //////////////
namespace AFKHero.Quest
{
    //퀘스트 타입
    public enum QuestType
    {
        Daily,   //일일 ( 매일 자정 00:00 초기화 )
        Repeat,  //반복 ( 상시 누적 / 진행도 이월 )
        Main     //메인 ( 초반 가이드 겸 장기 미션 )
    }

    //퀘스트 완료 조건
    public enum QuestConditionType
    {
        DailyLogin,            //일일접속
        HeroSummon,            //영웅 소환
        HeroLevelUp,           //영웅 레벨업
        EnemyKill,             //적 처치
        DailyQuestRewardClaim, //일일퀘스트 보상받기
        StageClear,            //스테이지 클리어
        PartyDeploy,           //영웅 파티 배치
    }

    //퀘스트 보상 타입
    public enum RewardType
    {
        Gold,                  //골드
        Dia,                   //다이아
        FreeTicket             //무료뽑기권
    }

    //메인퀘스트 - 목적지
    public enum GuideTarget
    {
        Party,        //파티
        HeroUpgrade,  //성장 (레벨업)
        HeroSummon,   //영웅소환 (상점)
        Battle,       //전투
        None          //이동없음
    }
}
/////////////////////////////////



/////////////////////////////////
//////////// 가이드 //////////////
public enum GuideStep
{
    None,

    SelectHero,           //영웅 선택

    ClickDeployButton,    //배치 버튼
    SelectDeployPosition, //배치 위치 선택

    ClickLevelUpButton,   //레벨업 버튼

    ClickQuestButton,     //퀘스트 버튼
    ClaimQuestReward,     //퀘스트 보상

    ClickShopButton,      //상점/소환 이동
    ClickSummon,          //소환

    ClickStageStart,      //스테이지 시작 버튼

    Complete
}

/////////////////////////////////