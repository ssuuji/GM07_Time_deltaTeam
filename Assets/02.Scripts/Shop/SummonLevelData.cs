using System;

namespace AFKHero.Shop
{
    [Serializable]
    public class SummonLevelData
    {
        public int level;        //제단 레벨
        public int maxExp;       //레벨별 max게이지

        public float normalRate; //노말카드 확률
        public float rareRate;   //레어카드 확률
        public float epicRate;   //에픽카드 확률
    }
}

