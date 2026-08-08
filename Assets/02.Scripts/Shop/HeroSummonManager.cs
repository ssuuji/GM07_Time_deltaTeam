using System;
using System.Collections.Generic;
using UnityEngine;

namespace AFKHero.Shop
{
    //소환제단에서 영웅을 소환한다.
    //소환제단은 레벨과 게이지가 존재하고 영웅 소환으로 게이지를 채워 레벨을 올릴 수 있다.
    //제단의 레벨에 따라 등장 가능한 영웅등급과 확률이 달라진다.
    public class HeroSummonManager : MonoBehaviour
    {
        private int summonLevel = 1; //제단의 레벨
        private int summonExp = 0;   //현재 누적된 게이지
        
        [SerializeField] private List<SummonLevelData> levelData; //레벨별 설정 데이터
        [SerializeField] private List<HeroData> heroList;         //영웅리스트

        public event Action OnSummonInfoChanged; //제단정보 변경 이벤트

        //프로퍼티
        public int SummonLevel => summonLevel;
        public int SummonExp => summonExp;
        public int MaxSummonExp
        {
            get
            {
                SummonLevelData data = GetCurrentLevelData();
                return data.maxExp;
            }
        }

        //현재 소환제단 레벨에 해당하는 데이터 가져오기
        private SummonLevelData GetCurrentLevelData()
        {
            foreach (SummonLevelData data in levelData)
            {
                if (data.level == summonLevel)
                {
                    return data;
                }
            }

            return null;
        }

        //영웅 등급 결정하기
        private HeroGrade GetHeroGrade()
        {
            SummonLevelData currentData = GetCurrentLevelData(); //소환레벨 가져오고
            float random = UnityEngine.Random.Range(0f, 100f);

            // 0 ~ 에픽 구간이면 Epic
            if (random < currentData.epicRate)
            {
                return HeroGrade.Epic;
            }
            //에픽 ~ 레어 구간이면 레어
            if (random < currentData.epicRate + currentData.rareRate)
            {
                return HeroGrade.Rare;
            }
            //레어 ~ 노멀 구간이면 노멀
            return HeroGrade.Normal;
        }

        //결정된 등급에서 영웅 1명 뽑기
        private HeroData GetRandomHero(HeroGrade grade)
        {
            List<HeroData> gradeHeroes = new List<HeroData>(); //영웅 리스트 

            foreach (HeroData hero in heroList)
            {
                if (hero.HeroGrade == grade)
                {
                    gradeHeroes.Add(hero);  //영웅의 등급이 결정된 등급과 같으면 추가
                }
            }

            int randomIndex = UnityEngine.Random.Range(0, gradeHeroes.Count); //랜덤 뽑기
            return gradeHeroes[randomIndex];                                  //영웅 반환
        }

        //소환
        public List<HeroData> Summon(int count)
        {
            List<HeroData> result = new List<HeroData>(); //소환된 영웅리스트

            for (int i = 0; i < count; i++)
            {
                HeroGrade grade = GetHeroGrade();     //영웅의 등급을 결정하고
                HeroData hero = GetRandomHero(grade); //결정된 등급에서 영웅 1명 소환
                result.Add(hero); //추가
                summonExp++;      //게이지 증가
                CheckLevelUp();   //레벨 체크
            }

            OnSummonInfoChanged?.Invoke(); //소환후 제단정보 변경 이벤트
            return result;
        }

        //레벨 체크
        private void CheckLevelUp()
        {
            SummonLevelData currentData = GetCurrentLevelData(); //제단의 레벨데이터 가져오기

            if (summonExp < currentData.maxExp) return;

            if (summonLevel >= levelData.Count)
            {
                summonExp = currentData.maxExp; //마지막 레벨이면 레벨과 게이지 고정
                return;
            }

            summonExp -= currentData.maxExp; //남은 게이지는 다음레벨로 넘기기
            summonLevel++;                   //제단 레벨업
        }
    }
}

