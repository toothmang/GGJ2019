#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

public class BuildDialogue : EditorWindow 
{
    public static string sceneFile = "Assets/Scenes/Default.Unity";
    public static string buildPath = "Build";
    public static BuildTarget buildTarget = BuildTarget.WebGL;
    public static bool devBuild = true;
    public static bool preBuilt = true;
    public static bool autoRun = false;

    [MenuItem("Build/Set details and build")]
    static void Init()
    {
        BuildDialogue dialogue = (BuildDialogue)EditorWindow.GetWindow(typeof(BuildDialogue));
        dialogue.Show();
    }

    public static void Build()
    {
        // Delete and recreate the build dir
        DirectoryInfo buildDir = new DirectoryInfo(buildPath);

        if (buildDir.Exists)
        {
            buildDir.Delete(true);
        }

        buildDir.Create();

        // Build the game 
        BuildOptions bo = BuildOptions.None;
        if (devBuild) bo |= BuildOptions.Development;
        if (autoRun) bo |= BuildOptions.AutoRunPlayer;
        BuildPipeline.BuildPlayer(new string[] { sceneFile }, buildPath, buildTarget, bo);
    }

    void OnGUI()
    {
        sceneFile = EditorGUILayout.TextField("scene:", sceneFile);
        buildPath = EditorGUILayout.TextField("build path", buildPath);

        if (GUILayout.Button("Build"))
        {
            try
            {
                Debug.Log("Beginning build...");
                Build();
                Debug.Log("Done with build!");
            }
            catch(Exception ex)
            {
                Debug.LogError("Build failed!");
                Debug.LogError(ex);
            }
        }

        devBuild = GUILayout.Toggle(devBuild, "Development build");
        preBuilt = GUILayout.Toggle(preBuilt, "Prebuilt engine");
        autoRun = GUILayout.Toggle(autoRun, "Auto-run");
        
    }
}
#endif