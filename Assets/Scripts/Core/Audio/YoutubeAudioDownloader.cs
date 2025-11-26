using System;
using System.IO;
using System.Threading.Tasks;
using RhythmGame.Core.Audio.Utilities;
using RhythmGame.Utils;
using UnityEngine;

namespace RhythmGame.Core.Audio
{
    public class YoutubeAudioDownloader
    {
        public event Action<float> OnProgress;
        public event Action<string> OnError;
        public event Action<string> OnComplete;

        public async Task<string> DownloadAudioAsync(string youtubeUrl)
        {
            try
            {
                Debug.Log("[YoutubeAudioDownloader] DownloadAudioAsync 시작");

                var videoId = YoutubeUrlParser.ExtractVideoId(youtubeUrl);
                Debug.Log($"[YoutubeAudioDownloader] VideoId: {videoId}");

                if (string.IsNullOrEmpty(videoId))
                {
                    OnError?.Invoke("유효하지 않은 YouTube URL입니다.");
                    return null;
                }

                var outputPath = CacheManager.GetAudioPath(videoId);
                Debug.Log($"[YoutubeAudioDownloader] OutputPath: {outputPath}");

                if (CacheManager.HasCachedAudio(videoId))
                {
                    Debug.Log($"[YoutubeAudioDownloader] 캐시된 오디오 사용: {outputPath}");
                    OnComplete?.Invoke(outputPath);
                    return outputPath;
                }

                Debug.Log("[YoutubeAudioDownloader] 캐시 없음, 다운로드 진행");
                OnProgress?.Invoke(0.1f);

                Debug.Log("[YoutubeAudioDownloader] GetAudioStreamUrlAsync 호출 전");
                var streamUrl = await GetAudioStreamUrlAsync(youtubeUrl);
                Debug.Log($"[YoutubeAudioDownloader] StreamUrl 획득: {streamUrl}");

                if (string.IsNullOrEmpty(streamUrl))
                {
                    OnError?.Invoke("오디오 스트림을 가져올 수 없습니다.");
                    return null;
                }

                OnProgress?.Invoke(0.3f);

                Debug.Log("[YoutubeAudioDownloader] ConvertToWavAsync 호출 전");
                var success = await ConvertToWavAsync(streamUrl, outputPath);
                Debug.Log($"[YoutubeAudioDownloader] ConvertToWavAsync 결과: {success}");

                if (!success)
                {
                    OnError?.Invoke("오디오 변환에 실패했습니다.");
                    return null;
                }

                OnProgress?.Invoke(1f);
                OnComplete?.Invoke(outputPath);
                return outputPath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[YoutubeAudioDownloader] 다운로드 실패: {ex.Message}\n{ex.StackTrace}");
                OnError?.Invoke($"다운로드 실패: {ex.Message}");
                return null;
            }
        }

        private async Task<string> GetAudioStreamUrlAsync(string youtubeUrl)
        {
            Debug.Log("[YoutubeAudioDownloader] GetAudioStreamUrlAsync 시작");

            var ytDlpPath = ExternalToolResolver.GetToolPath("yt-dlp");
            Debug.Log($"[YoutubeAudioDownloader] yt-dlp 경로: {ytDlpPath}");

            if (!ExternalToolResolver.ToolExists("yt-dlp"))
            {
                Debug.LogWarning("[YoutubeAudioDownloader] yt-dlp를 찾을 수 없습니다. StreamingAssets/ffmpeg 폴더에 yt-dlp를 추가해주세요.");
                return youtubeUrl;
            }

            Debug.Log("[YoutubeAudioDownloader] yt-dlp 존재 확인됨");
            Debug.Log("[YoutubeAudioDownloader] ProcessRunner.RunAsync 호출 직전");

            var result = await ProcessRunner.RunAsync(
                ytDlpPath,
                $"-f bestaudio -g \"{youtubeUrl}\""
            );

            Debug.Log($"[YoutubeAudioDownloader] ProcessRunner.RunAsync 완료, Success: {result.Success}");
            return result.Success ? result.Output : youtubeUrl;
        }

        private async Task<bool> ConvertToWavAsync(string inputUrl, string outputPath)
        {
            var ffmpegPath = ExternalToolResolver.GetToolPath("ffmpeg");

            if (!ExternalToolResolver.ToolExists("ffmpeg"))
            {
                Debug.LogError($"ffmpeg를 찾을 수 없습니다: {ffmpegPath}");
                return false;
            }

            CacheManager.EnsureDirectory(Path.GetDirectoryName(outputPath));

            var result = await ProcessRunner.RunAsync(
                ffmpegPath,
                $"-i \"{inputUrl}\" -vn -acodec pcm_s16le -ar 44100 -ac 2 -y \"{outputPath}\"",
                null,
                error => Debug.Log($"[ffmpeg] {error}")
            );

            return result.Success && File.Exists(outputPath);
        }
    }
}
