using System;
using UnityEngine;

namespace RhythmGame.Core.Gameplay
{
    public class SongController : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;

        private double _dspStartTime;
        private bool _isPlaying;
        private float _pausedTime;

        public float SongTime => _isPlaying
            ? (float)(AudioSettings.dspTime - _dspStartTime)
            : _pausedTime;

        public float SongLength => _audioSource.clip != null ? _audioSource.clip.length : 0f;
        public bool IsPlaying => _isPlaying;
        public float NormalizedTime => SongLength > 0 ? SongTime / SongLength : 0f;

        public event Action OnSongStart;
        public event Action OnSongEnd;

        private void Update()
        {
            if (_isPlaying && !_audioSource.isPlaying && SongTime >= SongLength)
            {
                _isPlaying = false;
                OnSongEnd?.Invoke();
            }
        }

        public void LoadAudio(AudioClip clip)
        {
            _audioSource.clip = clip;
            _pausedTime = 0f;
            _isPlaying = false;
        }

        public void Play(float startTime = 0f)
        {
            if (_audioSource.clip == null)
                return;

            _audioSource.time = startTime;
            _dspStartTime = AudioSettings.dspTime - startTime;
            _audioSource.Play();
            _isPlaying = true;
            OnSongStart?.Invoke();
        }

        public void Pause()
        {
            if (!_isPlaying)
                return;

            _pausedTime = SongTime;
            _audioSource.Pause();
            _isPlaying = false;
        }

        public void Resume()
        {
            if (_isPlaying || _audioSource.clip == null)
                return;

            _dspStartTime = AudioSettings.dspTime - _pausedTime;
            _audioSource.UnPause();
            _isPlaying = true;
        }

        public void Stop()
        {
            _audioSource.Stop();
            _isPlaying = false;
            _pausedTime = 0f;
        }

        public void SetVolume(float volume)
        {
            _audioSource.volume = Mathf.Clamp01(volume);
        }
    }
}
