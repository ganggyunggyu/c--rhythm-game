using System;
using RhythmGame.Data;
using UnityEngine;

namespace RhythmGame.Core.Gameplay
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private GameSettings _settings;
        [SerializeField] private JudgeController _judgeController;

        private int _score;
        private int _combo;
        private int _maxCombo;
        private int _perfectCount;
        private int _greatCount;
        private int _goodCount;
        private int _missCount;
        private int _totalNotes;

        public int Score => _score;
        public int Combo => _combo;
        public int MaxCombo => _maxCombo;
        public float Accuracy => _totalNotes > 0
            ? (float)(_perfectCount * 100 + _greatCount * 80 + _goodCount * 50) / (_totalNotes * 100)
            : 0f;

        public int PerfectCount => _perfectCount;
        public int GreatCount => _greatCount;
        public int GoodCount => _goodCount;
        public int MissCount => _missCount;

        public event Action OnScoreChanged;
        public event Action OnComboChanged;

        private void OnEnable()
        {
            if (_judgeController != null)
                _judgeController.OnJudge += HandleJudge;
        }

        private void OnDisable()
        {
            if (_judgeController != null)
                _judgeController.OnJudge -= HandleJudge;
        }

        public void Initialize(int totalNotes)
        {
            _totalNotes = totalNotes;
            Reset();
        }

        public void Reset()
        {
            _score = 0;
            _combo = 0;
            _maxCombo = 0;
            _perfectCount = 0;
            _greatCount = 0;
            _goodCount = 0;
            _missCount = 0;
        }

        private void HandleJudge(JudgeResult result, Note note)
        {
            var baseScore = result switch
            {
                JudgeResult.Perfect => _settings?.perfectScore ?? 1000,
                JudgeResult.Great => _settings?.greatScore ?? 800,
                JudgeResult.Good => _settings?.goodScore ?? 500,
                _ => 0
            };

            switch (result)
            {
                case JudgeResult.Perfect:
                    _perfectCount++;
                    _combo++;
                    break;
                case JudgeResult.Great:
                    _greatCount++;
                    _combo++;
                    break;
                case JudgeResult.Good:
                    _goodCount++;
                    _combo++;
                    break;
                case JudgeResult.Miss:
                    _missCount++;
                    _combo = 0;
                    break;
            }

            // 콤보 보너스
            var comboMultiplier = 1f + (_combo / 50f) * 0.1f;
            _score += Mathf.RoundToInt(baseScore * comboMultiplier);

            if (_combo > _maxCombo)
                _maxCombo = _combo;

            OnScoreChanged?.Invoke();
            OnComboChanged?.Invoke();
        }

        public string GetRank()
        {
            var acc = Accuracy;
            if (acc >= 0.98f && _missCount == 0) return "SS";
            if (acc >= 0.95f) return "S";
            if (acc >= 0.90f) return "A";
            if (acc >= 0.80f) return "B";
            if (acc >= 0.70f) return "C";
            return "D";
        }

        public ResultData GetResult()
        {
            return new ResultData
            {
                score = _score,
                maxCombo = _maxCombo,
                accuracy = Accuracy,
                rank = GetRank(),
                perfectCount = _perfectCount,
                greatCount = _greatCount,
                goodCount = _goodCount,
                missCount = _missCount
            };
        }
    }

    [Serializable]
    public class ResultData
    {
        public int score;
        public int maxCombo;
        public float accuracy;
        public string rank;
        public int perfectCount;
        public int greatCount;
        public int goodCount;
        public int missCount;
    }
}
