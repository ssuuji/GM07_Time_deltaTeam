using UnityEngine;
using AFKHero.Player;


//패널에 붙여 사용할 디버그용 클래스
//내부의 버튼을 누르면 재화 지급.
public class DebugGiver : MonoBehaviour
{
    public void OnClickedGoldButton()
    {
        if(AFKHero.Player.PlayerManager.Instance != null)
        {
            AFKHero.Player.PlayerManager.Instance.AddGold(500);
        }
    }

    public void OnClickedDiaButton()
    {
        if (AFKHero.Player.PlayerManager.Instance != null)
        {
            AFKHero.Player.PlayerManager.Instance.AddDia(500);
        }
    }

    public void OnClickedTicketButton()
    {
        if (AFKHero.Player.PlayerManager.Instance != null)
        {
            AFKHero.Player.PlayerManager.Instance.AddFreeTicket(1);
        }
    }






}
