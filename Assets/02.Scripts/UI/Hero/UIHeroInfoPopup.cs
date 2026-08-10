using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AFKHero.UI
{
    public class UIHeroInfoPopup : MonoBehaviour
    {
        [SerializeField] private GameObject heroInfoPopup;

        [Header("영웅 정보")]
        [SerializeField] private Image heroImage;
        [SerializeField] private TMP_Text heroNametext;
        [SerializeField] private TMP_Text heroGradetext;

    }
}
