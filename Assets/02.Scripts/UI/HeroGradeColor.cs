using UnityEngine;

namespace AFKHero.UI
{
    public static class HeroGradeColor
    {
        //등급별 색 미리 지정
        public static Color32 GetColor(HeroGrade grade)
        {
            switch (grade)
            {
                //노멀, 노멀+
                case HeroGrade.Normal:
                case HeroGrade.NormalPlus:
                    return new Color32(160, 160, 160, 255); //회색
                                                            //레어, 레어+
                case HeroGrade.Rare:
                case HeroGrade.RarePlus:
                    return new Color32(61, 141, 255, 255);  //파랑
                                                            //에픽, 에픽+
                case HeroGrade.Epic:
                case HeroGrade.EpicPlus:
                    return new Color32(176, 76, 255, 255);  //보라

                default:
                    return Color.white;
            }
        }
    }
}
