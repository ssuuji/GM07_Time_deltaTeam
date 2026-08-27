using AFKHeroPlayerManager = AFKHero.Player.PlayerManager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AFKHero.Collection
{
    public class CollectionManager : MonoBehaviour
    {
        public static CollectionManager Instance { get; private set; }

        private HashSet<int> collectedHeroIDs = new HashSet<int>();    //도감에 등록된 영웅 ID
        private HashSet<int> claimedRewardCounts = new HashSet<int>(); //이미 수령한 수집 보상

        public event Action OnCollectionChanged;

        public int CollectedCount => collectedHeroIDs.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        #region 영웅 도감

        //영웅 최초 획득 시 호출
        public void RegisterHero(int heroID)
        {
            if (collectedHeroIDs.Contains(heroID)) return; //이미 도감에 등록되어 있다면 무시

            collectedHeroIDs.Add(heroID);
            
            OnCollectionChanged?.Invoke();
        }

        //해당 영웅이 도감에 등록되어 있는지
        public bool IsCollected(int heroID)
        {
            return collectedHeroIDs.Contains(heroID);
        }

        #endregion

        #region 수집 보상

        //해당 수집 보상을 받을 수 있는지
        public bool CanClaimReward(int requiredCount)
        {
            if (CollectedCount < requiredCount) return false; //아직 필요한 수만큼 모으지 못함
            if (claimedRewardCounts.Contains(requiredCount)) return false; //이미 보상을 받음

            return true;
        }

        //해당 수집 보상을 이미 받았는지
        public bool IsRewardClaimed(int requiredCount)
        {
            return claimedRewardCounts.Contains(requiredCount);
        }

        //수집 보상 받기
        public void ClaimReward(int requiredCount)
        {
            if (!CanClaimReward(requiredCount)) return;

            claimedRewardCounts.Add(requiredCount);
            GiveCollectionReward(requiredCount);

            OnCollectionChanged?.Invoke();
        }

        //실제 보상 지급
        private void GiveCollectionReward(int requiredCount)
        {
            switch (requiredCount)
            {
                case 4:  AFKHeroPlayerManager.Instance?.AddDia(100); break;
                case 8:  AFKHeroPlayerManager.Instance?.AddDia(200); break;
                case 12: AFKHeroPlayerManager.Instance?.AddDia(300); break;
                case 16: AFKHeroPlayerManager.Instance?.AddDia(500); break;
                case 24: AFKHeroPlayerManager.Instance?.AddDia(800); break;
                case 32: AFKHeroPlayerManager.Instance?.AddDia(1500); break;
            }
        }

        #endregion

        #region 저장 / 불러오기

        //도감 저장 데이터 생성
        public CollectionSaveData GetCollectionSaveData()
        {
            return new CollectionSaveData
            {
                collectedHeroIDs = new List<int>(collectedHeroIDs),
                claimedRewardCounts = new List<int>(claimedRewardCounts)
            };
        }

        //도감 저장 데이터 불러오기
        public void LoadCollectionSaveData(CollectionSaveData saveData)
        {
            collectedHeroIDs.Clear();
            claimedRewardCounts.Clear();

            if (saveData == null)
            {
                OnCollectionChanged?.Invoke();
                return;
            }

            if (saveData.collectedHeroIDs != null)
            {
                foreach (int heroID in saveData.collectedHeroIDs)
                {
                    collectedHeroIDs.Add(heroID);
                }
            }

            if (saveData.claimedRewardCounts != null)
            {
                foreach (int rewardCount in saveData.claimedRewardCounts)
                {
                    claimedRewardCounts.Add(rewardCount);
                }
            }

            OnCollectionChanged?.Invoke();
        }

        #endregion
    }
}