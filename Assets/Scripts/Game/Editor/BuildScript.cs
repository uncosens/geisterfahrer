using System;
using System.IO;
using Geisterfahrer.Game;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Geisterfahrer.Editor
{
    // Batch: Unity.exe -batchmode -quit -projectPath . -buildTarget Android -executeMethod Geisterfahrer.Editor.BuildScript.BuildAndroid [-release]
    public static class BuildScript
    {
        const string ScenePath = "Assets/Scenes/Game.unity";

        [MenuItem("Geisterfahrer/Create Game Scene")]
        public static void CreateScene()
        {
            const string matPath = "Assets/Materials/Base.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, matPath);
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            new GameObject("Game").AddComponent<GameLoop>().baseMaterial = mat;
            // Fog on in the scene so URP keeps the fog shader variants (the haze is set at runtime).
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        // URP mobile settings from docs/dev/unity-plan.md section 1.
        static void ConfigureUrp()
        {
            foreach (var path in new[] { "Assets/Settings/Mobile_RPAsset.asset", "Assets/Settings/PC_RPAsset.asset" })
            {
                var a = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
                a.supportsHDR = false;
                a.msaaSampleCount = 2;
                a.shadowDistance = 60;
                a.shadowCascadeCount = 1;
                a.mainLightShadowmapResolution = 1024;
                EditorUtility.SetDirty(a);
            }
            AssetDatabase.SaveAssets();
        }

        public static void BuildAndroid()
        {
            bool release = Array.IndexOf(Environment.GetCommandLineArgs(), "-release") >= 0;
            if (!File.Exists(ScenePath)) CreateScene();
            ConfigureUrp();

            PlayerSettings.companyName = "uncosens";
            PlayerSettings.productName = "Geisterfahrer";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.uncosens.geisterfahrer");
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            // ARM64 only: Unity 6.3 refuses Android x86_64 (deprecated), so the emulator runs it through ARM translation.
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)29;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = release ? "Builds/Android/geisterfahrer.apk" : "Builds/Android/geisterfahrer-dev.apk",
                target = BuildTarget.Android,
                options = release ? BuildOptions.None : BuildOptions.Development,
            });
            var s = report.summary;
            Debug.Log($"BUILD {s.result}: {s.totalSize / (1024 * 1024)} MB in {s.totalTime}, errors {s.totalErrors}");
            if (s.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }
    }
}
