
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

/////////////////////////////////


/////////////////////////////////
////////////// UI ///////////////
namespace AFKHero.UI
{
    //메인화면 탭 종류
    public enum UIMainTab
    {
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
}
/////////////////////////////////