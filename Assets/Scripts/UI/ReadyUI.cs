using RhythmGame.Core.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmGame.UI
{
    public class ReadyUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _songInfoText;
        [SerializeField] private TMP_Text _bpmText;
        [SerializeField] private TMP_Text _noteCountText;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _backButton;

        private void Start()
        {
            _startButton?.onClick.AddListener(OnStartClicked);
            _backButton?.onClick.AddListener(OnBackClicked);

            Debug.Assert(_startButton != null, "Start 버튼이 연결되지 않았습니다");
            Debug.Assert(_backButton != null, "Back 버튼이 연결되지 않았습니다");

            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnGameStateChanged;

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        private void OnStartClicked()
        {
            GameManager.Instance.StartGame();
        }

        private void OnBackClicked()
        {
            GameManager.Instance.ReturnToMenu();
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Ready)
            {
                ShowChartInfo();
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void ShowChartInfo()
        {
            var chart = GameManager.Instance.CurrentChart;
            if (chart == null)
                return;

            if (_songInfoText != null)
                _songInfoText.text = $"Video ID: {chart.videoId}";

            if (_bpmText != null)
                _bpmText.text = $"BPM: {chart.bpm:F0}";

            if (_noteCountText != null)
                _noteCountText.text = $"Notes: {chart.notes.Count}";
        }
    }
}
