using System;
using RhythmGame.Core.Analysis;
using RhythmGame.Data;
using UnityEngine;

namespace RhythmGame.Core.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameStateManager _stateManager;
        [SerializeField] private ChartLoadingService _loadingService;
        [SerializeField] private GameFlowController _flowController;
        [SerializeField] private ScoreManager _scoreManager;

        public GameState State => _stateManager?.State ?? GameState.Idle;
        public ChartData CurrentChart => _loadingService?.CurrentChart;
        public float LoadProgress => _loadingService?.LoadProgress ?? 0f;

        private event Action<GameState> _onStateChanged;
        private event Action<float> _onLoadProgressChanged;
        private event Action<string> _onError;

        public event Action<GameState> OnStateChanged
        {
            add
            {
                _onStateChanged += value;
                if (_stateManager != null)
                    _stateManager.OnStateChanged += value;
            }
            remove
            {
                _onStateChanged -= value;
                if (_stateManager != null)
                    _stateManager.OnStateChanged -= value;
            }
        }

        public event Action<float> OnLoadProgressChanged
        {
            add
            {
                _onLoadProgressChanged += value;
                if (_loadingService != null)
                    _loadingService.OnLoadProgressChanged += value;
            }
            remove
            {
                _onLoadProgressChanged -= value;
                if (_loadingService != null)
                    _loadingService.OnLoadProgressChanged -= value;
            }
        }

        public event Action<string> OnError
        {
            add
            {
                _onError += value;
                if (_loadingService != null)
                    _loadingService.OnError += value;
            }
            remove
            {
                _onError -= value;
                if (_loadingService != null)
                    _loadingService.OnError -= value;
            }
        }

        private void Awake()
        {
            Debug.Log("[GameManager] Awake 시작");

            if (Instance != null && Instance != this)
            {
                Debug.Log("[GameManager] 이미 Instance 존재, 파괴");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameManager] Instance 설정 완료");

            EnsureComponents();
            SetupEventHandlers();
            Debug.Log("[GameManager] Awake 완료");
        }

        private void EnsureComponents()
        {
            if (_stateManager == null)
                _stateManager = gameObject.AddComponent<GameStateManager>();

            if (_loadingService == null)
                _loadingService = gameObject.AddComponent<ChartLoadingService>();

            if (_flowController == null)
                _flowController = gameObject.AddComponent<GameFlowController>();

            if (_scoreManager == null)
                _scoreManager = gameObject.AddComponent<ScoreManager>();
        }

        private void SetupEventHandlers()
        {
            _loadingService.OnChartLoaded += OnChartLoaded;

            // 기존 구독자들을 실제 컴포넌트에 연결
            if (_onStateChanged != null)
            {
                foreach (var handler in _onStateChanged.GetInvocationList())
                    _stateManager.OnStateChanged += (Action<GameState>)handler;
            }

            if (_onLoadProgressChanged != null)
            {
                foreach (var handler in _onLoadProgressChanged.GetInvocationList())
                    _loadingService.OnLoadProgressChanged += (Action<float>)handler;
            }

            if (_onError != null)
            {
                foreach (var handler in _onError.GetInvocationList())
                    _loadingService.OnError += (Action<string>)handler;
            }
        }

        private void OnDestroy()
        {
            if (_loadingService != null)
                _loadingService.OnChartLoaded -= OnChartLoaded;
        }

        private void Update()
        {
            if (_stateManager.IsPlaying())
            {
                _flowController.UpdateGameplay();

                if (Input.GetKeyDown(KeyCode.Escape))
                    PauseGame();
            }
            else if (_stateManager.IsPaused())
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    ResumeGame();
            }
        }

        public void LoadFromYoutube(string url, Difficulty difficulty = Difficulty.Normal)
        {
            Debug.Log($"[GameManager] LoadFromYoutube 호출: {url}");

            if (_stateManager == null)
            {
                Debug.LogError("[GameManager] _stateManager가 null!");
                return;
            }

            if (_loadingService == null)
            {
                Debug.LogError("[GameManager] _loadingService가 null!");
                return;
            }

            Debug.Log("[GameManager] State를 Loading으로 변경");
            _stateManager.SetState(GameState.Loading);
            Debug.Log("[GameManager] LoadingService.LoadFromYoutube 호출");
            _loadingService.LoadFromYoutube(url, difficulty);
        }

        public void LoadFromLocalFile(string audioPath, Difficulty difficulty = Difficulty.Normal)
        {
            _stateManager.SetState(GameState.Loading);
            _loadingService.LoadFromLocalFile(audioPath, difficulty);
        }

        private void OnChartLoaded(AudioClip clip, ChartData chart)
        {
            _flowController.PrepareGame(clip, chart);
        }

        public void StartGame() => _flowController.StartGame();
        public void PauseGame() => _flowController.PauseGame();
        public void ResumeGame() => _flowController.ResumeGame();

        public void RestartGame()
        {
            if (CurrentChart != null)
                _flowController.RestartGame(CurrentChart);
        }

        public void ReturnToMenu()
        {
            _flowController.ReturnToMenu();
            _loadingService.Clear();
        }

        public ResultData GetResult() => _scoreManager.GetResult();
    }
}
