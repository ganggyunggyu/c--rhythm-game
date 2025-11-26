using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace RhythmGame.Core.Audio
{
    public class AudioLoader : MonoBehaviour
    {
        public event Action<AudioClip> OnLoaded;
        public event Action<string> OnError;

        public void LoadAudioClip(string filePath)
        {
            StartCoroutine(LoadAudioCoroutine(filePath));
        }

        private IEnumerator LoadAudioCoroutine(string filePath)
        {
            if (!File.Exists(filePath))
            {
                OnError?.Invoke($"파일을 찾을 수 없습니다: {filePath}");
                yield break;
            }

            var uri = "file://" + filePath;
            var audioType = GetAudioType(filePath);

            using var request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                OnError?.Invoke($"오디오 로딩 실패: {request.error}");
                yield break;
            }

            var clip = DownloadHandlerAudioClip.GetContent(request);
            if (clip == null)
            {
                OnError?.Invoke("AudioClip 생성 실패");
                yield break;
            }

            clip.name = Path.GetFileNameWithoutExtension(filePath);
            OnLoaded?.Invoke(clip);
        }

        private AudioType GetAudioType(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLower();
            return ext switch
            {
                ".wav" => AudioType.WAV,
                ".mp3" => AudioType.MPEG,
                ".ogg" => AudioType.OGGVORBIS,
                _ => AudioType.UNKNOWN
            };
        }
    }
}
