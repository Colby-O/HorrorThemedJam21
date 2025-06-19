#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HTJ21
{
    /*
    [InitializeOnLoad]
    public static class PlayModeStartupScene
    {
        const string _startupScenePath = "Assets/Scenes/Act1.unity"; 
        const string _prefKey = "PlayModeStartupScene_PreviousScene";

        static PlayModeStartupScene()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !string.IsNullOrEmpty(scene.path))
            {
                EditorPrefs.SetString(_prefKey, scene.path);
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    string previousScenePath = EditorPrefs.GetString(_prefKey, null);

                    if (string.IsNullOrEmpty(previousScenePath))
                    {
                        Debug.LogWarning("No previous scene path stored. Skipping scene switch.");
                        return;
                    }

                    if (previousScenePath != _startupScenePath)
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(_startupScenePath);
                        }
                        else
                        {
                            EditorApplication.isPlaying = false;
                        }
                    }
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    if (EditorPrefs.HasKey(_prefKey))
                    {
                        string pathToRestore = EditorPrefs.GetString(_prefKey);
                        
                        EditorApplication.delayCall += () =>
                        {
                            EditorSceneManager.OpenScene(pathToRestore);
                        };
                    }
                    break;
            }
        }
    }
    */
}
#endif
