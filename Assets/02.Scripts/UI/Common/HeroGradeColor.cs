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
                
                case HeroGrade.Normal:
                case HeroGrade.NormalPlus:                  //노멀, 노멀+
                    return new Color32(160, 160, 160, 255); //회색
                                                            
                case HeroGrade.Rare:
                case HeroGrade.RarePlus:                    //레어, 레어+
                    return new Color32(61, 141, 255, 255);  //파랑
                                                            
                case HeroGrade.Epic:
                case HeroGrade.EpicPlus:                    //에픽, 에픽+
                    return new Color32(176, 76, 255, 255);  //보라

                default:
                    return Color.white;
            }
        }
    }
}
