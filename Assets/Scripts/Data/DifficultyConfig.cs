using UnityEngine;

namespace RhythmGame.Data
{
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "RhythmGame/DifficultyConfig")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Note Generation")]
        [Range(0f, 1f)]
        [Tooltip("노트 생성 확률 (0~1)")]
        public float noteDensity = 0.75f;

        [Range(1, 4)]
        [Tooltip("동시에 생성될 수 있는 최대 노트 수")]
        public int maxSimultaneousNotes = 2;

        [Range(0f, 1f)]
        [Tooltip("홀드 노트 생성 확률")]
        public float holdNoteChance = 0.1f;

        [Range(1, 3)]
        [Tooltip("패턴 복잡도 (1=쉬움, 3=어려움)")]
        public int patternComplexity = 2;

        public static DifficultyConfig CreateDefault(Difficulty difficulty)
        {
            var config = CreateInstance<DifficultyConfig>();
            config.ApplyDifficulty(difficulty);
            return config;
        }

        public void ApplyDifficulty(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    noteDensity = 0.5f;
                    maxSimultaneousNotes = 1;
                    holdNoteChance = 0f;
                    patternComplexity = 1;
                    break;
                case Difficulty.Normal:
                    noteDensity = 0.75f;
                    maxSimultaneousNotes = 2;
                    holdNoteChance = 0.1f;
                    patternComplexity = 2;
                    break;
                case Difficulty.Hard:
                    noteDensity = 1f;
                    maxSimultaneousNotes = 3;
                    holdNoteChance = 0.2f;
                    patternComplexity = 3;
                    break;
            }
        }
    }
}
