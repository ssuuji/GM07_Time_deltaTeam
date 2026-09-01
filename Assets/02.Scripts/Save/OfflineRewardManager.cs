using AFKHeroPlayerManager = AFKHero.Player.PlayerManager;
using System;
using System.Collections.Generic;
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

        // 오프라인 동안 쌓인 장비들을 임시로 담아둘 리스트
        public List<EquipmentInstance> rewardEquipments = new List<EquipmentInstance>();

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

            // ==========================================
            // 1시간당 장비 1~4개 랜덤 드랍 계산
            // ==========================================
            int offlineHours = offlineMinutes / 60; // 몇 시간 지났는지 계산
            int equipDropCount = 0;

            for (int i = 0; i < offlineHours; i++)
            {
                equipDropCount += UnityEngine.Random.Range(1, 5); // 1시간마다 1~4개 누적
            }

            // Resources/Equipments 폴더 안의 모든 장비 원본 데이터를 가져옵니다.
            EquipmentData[] allEquips = Resources.LoadAll<EquipmentData>("Equipments");

            if (allEquips.Length > 0 && equipDropCount > 0)
            {
                for (int i = 0; i < equipDropCount; i++)
                {
                    // 랜덤으로 하나 뽑아서 인스턴스 생성 후 리스트에 담기
                    EquipmentData randomData = allEquips[UnityEngine.Random.Range(0, allEquips.Length)];
                    rewardEquipments.Add(new EquipmentInstance(randomData));
                }
            }
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
            // 장비 보상이 있을 수도 있으니 조건식 변경
            if (rewardGold <= 0 && rewardEquipments.Count == 0) return;

            AFKHeroPlayerManager.Instance.AddGold(rewardGold); //방치 골드 지급

            // 보관해둔 장비들을 실제 가방에 넣기
            if (EquipmentManager.Instance != null)
            {
                foreach (var equip in rewardEquipments)
                {
                    EquipmentManager.Instance.AddEquipment(equip);
                }
            }

            rewardGold = 0; //수령 후 초기화
            rewardEquipments.Clear(); // 수령 후 장비 리스트도 싹 비워주기

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