using System;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGame.Core.Analysis
{
    public class BeatAnalyzer
    {
        private const int SampleRate = 44100;
        private const int WindowSize = 1024;
        private const int HopSize = 512;

        public float EstimatedBpm { get; private set; }
        public List<float> BeatTimes { get; private set; } = new();

        public void Analyze(AudioClip clip)
        {
            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            // 모노로 변환
            var monoSamples = ConvertToMono(samples, clip.channels);

            // 에너지 기반 온셋 검출
            var energyEnvelope = ComputeEnergyEnvelope(monoSamples);

            // 피크 검출로 비트 타이밍 추출
            var peaks = DetectPeaks(energyEnvelope);

            // BPM 추정
            EstimatedBpm = EstimateBpm(peaks, clip.length);

            // 비트 그리드 생성
            BeatTimes = GenerateBeatGrid(peaks, clip.length);

            Debug.Log($"분석 완료 - BPM: {EstimatedBpm:F1}, 비트 수: {BeatTimes.Count}");
        }

        private float[] ConvertToMono(float[] samples, int channels)
        {
            if (channels == 1)
                return samples;

            var mono = new float[samples.Length / channels];
            for (int i = 0; i < mono.Length; i++)
            {
                float sum = 0;
                for (int ch = 0; ch < channels; ch++)
                    sum += samples[i * channels + ch];
                mono[i] = sum / channels;
            }
            return mono;
        }

        private float[] ComputeEnergyEnvelope(float[] samples)
        {
            var frameCount = (samples.Length - WindowSize) / HopSize + 1;
            var envelope = new float[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                var start = i * HopSize;
                float energy = 0;

                for (int j = 0; j < WindowSize && start + j < samples.Length; j++)
                {
                    var sample = samples[start + j];
                    energy += sample * sample;
                }

                envelope[i] = Mathf.Sqrt(energy / WindowSize);
            }

            // 정규화
            var max = 0f;
            foreach (var e in envelope)
                if (e > max) max = e;

            if (max > 0)
                for (int i = 0; i < envelope.Length; i++)
                    envelope[i] /= max;

            return envelope;
        }

        private List<float> DetectPeaks(float[] envelope)
        {
            var peaks = new List<float>();
            var threshold = 0.3f;
            var minPeakDistance = (int)(SampleRate * 0.1f / HopSize); // 최소 100ms 간격

            var lastPeakIdx = -minPeakDistance;

            for (int i = 1; i < envelope.Length - 1; i++)
            {
                if (envelope[i] > threshold &&
                    envelope[i] > envelope[i - 1] &&
                    envelope[i] > envelope[i + 1] &&
                    i - lastPeakIdx >= minPeakDistance)
                {
                    var timeInSeconds = (float)i * HopSize / SampleRate;
                    peaks.Add(timeInSeconds);
                    lastPeakIdx = i;
                }
            }

            return peaks;
        }

        private float EstimateBpm(List<float> peaks, float totalDuration)
        {
            if (peaks.Count < 2)
                return 120f; // 기본값

            // 피크 간 간격 계산
            var intervals = new List<float>();
            for (int i = 1; i < peaks.Count; i++)
                intervals.Add(peaks[i] - peaks[i - 1]);

            // 중앙값 계산
            intervals.Sort();
            var medianInterval = intervals[intervals.Count / 2];

            if (medianInterval <= 0)
                return 120f;

            var bpm = 60f / medianInterval;

            // BPM 범위 조정 (60~200 사이로)
            while (bpm < 60) bpm *= 2;
            while (bpm > 200) bpm /= 2;

            return Mathf.Round(bpm);
        }

        private List<float> GenerateBeatGrid(List<float> peaks, float totalDuration)
        {
            if (EstimatedBpm <= 0)
                return peaks;

            var beatInterval = 60f / EstimatedBpm;
            var grid = new List<float>();

            // 첫 비트 위치 찾기
            var firstBeat = peaks.Count > 0 ? peaks[0] : 0f;

            // 시작부터 끝까지 비트 그리드 생성
            for (var time = firstBeat; time < totalDuration; time += beatInterval)
            {
                grid.Add(time);
            }

            return grid;
        }
    }
}
