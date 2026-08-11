using TMPro;
using UnityEngine;

namespace AFKHero.UI
{
    public class UISynergy : MonoBehaviour
    {
        [Header("파티화면 시너지")]
        [SerializeField] private TMP_Text humanCountText;
        [SerializeField] private TMP_Text elfCountText;
        [SerializeField] private TMP_Text orcCountText;
        [SerializeField] private TMP_Text undeadCountText;

        [Header("전투화면 시너지")]
        [SerializeField] private TMP_Text humanCountTextb;
        [SerializeField] private TMP_Text elfCountTextb;
        [SerializeField] private TMP_Text orcCountTextb;
        [SerializeField] private TMP_Text undeadCountTextb;

        //시너지 UI 갱신
        public void UpdateUI()
        {
            int humanCount = 0, elfCount = 0, orcCount = 0, undeadCount = 0;

            foreach (HeroInstance hero in PartyManager.Instance.partySlots)
            {
                if (hero == null || hero.data == null) continue;

                switch (hero.data.RaceType)
                {
                    case RaceType.Human: humanCount++; break;
                    case RaceType.Elf: elfCount++; break;
                    case RaceType.Orc: orcCount++; break;
                    case RaceType.Undead: undeadCount++; break;
                }
            }

            //파티화면
            SetSynergyText(humanCountText, humanCount);
            SetSynergyText(elfCountText, elfCount);
            SetSynergyText(orcCountText, orcCount);
            SetSynergyText(undeadCountText, undeadCount);

            //전투화면
            SetSynergyText(humanCountTextb, humanCount);
            SetSynergyText(elfCountTextb, elfCount);
            SetSynergyText(orcCountTextb, orcCount);
            SetSynergyText(undeadCountTextb, undeadCount);
        }

        //텍스트 색입히기
        private void SetSynergyText(TMP_Text text, int count)
        {
            if (count >= 3)
            {
                text.text = "3 / 3";
                text.color = new Color32(255, 138, 36, 255); //주황색
            }
            else if (count >= 2)
            {
                text.text = "2 / 2";
                text.color = new Color32(237, 209, 52, 255); //노란색
            }
            else
            {
                text.text = $"{count} / 2";
                text.color = new Color32(168, 143, 118, 255); //갈색
            }
        }


    }
}

