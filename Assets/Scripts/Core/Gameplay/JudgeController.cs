using System;
using RhythmGame.Data;
using UnityEngine;

namespace RhythmGame.Core.Gameplay
{
    public enum JudgeResult
    {
        None,
        Perfect,
        Great,
        Good,
        Miss
    }

    public class JudgeController : MonoBehaviour
    {
        [SerializeField] private GameSettings _settings;
        [SerializeField] private NoteSpawner _noteSpawner;
        [SerializeField] private SongController _songController;

        private float _missThreshold;

        public event Action<JudgeResult, Note> OnJudge;

        private void Awake()
        {
            _missThreshold = (_settings?.goodWindow ?? 150f) / 1000f + 0.05f;
        }

        private void Update()
        {
            if (_songController == null || _noteSpawner == null)
                return;

            ProcessInput();
            CheckMissedNotes();
        }

        private void ProcessInput()
        {
            var keys = _settings?.laneKeys ?? new KeyCode[]
            {
                KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K
            };

            for (int lane = 0; lane < keys.Length; lane++)
            {
                if (Input.GetKeyDown(keys[lane]))
                    TryJudgeLane(lane);
            }
        }

        private void TryJudgeLane(int lane)
        {
            var currentTime = _songController.SongTime;
            Note closestNote = null;
            float closestDiff = float.MaxValue;

            foreach (var note in _noteSpawner.ActiveNotes)
            {
                if (!note.IsActive || note.Lane != lane)
                    continue;

                var diff = Mathf.Abs(note.TargetTime - currentTime);
                if (diff < closestDiff && diff <= _missThreshold)
                {
                    closestDiff = diff;
                    closestNote = note;
                }
            }

            if (closestNote != null)
            {
                var result = EvaluateTiming(closestDiff);
                OnJudge?.Invoke(result, closestNote);
                _noteSpawner.ReturnToPool(closestNote);
            }
        }

        private JudgeResult EvaluateTiming(float timeDiff)
        {
            var diffMs = timeDiff * 1000f;
            var perfectWindow = _settings?.perfectWindow ?? 50f;
            var greatWindow = _settings?.greatWindow ?? 100f;
            var goodWindow = _settings?.goodWindow ?? 150f;

            if (diffMs <= perfectWindow) return JudgeResult.Perfect;
            if (diffMs <= greatWindow) return JudgeResult.Great;
            if (diffMs <= goodWindow) return JudgeResult.Good;

            return JudgeResult.Miss;
        }

        private void CheckMissedNotes()
        {
            var currentTime = _songController.SongTime;

            _noteSpawner.ProcessNotes(
                shouldRemove: note =>
                    note.IsActive && currentTime - note.TargetTime > _missThreshold,
                onRemove: note =>
                {
                    note.SetMissed();
                    OnJudge?.Invoke(JudgeResult.Miss, note);
                }
            );
        }
    }
}
