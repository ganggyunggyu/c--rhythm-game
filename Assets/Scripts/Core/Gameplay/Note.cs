using RhythmGame.Data;
using UnityEngine;

namespace RhythmGame.Core.Gameplay
{
    public class Note : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _holdTail;

        private NoteData _data;
        private float _targetTime;
        private int _lane;
        private bool _isActive;
        private float _speed;
        private float _judgeLineY;

        public NoteData Data => _data;
        public float TargetTime => _targetTime;
        public int Lane => _lane;
        public bool IsActive => _isActive;
        public bool IsHoldNote => _data?.type == NoteType.Hold;

        public void Initialize(NoteData data, float speed, float judgeLineY, Color laneColor)
        {
            _data = data;
            _targetTime = data.time;
            _lane = data.lane;
            _speed = speed;
            _judgeLineY = judgeLineY;
            _isActive = true;

            if (_spriteRenderer != null)
                _spriteRenderer.color = laneColor;

            // 홀드 노트 설정
            if (_holdTail != null)
            {
                _holdTail.gameObject.SetActive(IsHoldNote);
                if (IsHoldNote)
                {
                    var tailLength = data.duration * speed;
                    _holdTail.size = new Vector2(_holdTail.size.x, tailLength);
                    _holdTail.transform.localPosition = new Vector3(0, tailLength / 2, 0);
                }
            }
        }

        public void UpdatePosition(float currentTime)
        {
            if (!_isActive)
                return;

            var timeUntilHit = _targetTime - currentTime;
            var yPos = _judgeLineY + timeUntilHit * _speed;
            transform.position = new Vector3(transform.position.x, yPos, transform.position.z);
        }

        public void Deactivate()
        {
            _isActive = false;
            gameObject.SetActive(false);
        }

        public void SetMissed()
        {
            if (_spriteRenderer != null)
                _spriteRenderer.color = Color.gray;
        }
    }
}
