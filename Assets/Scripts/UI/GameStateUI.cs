using RhythmGame.Core.Gameplay;
using UnityEngine;

namespace RhythmGame.UI
{
    public abstract class GameStateUI : MonoBehaviour
    {
        protected virtual GameState ActiveState => GameState.Idle;
        protected GameManager CachedGameManager { get; private set; }

        protected virtual void Start()
        {
            CachedGameManager = GameManager.Instance;
            if (CachedGameManager != null)
                CachedGameManager.OnStateChanged += OnGameStateChanged;

            gameObject.SetActive(false);
        }

        protected virtual void OnDestroy()
        {
            if (CachedGameManager != null)
                CachedGameManager.OnStateChanged -= OnGameStateChanged;
        }

        protected virtual void OnGameStateChanged(GameState state)
        {
            gameObject.SetActive(state == ActiveState);
        }
    }
}
