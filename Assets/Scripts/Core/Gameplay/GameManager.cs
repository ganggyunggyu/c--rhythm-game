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

        public GameState State => _stateManager.State;
        public ChartData CurrentChart => _loadingService.CurrentChart;
        public float LoadProgress => _loadingService.LoadProgress;

        public event Action<GameState> OnStateChanged
        {
            add => _stateManager.OnStateChanged += value;
            remove => _stateManager.OnStateChanged -= value;
        }

        public event Action<float> OnLoadProgressChanged
        {
            add => _loadingService.OnLoadProgressChanged += value;
            remove => _loadingService.OnLoadProgressChanged -= value;
        }

        public event Action<string> OnError
        {
            add => _loadingService.OnError += value;
            remove => _loadingService.OnError -= value;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureComponents();
            SetupEventHandlers();
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
            _stateManager.SetState(GameState.Loading);
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
