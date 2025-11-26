using RhythmGame.Core.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmGame.UI
{
    public class ResultUI : MonoBehaviour
    {
        [Header("Result Display")]
        [SerializeField] private TMP_Text _rankText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _accuracyText;
        [SerializeField] private TMP_Text _maxComboText;

        [Header("Judge Counts")]
        [SerializeField] private TMP_Text _perfectCountText;
        [SerializeField] private TMP_Text _greatCountText;
        [SerializeField] private TMP_Text _goodCountText;
        [SerializeField] private TMP_Text _missCountText;

        [Header("Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _menuButton;

        private void Start()
        {
            _retryButton?.onClick.AddListener(OnRetryClicked);
            _menuButton?.onClick.AddListener(OnMenuClicked);

            Debug.Assert(_retryButton != null, "Retry 버튼이 연결되지 않았습니다");
            Debug.Assert(_menuButton != null, "Menu 버튼이 연결되지 않았습니다");

            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnGameStateChanged;

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        private void OnRetryClicked()
        {
            GameManager.Instance.RestartGame();
            GameManager.Instance.StartGame();
        }

        private void OnMenuClicked()
        {
            GameManager.Instance.ReturnToMenu();
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Result)
            {
                ShowResult();
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void ShowResult()
        {
            var result = GameManager.Instance.GetResult();

            if (_rankText != null)
            {
                _rankText.text = result.rank;
                _rankText.color = GetRankColor(result.rank);
            }

            if (_scoreText != null)
                _scoreText.text = result.score.ToString("N0");

            if (_accuracyText != null)
                _accuracyText.text = $"{result.accuracy * 100:F2}%";

            if (_maxComboText != null)
                _maxComboText.text = result.maxCombo.ToString();

            if (_perfectCountText != null)
                _perfectCountText.text = result.perfectCount.ToString();

            if (_greatCountText != null)
                _greatCountText.text = result.greatCount.ToString();

            if (_goodCountText != null)
                _goodCountText.text = result.goodCount.ToString();

            if (_missCountText != null)
                _missCountText.text = result.missCount.ToString();
        }

        private Color GetRankColor(string rank)
        {
            return rank switch
            {
                "SS" => new Color(1f, 0.84f, 0f),
                "S" => new Color(1f, 0.65f, 0f),
                "A" => Color.green,
                "B" => Color.cyan,
                "C" => Color.white,
                _ => Color.gray
            };
        }
    }
}
