using System;
using UnityEngine;

namespace AFKHero.Player
{
    //플레이어의 정보와 재화관리
    //화면 상단에 아이콘, 이름, 파티 공격력, 골드, 다이아 를 표시함 ( + 무료뽑기권 까지..? )
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }

        [Header("Profile")]
        [SerializeField] private Sprite playerIcon; //플레이어 아이콘 //추후에.. 플레이어설정 같은 창을 따로 만들어서 프로필아이콘을 설정 할 수 있도록 확장해볼계획
        [SerializeField] private string playerName; //이름           // + 이름도 직접 정해서 저장할 수 있도록

        [Header("Currency")]
        [SerializeField] private int gold = 0;       //골드
        [SerializeField] private int dia = 0;        //다이아
        [SerializeField] private int freeTicket = 0; //무료뽑기권

        [Header("Power")]
        [SerializeField] private int partyPower = 0; //현재 배치된 파티의 공격력

        public event Action OnPlayerInfoChanged; //플레이어 정보변경 이벤트

        //프로퍼티
        public Sprite PlayerIcon => playerIcon;
        public string PlayerName => playerName;
        public int Gold => gold;
        public int Dia => dia;
        public int FreeTicket => freeTicket;
        public int PartyPower => partyPower;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region 다이아

        //추가
        public void AddDia(int amount)
        {
            dia += amount;

            OnPlayerInfoChanged?.Invoke();
        }

        //사용
        public bool TryUseDia(int amount)
        {
            if (amount <= 0) return false;  //0이하면 f
            if (dia < amount) return false; //보유중인 다이아가 적으면 f

            dia -= amount;

            OnPlayerInfoChanged?.Invoke();
            return true; //소모완료
        }

        #endregion

        #region 골드

        //추가
        public void AddGold(int amount)
        {
            gold += amount;

            OnPlayerInfoChanged?.Invoke();
        }

        //사용
        public bool TryUseGold(int amount)
        {
            if (amount <= 0) return false;  //0이하면 f
            if (gold < amount) return false; //보유중인 골드가 적으면 f

            gold -= amount;

            OnPlayerInfoChanged?.Invoke();
            return true; //소모완료
        }
        #endregion

        #region 무료뽑기권

        //추가
        public void AddFreeTicket(int amount)
        {
            freeTicket += amount;

            OnPlayerInfoChanged?.Invoke();
        }

        //사용 ( 일단 한장씩.. 만 뽑게 되어있어서 amount 뺌 )
        public bool TryUseFreeTicket()
        {
            if (freeTicket <= 0) return false; //보유중인 뽑기권이 없으면 f

            freeTicket--;

            OnPlayerInfoChanged?.Invoke();
            return true; //소모완료
        }

        #endregion

        #region 프로필( 아이콘 , 이름 )

        //아이콘 변경
        public void ChangePlayerIcon(Sprite newIcon)
        {
            if (newIcon == null) return;

            playerIcon = newIcon;

            OnPlayerInfoChanged?.Invoke();
        }

        //이름 변경
        public void ChangePlayerName(string newName)
        {
            playerName = newName;

            OnPlayerInfoChanged?.Invoke();
        }

        #endregion
    }
}

