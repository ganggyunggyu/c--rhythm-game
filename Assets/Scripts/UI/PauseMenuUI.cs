using RhythmGame.Core.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmGame.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;

        private void Start()
        {
            _resumeButton?.onClick.AddListener(OnResumeClicked);
            _restartButton?.onClick.AddListener(OnRestartClicked);
            _quitButton?.onClick.AddListener(OnQuitClicked);

            Debug.Assert(_resumeButton != null, "Resume 버튼이 연결되지 않았습니다");
            Debug.Assert(_restartButton != null, "Restart 버튼이 연결되지 않았습니다");
            Debug.Assert(_quitButton != null, "Quit 버튼이 연결되지 않았습니다");

            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnGameStateChanged;

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        private void OnResumeClicked()
        {
            GameManager.Instance.ResumeGame();
        }

        private void OnRestartClicked()
        {
            GameManager.Instance.RestartGame();
            GameManager.Instance.StartGame();
        }

        private void OnQuitClicked()
        {
            GameManager.Instance.ReturnToMenu();
        }

        private void OnGameStateChanged(GameState state)
        {
            gameObject.SetActive(state == GameState.Paused);
        }
    }
}
