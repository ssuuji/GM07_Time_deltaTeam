using UnityEngine;

[System.Serializable]
public class HeroInstance
{
    public HeroData data;      // 이 영웅의 원본 데이터(이름, 직업, 프리팹 등)를 연결해 둡니다.
    public int level;         // 영웅의 현재 레벨입니다. (기본값 1, 최대 10)
    public bool isUnlocked;    // 플레이어가 이 영웅을 획득(해금)했는지 여부입니다.

    public HeroInstance(HeroData heroData, bool defaultUnlocked = false)
    {
        data = heroData;
        level = 1; 
        isUnlocked = defaultUnlocked; 
    }

    public int MaxHP => data.GetJobStats().hp + (level - 1) * 20;

    // 공격력: 기본 공격력 + (현재 레벨 - 1) * 5
    public int Attack => data.GetJobStats().attack + (level - 1) * 5;

    // 방어력: 기본 방어력 + (현재 레벨 - 1) * 2
    public int Defense => data.GetJobStats().defense + (level - 1) * 2;
    public float AttackSpeed => data.GetJobStats().attackSpeed;
    public float AttackRange => data.GetJobStats().attackRange;


    // ==========================================
    // 영웅 성장(레벨업) 기능
    // ==========================================
    public bool LevelUp()
    {
        // 현재 레벨이 기획서의 최대 레벨인 10 이상인지 확인
        if (level >= 10)
        {
            // 최대 레벨이면 유니티 콘솔에 경고 메시지를 띄우고 실패(false)를 반환
            Debug.LogWarning($"{data.HeroName}은(는) 이미 최고 레벨(10)입니다.");
            return false;
        }

        // 최대 레벨이 아니라면 레벨 숫자를 1 올리기
        level++;

        // 레벨업 성공(true)을 반환합니다.
        return true;
    }
}
