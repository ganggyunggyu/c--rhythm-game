using UnityEngine;

namespace RhythmGame.Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "RhythmGame/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        private static GameSettings _default;
        public static GameSettings Default => _default ??= CreateDefaultSettings();

        [Header("판정 설정 (ms)")]
        public float perfectWindow = 50f;
        public float greatWindow = 100f;
        public float goodWindow = 150f;

        [Header("점수 설정")]
        public int perfectScore = 1000;
        public int greatScore = 800;
        public int goodScore = 500;

        [Header("게임플레이 설정")]
        public float noteSpawnLeadTime = 2f;
        public float noteSpeed = 5f;
        public int laneCount = 4;

        [Header("입력 키 설정")]
        public KeyCode[] laneKeys = new KeyCode[]
        {
            KeyCode.D,
            KeyCode.F,
            KeyCode.J,
            KeyCode.K
        };

        [Header("캐시 설정")]
        public string cacheFolder = "cache";

        private static GameSettings CreateDefaultSettings()
        {
            var settings = CreateInstance<GameSettings>();
            settings.perfectWindow = 50f;
            settings.greatWindow = 100f;
            settings.goodWindow = 150f;
            settings.perfectScore = 1000;
            settings.greatScore = 800;
            settings.goodScore = 500;
            settings.noteSpawnLeadTime = 2f;
            settings.noteSpeed = 5f;
            settings.laneCount = 4;
            settings.laneKeys = new[] { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };
            settings.cacheFolder = "cache";
            return settings;
        }

        public static GameSettings GetOrDefault(GameSettings settings) => settings != null ? settings : Default;
    }
}
