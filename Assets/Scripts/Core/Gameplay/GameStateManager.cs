using System;
using UnityEngine;

namespace RhythmGame.Core.Gameplay
{
    public enum GameState
    {
        Idle,
        Loading,
        Ready,
        Playing,
        Paused,
        Result
    }

    public class GameStateManager : MonoBehaviour
    {
        private GameState _state = GameState.Idle;

        public GameState State => _state;
        public event Action<GameState> OnStateChanged;

        public void SetState(GameState newState)
        {
            if (_state == newState)
                return;

            _state = newState;
            OnStateChanged?.Invoke(_state);
            Debug.Log($"[GameState] {newState}");
        }

        public bool CanStartGame() => _state == GameState.Ready;
        public bool CanPauseGame() => _state == GameState.Playing;
        public bool CanResumeGame() => _state == GameState.Paused;
        public bool IsPlaying() => _state == GameState.Playing;
        public bool IsPaused() => _state == GameState.Paused;
    }
}
