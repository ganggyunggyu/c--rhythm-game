using System;
using System.Text.RegularExpressions;

namespace RhythmGame.Utils
{
    public static class YoutubeUrlParser
    {
        private static readonly Regex VideoIdRegex = new Regex(
            @"(?:youtube\.com\/(?:watch\?v=|embed\/|v\/)|youtu\.be\/)([a-zA-Z0-9_-]{11})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        public static string ExtractVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            var match = VideoIdRegex.Match(url);
            return match.Success ? match.Groups[1].Value : null;
        }

        public static bool IsValidYoutubeUrl(string url) =>
            !string.IsNullOrEmpty(ExtractVideoId(url));
    }
}
