using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildScript
{
    private static string[] GetScenes()
    {
        return new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Loading.unity",
            "Assets/Scenes/Game.unity"
        };
    }

    [MenuItem("Build/Build Mac")]
    public static void BuildMac()
    {
        var path = "Builds/Mac/RhythmGame.app";
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = path,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        });

        Debug.Log($"Mac 빌드 완료: {path}");
    }

    [MenuItem("Build/Build Windows")]
    public static void BuildWindows()
    {
        var path = "Builds/Windows/RhythmGame.exe";
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = path,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });

        Debug.Log($"Windows 빌드 완료: {path}");
    }

    // CLI에서 호출할 메서드
    public static void BuildMacCLI()
    {
        BuildMac();
    }

    public static void BuildWindowsCLI()
    {
        BuildWindows();
    }
}
