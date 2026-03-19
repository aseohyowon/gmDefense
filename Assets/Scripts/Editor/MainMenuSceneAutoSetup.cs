#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace S2RD.EditorTools
{
    /// <summary>
    /// MainMenu 씬이 없으면 자동 생성하고 Build Settings에 포함합니다.
    /// </summary>
    public static class MainMenuSceneAutoSetup
    {
        [InitializeOnLoadMethod]
        private static void 에디터시작자동설정()
        {
            const string sessionKey = "S2RD.MainMenuSceneAutoSetup.RunOnce";
            if (SessionState.GetBool(sessionKey, false))
                return;

            SessionState.SetBool(sessionKey, true);
            EditorApplication.delayCall += 실행;
        }

        private static void 실행()
        {
            const string mainMenuPath = "Assets/MainMenu.unity";
            if (!System.IO.File.Exists(mainMenuPath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, mainMenuPath);
            }

            BuildScene추가(mainMenuPath);
            if (System.IO.File.Exists("Assets/Game.unity"))
                BuildScene추가("Assets/Game.unity");
            else if (System.IO.File.Exists("Assets/test.unity"))
                BuildScene추가("Assets/test.unity");
        }

        private static void BuildScene추가(string path)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == path)
                    return;
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
