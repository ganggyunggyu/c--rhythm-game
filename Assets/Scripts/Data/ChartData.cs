using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RhythmGame.Data
{
    [Serializable]
    public class ChartData
    {
        public string videoId;
        public string audioPath;
        public float bpm;
        public string difficulty;
        public List<NoteData> notes;

        public ChartData()
        {
            notes = new List<NoteData>();
        }

        public static ChartData FromJson(string json)
        {
            return JsonUtility.FromJson<ChartData>(json);
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public void Save(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(path, ToJson());
        }

        public static ChartData Load(string path)
        {
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return FromJson(json);
        }
    }
}
