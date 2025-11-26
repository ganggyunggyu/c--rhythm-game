using System;

namespace RhythmGame.Data
{
    public enum NoteType
    {
        Tap,
        Hold
    }

    [Serializable]
    public class NoteData
    {
        public float time;
        public int lane;
        public NoteType type;
        public float duration;

        public NoteData() { }

        public NoteData(float time, int lane, NoteType type = NoteType.Tap, float duration = 0f)
        {
            this.time = time;
            this.lane = lane;
            this.type = type;
            this.duration = duration;
        }
    }
}
