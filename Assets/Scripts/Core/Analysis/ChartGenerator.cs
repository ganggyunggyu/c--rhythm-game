using System.Collections.Generic;
using RhythmGame.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RhythmGame.Core.Analysis
{
    public class ChartGenerator
    {
        private readonly int _laneCount;

        public ChartGenerator(int laneCount = 4)
        {
            _laneCount = laneCount;
        }

        public ChartData Generate(string videoId, string audioPath, BeatAnalyzer analyzer, Difficulty difficulty = Difficulty.Normal)
        {
            var chart = new ChartData
            {
                videoId = videoId,
                audioPath = audioPath,
                bpm = analyzer.EstimatedBpm,
                difficulty = difficulty.ToString().ToLower(),
                notes = new List<NoteData>()
            };

            var beatTimes = analyzer.BeatTimes;
            var config = DifficultyConfig.CreateDefault(difficulty);

            GenerateNotes(chart, beatTimes, config);

            Debug.Log($"차트 생성 완료 - 난이도: {difficulty}, 노트 수: {chart.notes.Count}");
            return chart;
        }

        private void GenerateNotes(ChartData chart, List<float> beatTimes, DifficultyConfig config)
        {
            var lastLane = -1;
            var patternIndex = 0;

            for (int i = 0; i < beatTimes.Count; i++)
            {
                if (Random.value > config.noteDensity)
                    continue;

                var time = beatTimes[i];
                var noteCount = DetermineNoteCount(config, patternIndex);

                var lanes = SelectLanes(noteCount, lastLane, config.patternComplexity);
                foreach (var lane in lanes)
                {
                    var noteType = DetermineNoteType(config, i, beatTimes.Count);
                    var duration = noteType == NoteType.Hold ? GetHoldDuration(i, beatTimes) : 0f;

                    chart.notes.Add(new NoteData(time, lane, noteType, duration));
                }

                if (lanes.Count > 0)
                    lastLane = lanes[lanes.Count - 1];

                patternIndex++;
            }

            // 시간순 정렬
            chart.notes.Sort((a, b) => a.time.CompareTo(b.time));
        }

        private int DetermineNoteCount(DifficultyConfig config, int patternIndex)
        {
            if (patternIndex % 4 == 0 && Random.value < 0.3f)
                return Mathf.Min(2, config.maxSimultaneousNotes);

            if (patternIndex % 8 == 0 && Random.value < 0.2f)
                return config.maxSimultaneousNotes;

            return 1;
        }

        private List<int> SelectLanes(int count, int lastLane, int complexity)
        {
            var lanes = new List<int>();
            var available = new List<int>();

            for (int i = 0; i < _laneCount; i++)
                available.Add(i);

            // 너무 멀리 점프하지 않도록 (complexity에 따라 조절)
            if (lastLane >= 0 && complexity < 3)
            {
                var maxJump = complexity + 1;
                available.RemoveAll(l => Mathf.Abs(l - lastLane) > maxJump);

                // 만약 모든 레인이 제거되면 복구
                if (available.Count == 0)
                    for (int i = 0; i < _laneCount; i++)
                        available.Add(i);
            }

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                var idx = Random.Range(0, available.Count);
                lanes.Add(available[idx]);
                available.RemoveAt(idx);
            }

            lanes.Sort();
            return lanes;
        }

        private NoteType DetermineNoteType(DifficultyConfig config, int beatIndex, int totalBeats)
        {
            if (beatIndex > totalBeats - 4)
                return NoteType.Tap;

            return Random.value < config.holdNoteChance ? NoteType.Hold : NoteType.Tap;
        }

        private float GetHoldDuration(int currentIndex, List<float> beatTimes)
        {
            // 1~4박자 길이의 홀드
            var holdBeats = Random.Range(1, 5);
            var endIndex = Mathf.Min(currentIndex + holdBeats, beatTimes.Count - 1);

            if (endIndex <= currentIndex)
                return 0.5f;

            return beatTimes[endIndex] - beatTimes[currentIndex];
        }
    }
}
