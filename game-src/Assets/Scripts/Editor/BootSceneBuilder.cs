using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Starstate.Ui;

namespace Starstate.EditorTools
{
    /// <summary>一键生成 Boot 场景（工程首次打开后执行一次即可）。</summary>
    public static class BootSceneBuilder
    {
        [MenuItem("STARSTATE/创建 Boot 场景")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera", typeof(Camera));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.11f, 0.10f, 0.09f);
            cam.orthographic = true;

            // 场景不挂任何脚本：GameBootstrap 由 RuntimeInitializeOnLoadMethod 运行时自举，
            // 从根源避免“场景脚本引用剥落 → The referenced script is missing”。
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Boot.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true) };

            Debug.Log("[STARSTATE] Boot 场景已创建（仅摄像机，无脚本）并设为起始场景——按 Play 开始游戏。");
        }
    }
}
