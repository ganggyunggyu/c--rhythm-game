using System;
using System.Threading.Tasks;
using RhythmGame.Core.Analysis;
using RhythmGame.Core.Audio;
using RhythmGame.Data;
using RhythmGame.Utils;
using UnityEngine;

namespace RhythmGame.Core.Gameplay
{
    public class ChartLoadingService : MonoBehaviour
    {
        [SerializeField] private AudioLoader _audioLoader;
        [SerializeField] private GameSettings _settings;

        private YoutubeAudioDownloader _downloader;
        private BeatAnalyzer _beatAnalyzer;
        private ChartGenerator _chartGenerator;
        private string _currentVideoId;
        private Difficulty _currentDifficulty;

        public float LoadProgress { get; private set; }
        public ChartData CurrentChart { get; private set; }

        public event Action<float> OnLoadProgressChanged;
        public event Action<string> OnError;
        public event Action<AudioClip, ChartData> OnChartLoaded;

        private void Awake()
        {
            // Auto-create AudioLoader if missing
            if (_audioLoader == null)
            {
                _audioLoader = gameObject.AddComponent<AudioLoader>();
                Debug.Log("AudioLoader 자동 생성됨");
            }

            _downloader = new YoutubeAudioDownloader();
            _beatAnalyzer = new BeatAnalyzer();
            _chartGenerator = new ChartGenerator(_settings?.laneCount ?? 4);

            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            _downloader.OnProgress += progress =>
            {
                LoadProgress = progress * 0.5f;
                OnLoadProgressChanged?.Invoke(LoadProgress);
            };

            _downloader.OnError += error => OnError?.Invoke(error);
            _audioLoader.OnLoaded += OnAudioLoaded;
            _audioLoader.OnError += error => OnError?.Invoke(error);
        }

        public void LoadFromYoutube(string url, Difficulty difficulty = Difficulty.Normal)
        {
            _ = LoadFromYoutubeAsync(url, difficulty);
        }

        public async Task LoadFromYoutubeAsync(string url, Difficulty difficulty = Difficulty.Normal)
        {
            try
            {
                Debug.Log("[ChartLoadingService] LoadFromYoutube 시작");

                var videoId = YoutubeUrlParser.ExtractVideoId(url);
                Debug.Log($"[ChartLoadingService] VideoId 추출: {videoId}");

                if (string.IsNullOrEmpty(videoId))
                {
                    OnError?.Invoke("유효하지 않은 YouTube URL입니다.");
                    return;
                }

                _currentVideoId = videoId;
                _currentDifficulty = difficulty;
                LoadProgress = 0f;

                Debug.Log("[ChartLoadingService] 캐시 확인 중...");
                var chartPath = CacheManager.GetChartPath(videoId, difficulty.ToString().ToLower());
                if (CacheManager.HasCachedChart(videoId, difficulty.ToString().ToLower()))
                {
                    Debug.Log("[ChartLoadingService] 캐시된 차트 발견, 로드 중...");
                    CurrentChart = ChartData.Load(chartPath);
                    LoadProgress = 0.9f;
                    OnLoadProgressChanged?.Invoke(LoadProgress);
                    LoadAudioForChart();
                    return;
                }

                Debug.Log("[ChartLoadingService] 오디오 다운로드 시작...");
                var audioPath = await _downloader.DownloadAudioAsync(url);
                Debug.Log($"[ChartLoadingService] 오디오 다운로드 완료: {audioPath}");

                if (string.IsNullOrEmpty(audioPath))
                    return;

                LoadProgress = 0.5f;
                OnLoadProgressChanged?.Invoke(LoadProgress);

                Debug.Log("[ChartLoadingService] AudioLoader로 오디오 로드 중...");
                _audioLoader.LoadAudioClip(audioPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChartLoadingService] LoadFromYoutube 에러: {ex.Message}\n{ex.StackTrace}");
                OnError?.Invoke($"로딩 중 에러 발생: {ex.Message}");
            }
        }

        public void LoadFromLocalFile(string audioPath, Difficulty difficulty = Difficulty.Normal)
        {
            LoadProgress = 0.5f;
            _currentVideoId = System.IO.Path.GetFileNameWithoutExtension(audioPath);
            _currentDifficulty = difficulty;
            _audioLoader.LoadAudioClip(audioPath);
        }

        private void OnAudioLoaded(AudioClip clip)
        {
            UpdateLoadProgress(0.6f);

            _beatAnalyzer.Analyze(clip);
            UpdateLoadProgress(0.8f);

            var audioPath = CacheManager.GetAudioPath(_currentVideoId);
            CurrentChart = _chartGenerator.Generate(
                _currentVideoId,
                audioPath,
                _beatAnalyzer,
                _currentDifficulty
            );

            var chartPath = CacheManager.GetChartPath(_currentVideoId, _currentDifficulty.ToString().ToLower());
            CurrentChart.Save(chartPath);

            UpdateLoadProgress(0.9f);

            OnChartLoaded?.Invoke(clip, CurrentChart);

            UpdateLoadProgress(1f);
        }

        private void LoadAudioForChart()
        {
            _audioLoader.LoadAudioClip(CurrentChart.audioPath);
        }

        private void UpdateLoadProgress(float progress)
        {
            LoadProgress = progress;
            OnLoadProgressChanged?.Invoke(LoadProgress);
        }

        public void Clear()
        {
            CurrentChart = null;
            _currentVideoId = null;
            LoadProgress = 0f;
        }
    }
}
