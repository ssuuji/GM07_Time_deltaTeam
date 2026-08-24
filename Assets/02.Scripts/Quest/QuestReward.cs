using System;
using UnityEngine;


namespace AFKHero.Quest
{
    [Serializable]
    public class QuestReward
    {
        [SerializeField] private RewardType rewardType; //퀘스트 보상 타입
        [SerializeField] private int amount;            //갯수

        public RewardType RewardType => rewardType;     //퀘스트 보상 타입
        public int Amount => amount;                    //갯수
    }
}

