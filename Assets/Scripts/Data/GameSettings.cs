using UnityEngine;

namespace RhythmGame.Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "RhythmGame/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Header("판정 설정 (ms)")]
        public float perfectWindow = 50f;
        public float greatWindow = 100f;
        public float goodWindow = 150f;

        [Header("점수 설정")]
        public int perfectScore = 1000;
        public int greatScore = 800;
        public int goodScore = 500;

        [Header("게임플레이 설정")]
        public float noteSpawnLeadTime = 2f;  // 노트가 미리 스폰되는 시간 (초)
        public float noteSpeed = 5f;          // 노트 낙하 속도
        public int laneCount = 4;             // 레인 개수

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
    }
}
