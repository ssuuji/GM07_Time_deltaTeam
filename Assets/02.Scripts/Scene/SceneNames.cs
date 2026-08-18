using AFKHero.Scene;
using System.Collections.Generic;

namespace AFKHero.Scene
{
    //씬 이름을 관리하는 정적 클래스
    public static class SceneNames
    {
        private static readonly Dictionary<SceneType, string> sceneTable = new Dictionary<SceneType, string>()
        {
            { SceneType.Title, "Title" },
            { SceneType.Game, "Game" }
        };

        public static string GetSceneName(SceneType sceneType)
        {
            return sceneTable[sceneType];
        }
    }
}