using RhythmGame.Core.Analysis;
using RhythmGame.Core.Gameplay;
using RhythmGame.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmGame.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private InputField _urlInput;
        [SerializeField] private Dropdown _difficultyDropdown;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _localFileButton;
        [SerializeField] private Text _errorText;

        private void Start()
        {
            // _playButton이 null이면 자동으로 찾기
            if (_playButton == null)
            {
                _playButton = GameObject.Find("PlayButton")?.GetComponent<Button>();
            }

            if (_playButton != null)
            {
                _playButton.onClick.AddListener(OnPlayClicked);
                Debug.Log("Play 버튼 이벤트 연결 완료!");
            }
            else
            {
                Debug.LogError("Play 버튼을 찾을 수 없습니다!");
            }

            if (_localFileButton != null)
                _localFileButton.onClick.AddListener(OnLocalFileClicked);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnError += ShowError;
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
            }
            else
            {
                Debug.LogWarning("GameManager.Instance가 null입니다!");
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnError -= ShowError;
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            }
        }

        private void OnPlayClicked()
        {
            string url;

            if (_urlInput != null && !string.IsNullOrEmpty(_urlInput.text))
            {
                url = _urlInput.text.Trim();
            }
            else
            {
                url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
                Debug.Log($"URL 입력창이 없어서 테스트 URL 사용: {url}");
            }

            if (!YoutubeUrlParser.IsValidYoutubeUrl(url))
            {
                ShowError("유효한 YouTube URL을 입력해주세요.");
                return;
            }

            ClearError();
            var difficulty = _difficultyDropdown != null ? (Difficulty)_difficultyDropdown.value : Difficulty.Normal;
            Debug.Log($"게임 시작: URL={url}, Difficulty={difficulty}");
            GameManager.Instance.LoadFromYoutube(url, difficulty);
        }

        private void OnLocalFileClicked()
        {
            Debug.Log("로컬 파일 선택 기능 - 구현 필요");
        }

        private void OnGameStateChanged(GameState state)
        {
            gameObject.SetActive(state == GameState.Idle);
        }

        private void ShowError(string message)
        {
            if (_errorText != null)
            {
                _errorText.text = message;
                _errorText.gameObject.SetActive(true);
            }
        }

        private void ClearError()
        {
            if (_errorText != null)
                _errorText.gameObject.SetActive(false);
        }
    }
}
