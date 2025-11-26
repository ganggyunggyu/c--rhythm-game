using System;
using System.IO;
using UnityEngine;

namespace RhythmGame.Utils
{
    public static class CacheManager
    {
        private static string _cacheRoot;

        public static string CacheRoot
        {
            get
            {
                if (string.IsNullOrEmpty(_cacheRoot))
                    _cacheRoot = Path.Combine(Application.persistentDataPath, "cache");
                return _cacheRoot;
            }
        }

        public static string GetVideoFolder(string videoId)
        {
            var folder = Path.Combine(CacheRoot, videoId);
            EnsureDirectory(folder);
            return folder;
        }

        public static string GetAudioPath(string videoId) =>
            Path.Combine(GetVideoFolder(videoId), "audio.wav");

        public static string GetChartPath(string videoId, string difficulty = "normal") =>
            Path.Combine(GetVideoFolder(videoId), $"chart_{difficulty}.json");

        public static bool HasCachedAudio(string videoId) =>
            File.Exists(GetAudioPath(videoId));

        public static bool HasCachedChart(string videoId, string difficulty = "normal") =>
            File.Exists(GetChartPath(videoId, difficulty));

        public static bool EnsureDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"디렉토리 생성 실패: {path} - {ex.Message}");
                return false;
            }
        }

        public static bool TryClearCache(string videoId, out string error)
        {
            error = null;
            try
            {
                var folder = Path.Combine(CacheRoot, videoId);
                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Debug.LogWarning($"캐시 삭제 실패: {videoId} - {error}");
                return false;
            }
        }

        public static void ClearCache(string videoId)
        {
            TryClearCache(videoId, out _);
        }

        public static bool TryClearAllCache(out string error)
        {
            error = null;
            try
            {
                if (Directory.Exists(CacheRoot))
                    Directory.Delete(CacheRoot, true);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Debug.LogWarning($"전체 캐시 삭제 실패: {error}");
                return false;
            }
        }

        public static void ClearAllCache()
        {
            TryClearAllCache(out _);
        }
    }
}
