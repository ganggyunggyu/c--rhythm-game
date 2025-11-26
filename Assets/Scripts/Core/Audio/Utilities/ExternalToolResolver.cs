using System.IO;
using UnityEngine;

namespace RhythmGame.Core.Audio.Utilities
{
    public static class ExternalToolResolver
    {
        private const string TOOL_FOLDER = "ffmpeg";

        public static string GetToolPath(string toolName)
        {
            var fileName = GetPlatformFileName(toolName);
            return Path.Combine(Application.streamingAssetsPath, TOOL_FOLDER, fileName);
        }

        private static string GetPlatformFileName(string toolName)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return $"{toolName}.exe";
#else
            return toolName;
#endif
        }

        public static bool ToolExists(string toolName)
        {
            var path = GetToolPath(toolName);
            return File.Exists(path);
        }
    }
}
