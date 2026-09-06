//using AFKHero.Quest;
//using DG.Tweening;
//using UnityEngine;
//using UnityEngine.UI;

////보상이 발생했을 때 호출하여, 정해진 위치로 정해진 이미지를 이동시키고 삭제할 클래스
////보내는 위치는 얘만 알면 되고, 시작 위치와 재화를 특정 클래스에서 전달해야 한다.

//public class UIRewardGiver : MonoBehaviour
//{
//    public static UIRewardGiver Instance { get; private set; }

//    [Header("보상 이미지")]
//    [SerializeField] private GameObject goldImage;
//    [SerializeField] private GameObject diaImage;
//    [SerializeField] private GameObject ticketImage;
//    [SerializeField] private GameObject weaponImage;

//    [Header("보낼 위치")]
//    [SerializeField] private RectTransform goldUI;
//    [SerializeField] private RectTransform diaUI;
//    [SerializeField] private RectTransform ticketUI;
//    [SerializeField] private RectTransform weaponUI;


//    private Sequence seq;



//    private void Awake()
//    {
//        if(Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    public void PlayRewardEffect(RewardType reward, Vector3 position)
//    {
//        GameObject rewardImage = null;
//        RectTransform rewardTarget = null;

//        switch (reward)
//        {
//            case RewardType.Gold:
//                rewardImage = goldImage;
//                rewardTarget = goldUI;
//                break;
//            case RewardType.Dia:
//                rewardImage = diaImage;
//                rewardTarget = diaUI;
//                break;
//            case RewardType.FreeTicket:
//                rewardImage = ticketImage;
//                rewardTarget = ticketUI;
//                break;
//            default:
//                break;
//        }

//        if(rewardImage == null || rewardTarget == null)
//        {
//            return;
//        }



//    }

//    private void PlayRewardEffect(GameObject rewardImage, RectTransform rewardTarget, Vector3 position)
//    {
  
//        Vector2 targetPosition = rewardTarget.anchoredPosition;

//        GameObject rewardEffect = Instantiate(rewardImage, position, Quaternion.identity);


//    }


//}
