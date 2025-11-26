using RhythmGame.Core.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmGame.UI
{
    public class LoadingUI : MonoBehaviour
    {
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private TMP_Text _statusText;

        private GameManager _cachedGameManager;

        private void Start()
        {
            _cachedGameManager = GameManager.Instance;
            if (_cachedGameManager != null)
            {
                _cachedGameManager.OnLoadProgressChanged += UpdateProgress;
                _cachedGameManager.OnStateChanged += OnGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (_cachedGameManager != null)
            {
                _cachedGameManager.OnLoadProgressChanged -= UpdateProgress;
                _cachedGameManager.OnStateChanged -= OnGameStateChanged;
            }
        }

        private void UpdateProgress(float progress)
        {
            if (_progressBar != null)
                _progressBar.value = progress;

            if (_progressText != null)
                _progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";

            if (_statusText != null)
            {
                _statusText.text = progress switch
                {
                    < 0.3f => "YouTube에서 오디오 다운로드 중...",
                    < 0.5f => "오디오 변환 중...",
                    < 0.7f => "비트 분석 중...",
                    < 0.9f => "차트 생성 중...",
                    _ => "준비 완료!"
                };
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            gameObject.SetActive(state == GameState.Loading);
        }
    }
}
