using AFKHeroPlayerManager = AFKHero.Player.PlayerManager;
using System;
using UnityEngine;

namespace AFKHero.Save
{
    public class OfflineRewardManager : MonoBehaviour
    {
        public static OfflineRewardManager Instance { get; private set; }

        private DateTime lastOnlineTime;           //마지막 저장 시간
        private const int maxOfflineMinutes = 240; //최대 방치 시간 4시간
        private int offlineMinutes;                //방치 시간
        private int rewardGold;                    //방치 보상 골드

        public int OfflineMinutes => offlineMinutes;
        public int RewardGold => rewardGold;

        private void Awake()
        {
            Instance = this;
        }

        //방치 보상 계산
        public void CalculateOfflineReward(int stageNumber, int sectionNumber)
        {
            DateTime currentTime = DateTime.Now; //현재 시간

            TimeSpan elapsedTime = currentTime - lastOnlineTime; //접속하지 않은 시간

            offlineMinutes = (int)elapsedTime.TotalMinutes; //경과 시간(분)

            if (offlineMinutes < 0)
            {
                offlineMinutes = 0;
            }

            if (offlineMinutes > maxOfflineMinutes)
            {
                offlineMinutes = maxOfflineMinutes;
            }

            int goldPerMinute = CalculateGoldPerMinute(stageNumber, sectionNumber); //분당 골드
            int calculatedGold = offlineMinutes * goldPerMinute; //이번 방치 골드

            rewardGold += calculatedGold; //방치 보상 골드에 추가
        }

        //분당 방치 골드 계산
        private int CalculateGoldPerMinute(int stageNumber, int sectionNumber)
        {
            /*
                스테이지        | 분당 골드
                1-1               100
                1-2               120
                1-3               140
                1-4               160
                1-5               180

                2-1               200
                2-2               220

                ... 

                기본 분당 골드     : 100
                섹션 1 증가 시     : +20골드
                스테이지 1 증가 시 : +100골드
            */
            int baseGold = 100;                          //기본 골드
            int stageBonus = (stageNumber - 1) * 100;    //스테이지 증가 보상
            int sectionBonus = (sectionNumber - 1) * 20; //섹션 증가 보상

            return baseGold + stageBonus + sectionBonus;
        }

        //방치 보상 수령
        public void ClaimReward()
        {
            if (rewardGold <= 0) return;

            AFKHeroPlayerManager.Instance.AddGold(rewardGold); //방치 골드 지급

            rewardGold = 0; //수령 후 초기화

            GameSaveManager.Instance.SaveGame();
        }

        #region 저장/불러오기

        //방치 보상 저장 데이터 생성
        public OfflineRewardSaveData CreateOfflineRewardSaveData()
        {
            lastOnlineTime = DateTime.Now;

            OfflineRewardSaveData saveData = new OfflineRewardSaveData();

            saveData.lastOnlineTime = lastOnlineTime.ToString("O");
            saveData.rewardGold = rewardGold;

            return saveData;
        }

        //방치 보상 데이터 불러오기
        public void LoadOfflineRewardSaveData(OfflineRewardSaveData saveData)
        {
            if (saveData == null || string.IsNullOrEmpty(saveData.lastOnlineTime))
            {
                lastOnlineTime = DateTime.Now;
                rewardGold = 0;
                return;
            }

            if (!DateTime.TryParse(saveData.lastOnlineTime, out lastOnlineTime))
            {
                lastOnlineTime = DateTime.Now; //저장된 시간이 datetime으로 변환 실패하면 현재 시간 넣어주기
            }

            rewardGold = saveData.rewardGold;
        }
        #endregion
    }
}
