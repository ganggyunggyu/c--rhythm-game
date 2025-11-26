using RhythmGame.Core.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmGame.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _comboText;
        [SerializeField] private TMP_Text _accuracyText;

        [Header("Judge Display")]
        [SerializeField] private TMP_Text _judgeText;
        [SerializeField] private float _judgeFadeTime = 0.5f;

        [Header("Progress")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TMP_Text _timeText;

        [Header("References")]
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private SongController _songController;
        [SerializeField] private JudgeController _judgeController;

        private float _judgeDisplayTimer;
        private Color _judgeColor;

        private void Start()
        {
            if (_scoreManager != null)
            {
                _scoreManager.OnScoreChanged += UpdateScoreDisplay;
                _scoreManager.OnComboChanged += UpdateComboDisplay;
            }

            if (_judgeController != null)
                _judgeController.OnJudge += ShowJudge;

            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            if (_scoreManager != null)
            {
                _scoreManager.OnScoreChanged -= UpdateScoreDisplay;
                _scoreManager.OnComboChanged -= UpdateComboDisplay;
            }

            if (_judgeController != null)
                _judgeController.OnJudge -= ShowJudge;

            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        private void Update()
        {
            UpdateProgress();
            UpdateJudgeFade();
        }

        private void UpdateScoreDisplay()
        {
            if (_scoreText != null)
                _scoreText.text = _scoreManager.Score.ToString("N0");

            if (_accuracyText != null)
                _accuracyText.text = $"{_scoreManager.Accuracy * 100:F2}%";
        }

        private void UpdateComboDisplay()
        {
            if (_comboText != null)
            {
                _comboText.text = _scoreManager.Combo > 0
                    ? $"{_scoreManager.Combo} COMBO"
                    : "";
            }
        }

        private void ShowJudge(JudgeResult result, Note note)
        {
            if (_judgeText == null)
                return;

            _judgeText.text = result.ToString().ToUpper();
            _judgeColor = result switch
            {
                JudgeResult.Perfect => Color.yellow,
                JudgeResult.Great => Color.green,
                JudgeResult.Good => Color.cyan,
                JudgeResult.Miss => Color.red,
                _ => Color.white
            };

            _judgeText.color = _judgeColor;
            _judgeDisplayTimer = _judgeFadeTime;
        }

        private void UpdateJudgeFade()
        {
            if (_judgeText == null || _judgeDisplayTimer <= 0)
                return;

            _judgeDisplayTimer -= Time.deltaTime;
            var alpha = Mathf.Clamp01(_judgeDisplayTimer / _judgeFadeTime);
            _judgeText.color = new Color(_judgeColor.r, _judgeColor.g, _judgeColor.b, alpha);
        }

        private void UpdateProgress()
        {
            if (_songController == null)
                return;

            if (_progressBar != null)
                _progressBar.value = _songController.NormalizedTime;

            if (_timeText != null)
            {
                var current = _songController.SongTime;
                var total = _songController.SongLength;
                _timeText.text = $"{FormatTime(current)} / {FormatTime(total)}";
            }
        }

        private string FormatTime(float seconds)
        {
            var mins = Mathf.FloorToInt(seconds / 60);
            var secs = Mathf.FloorToInt(seconds % 60);
            return $"{mins:D2}:{secs:D2}";
        }

        private void OnGameStateChanged(GameState state)
        {
            gameObject.SetActive(state == GameState.Playing || state == GameState.Paused);
        }
    }
}
