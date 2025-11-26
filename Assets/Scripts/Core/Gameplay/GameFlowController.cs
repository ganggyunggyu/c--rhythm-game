using RhythmGame.Data;
using UnityEngine;

namespace RhythmGame.Core.Gameplay
{
    public class GameFlowController : MonoBehaviour
    {
        [SerializeField] private SongController _songController;
        [SerializeField] private NoteSpawner _noteSpawner;
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private GameStateManager _stateManager;

        private void Awake()
        {
            if (_songController != null)
                _songController.OnSongEnd += OnSongEnd;
        }

        private void OnDestroy()
        {
            if (_songController != null)
                _songController.OnSongEnd -= OnSongEnd;
        }

        public void PrepareGame(AudioClip clip, ChartData chart)
        {
            _songController.LoadAudio(clip);
            _noteSpawner.LoadChart(chart);
            _scoreManager.Initialize(chart.notes.Count);

            _stateManager.SetState(GameState.Ready);
        }

        public void StartGame()
        {
            if (!_stateManager.CanStartGame())
                return;

            _stateManager.SetState(GameState.Playing);
            _songController.Play();
        }

        public void PauseGame()
        {
            if (!_stateManager.CanPauseGame())
                return;

            _stateManager.SetState(GameState.Paused);
            _songController.Pause();
        }

        public void ResumeGame()
        {
            if (!_stateManager.CanResumeGame())
                return;

            _stateManager.SetState(GameState.Playing);
            _songController.Resume();
        }

        public void RestartGame(ChartData chart)
        {
            _songController.Stop();
            _noteSpawner.ClearActiveNotes();
            _noteSpawner.LoadChart(chart);
            _scoreManager.Reset();
            _stateManager.SetState(GameState.Ready);
        }

        public void ReturnToMenu()
        {
            _songController.Stop();
            _noteSpawner.ClearActiveNotes();
            _stateManager.SetState(GameState.Idle);
        }

        private void OnSongEnd()
        {
            _stateManager.SetState(GameState.Result);
        }

        public void UpdateGameplay()
        {
            if (_stateManager.IsPlaying())
                _noteSpawner.UpdateSpawner(_songController.SongTime);
        }
    }
}
