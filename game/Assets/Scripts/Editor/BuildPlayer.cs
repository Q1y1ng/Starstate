using System.IO;
using UnityEditor;
using UnityEngine;

namespace Starstate.EditorTools
{
    /// <summary>Windows 独立可执行构建（batchmode 可调用）。</summary>
    public static class BuildPlayer
    {
        private const string OutDir = "E:/Starstate/builds/Starstate";

        [MenuItem("STARSTATE/构建 Windows 包")]
        public static void BuildWindows()
        {
            Directory.CreateDirectory(OutDir);
            var opts = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Boot.unity" },
                locationPathName = OutDir + "/Starstate.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError("[STARSTATE] 构建失败：" + report.summary.result +
                    " errors=" + report.summary.totalErrors);
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }
            Debug.Log("[STARSTATE] 构建成功：" + opts.locationPathName +
                " size=" + report.summary.totalSize);
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
    }
}
