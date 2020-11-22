using UnityEditor;
using UnityEngine;

public class BuildHelper
{
    [MenuItem("Build/BuildAll")]
    public static void BuildAll()
    {
        BuildServer();
        BuildClient();
        if (EditorUtility.DisplayDialog("Build finished", "You can now test the game.", "OK"))
        { }
    }

    [MenuItem("Build/BuildServer")]
    public static void BuildServer()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] {"Assets/Scenes/World.unity"};
        buildPlayerOptions.locationPathName = "C:/Daten/Anega/Program/Program-Server/AnegaServer.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.EnableHeadlessMode;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    [MenuItem("Build/BuildClient")]
    public static void BuildClient()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/World.unity" };
        buildPlayerOptions.locationPathName = "C:/Daten/Anega/Program/Program-Client/AnegaClient.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}
