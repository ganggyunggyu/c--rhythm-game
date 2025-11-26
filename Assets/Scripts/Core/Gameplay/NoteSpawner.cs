using System.Collections.Generic;
using RhythmGame.Data;
using UnityEngine;

namespace RhythmGame.Core.Gameplay
{
    public class NoteSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _notePrefab;
        [SerializeField] private Transform[] _laneTransforms;
        [SerializeField] private float _judgeLineY = -3f;
        [SerializeField] private float _spawnLeadTime = 2f;
        [SerializeField] private float _noteSpeed = 5f;

        [SerializeField] private Color[] _laneColors = new Color[]
        {
            new Color(1f, 0.3f, 0.3f),    // 빨강
            new Color(0.3f, 1f, 0.3f),    // 초록
            new Color(0.3f, 0.3f, 1f),    // 파랑
            new Color(1f, 1f, 0.3f)       // 노랑
        };

        private ChartData _chart;
        private int _nextNoteIndex;
        private Queue<Note> _notePool;
        private List<Note> _activeNotes;

        public List<Note> ActiveNotes => _activeNotes;
        public float JudgeLineY => _judgeLineY;

        private void Awake()
        {
            _notePool = new Queue<Note>();
            _activeNotes = new List<Note>();

            if (_notePrefab != null)
                InitializePool(50);
        }

        private void InitializePool(int size)
        {
            for (int i = 0; i < size; i++)
            {
                var noteObj = Instantiate(_notePrefab, transform);
                noteObj.SetActive(false);
                var note = noteObj.GetComponent<Note>();
                _notePool.Enqueue(note);
            }
        }

        public void LoadChart(ChartData chart)
        {
            _chart = chart;
            _nextNoteIndex = 0;
            ClearActiveNotes();
        }

        public void UpdateSpawner(float currentTime)
        {
            if (_chart == null)
                return;

            SpawnUpcomingNotes(currentTime);
            UpdateActiveNotes(currentTime);
        }

        private void SpawnUpcomingNotes(float currentTime)
        {
            var spawnTime = currentTime + _spawnLeadTime;

            while (_nextNoteIndex < _chart.notes.Count &&
                   _chart.notes[_nextNoteIndex].time <= spawnTime)
            {
                SpawnNote(_chart.notes[_nextNoteIndex]);
                _nextNoteIndex++;
            }
        }

        private void SpawnNote(NoteData data)
        {
            var note = GetNoteFromPool();
            if (note == null)
                return;

            var laneIndex = Mathf.Clamp(data.lane, 0, _laneTransforms.Length - 1);
            var lanePos = _laneTransforms[laneIndex].position;
            var color = _laneColors[laneIndex % _laneColors.Length];

            note.transform.position = new Vector3(lanePos.x, _judgeLineY + _spawnLeadTime * _noteSpeed, 0);
            note.gameObject.SetActive(true);
            note.Initialize(data, _noteSpeed, _judgeLineY, color);

            _activeNotes.Add(note);
        }

        private void UpdateActiveNotes(float currentTime)
        {
            foreach (var note in _activeNotes)
            {
                if (note.IsActive)
                    note.UpdatePosition(currentTime);
            }
        }

        private Note GetNoteFromPool()
        {
            if (_notePool.Count > 0)
                return _notePool.Dequeue();

            // 풀이 비었으면 새로 생성
            var noteObj = Instantiate(_notePrefab, transform);
            return noteObj.GetComponent<Note>();
        }

        public void ReturnToPool(Note note)
        {
            note.Deactivate();
            _activeNotes.Remove(note);
            _notePool.Enqueue(note);
        }

        public void ProcessNotes(System.Func<Note, bool> shouldRemove, System.Action<Note> onRemove = null)
        {
            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                var note = _activeNotes[i];
                if (shouldRemove(note))
                {
                    onRemove?.Invoke(note);
                    note.Deactivate();
                    _activeNotes.RemoveAt(i);
                    _notePool.Enqueue(note);
                }
            }
        }

        public void ClearActiveNotes()
        {
            foreach (var note in _activeNotes)
            {
                note.Deactivate();
                _notePool.Enqueue(note);
            }
            _activeNotes.Clear();
        }

        public void SetNoteSpeed(float speed)
        {
            _noteSpeed = speed;
        }

        public void SetSpawnLeadTime(float time)
        {
            _spawnLeadTime = time;
        }
    }
}
