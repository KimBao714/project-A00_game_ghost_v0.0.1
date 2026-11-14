using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GhostRiver.Editor
{
    [InitializeOnLoad]
    internal static class GhostRiverSceneSetup
    {
        private const string ScenePath = "Assets/GhostRiver/Scenes/Ghost_Level_1.unity";

        static GhostRiverSceneSetup()
        {
            EditorApplication.delayCall += CreateStarterSceneIfMissing;
        }

        [MenuItem("Ghost/Tạo hoặc mở Màn 1")]
        private static void OpenOrCreateStarterScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (scene != null)
            {
                if (SceneManager.GetActiveScene().path != ScenePath)
                    EditorSceneManager.OpenScene(ScenePath);
                return;
            }

            CreateStarterScene();
        }

        private static void CreateStarterSceneIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                CreateStarterScene();
        }

        private static void CreateStarterScene()
        {
            Directory.CreateDirectory("Assets/GhostRiver/Scenes");
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("GHOST · màn 1");
            root.AddComponent<GhostRiverGame>();
            EditorSceneManager.SaveScene(newScene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            Debug.Log("Đã tạo GHOST · Màn 1. Nhấn Play để bắt đầu.");
        }
    }
}
