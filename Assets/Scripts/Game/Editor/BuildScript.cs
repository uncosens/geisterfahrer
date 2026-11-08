using System;
using System.IO;
using System.Linq;
using Geisterfahrer.Core;
using Geisterfahrer.Game;
using TMPro;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.LowLevel;

namespace Geisterfahrer.Editor
{
    // Batch:
    //   Unity.exe -batchmode -quit -projectPath . -executeMethod Geisterfahrer.Editor.BuildScript.Setup
    //   Unity.exe -batchmode -quit -projectPath . -buildTarget Android -executeMethod Geisterfahrer.Editor.BuildScript.BuildAndroid [-release]
    // Setup makes materials, fonts and the scene from what is in Assets (run tools/fetch-assets.sh first).
    public static class BuildScript
    {
        const string ScenePath = "Assets/Scenes/Game.unity", Gen = "Assets/ThirdParty/Generated", TP = "Assets/ThirdParty";

        [MenuItem("Geisterfahrer/Setup Scene and Assets")]
        public static void Setup()
        {
            if (!Directory.Exists("Assets/TextMesh Pro")) throw new Exception("TMP Essential Resources missing: Window > TextMeshPro > Import TMP Essential Resources.");
            Directory.CreateDirectory(Gen);
            AssetDatabase.Refresh();

            foreach (var path in Directory.GetFiles("Assets/UI/Resources/Icons", "*.png"))
            {
                var ti = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                if (ti.textureType == TextureImporterType.Sprite) continue;
                ti.textureType = TextureImporterType.Sprite; ti.mipmapEnabled = false; ti.alphaIsTransparency = true;
                ti.SaveAndReimport();
            }

            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var art = new Art
            {
                baseLit = Asset("Assets/Materials/Base.mat", () => new Material(lit)),
                dash = Asset("Assets/Materials/Dash.mat", () =>
                {
                    var m = new Material(lit);
                    m.SetFloat("_AlphaClip", 1); m.SetFloat("_Cutoff", 0.5f); m.EnableKeyword("_ALPHATEST_ON");
                    m.SetFloat("_Smoothness", 0.35f);
                    return m;
                }),
                spark = Asset("Assets/Materials/Spark.mat", () =>
                {
                    var m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                    m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 2); // transparent, additive
                    m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); m.SetFloat("_DstBlend", (float)BlendMode.One); m.SetFloat("_ZWrite", 0);
                    m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    m.renderQueue = (int)RenderQueue.Transparent;
                    return m;
                }),
                asphalt = TexMat("Asphalt", "Road/asphalt_01_diff_1k.jpg", "Road/asphalt_01_nor_gl_1k.jpg", 0.6f * 0.5f),
                grass = TexMat("Grass", "Road/Grass004_1K-JPG_Color.jpg", null, 0.1f),
                bold = Font("Inter-Bold", 96, 10, "0123456789.,- kmh/"),
                semi = Font("Inter-SemiBold", 72, 8, "0123456789.,- kmh/"),
                black = Font("Inter-Black", 96, 10, "GEISTERFAHR"),
                sign = Font("BarlowSemiCondensed-SemiBold", 64, 6, "0123456789."),
                vehicles = Road.Types.Select(t => new Models { list = ModelsFor(t.ModelKey) }).ToArray(),
                player = PlayerPrefab.Build() ?? Model("Player"),
            };
            art.boldShadow = Shadow(art.bold);
            art.semiShadow = Shadow(art.semi);

            ConfigureUrp();
            var post = Asset("Assets/Settings/PostProfile.asset", () =>
            {
                var p = ScriptableObject.CreateInstance<VolumeProfile>();
                return p;
            });
            if (!post.Has<ColorAdjustments>())
            {
                var ca = post.Add<ColorAdjustments>(true); ca.contrast.value = 8; ca.saturation.value = -6;
                post.Add<WhiteBalance>(true).temperature.value = 5;
                var v = post.Add<Vignette>(true); v.intensity.value = 0.18f; v.smoothness.value = 0.4f;
                foreach (var c in post.components) AssetDatabase.AddObjectToAsset(c, post);
                EditorUtility.SetDirty(post);
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            new GameObject("Game").AddComponent<GameLoop>().art = art;
            var vol = new GameObject("Post").AddComponent<Volume>();
            vol.isGlobal = true; vol.sharedProfile = post;

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(40, 30, 0);
            sun.color = new Color(1, 0.945f, 0.871f); // #FFF1DE
            sun.intensity = 1.35f; // 1.6 in look-spec 1; too bright on the tablet (decision 33)
            sun.shadows = LightShadows.Hard; sun.shadowStrength = 0.85f; sun.shadowBias = 1; sun.shadowNormalBias = 0.6f;
            RenderSettings.sun = sun;

            // Fog on in the scene so URP keeps the fog shader variants (distances are set at runtime).
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.788f, 0.839f, 0.886f); // #C9D6E2
            var hdr = AssetDatabase.LoadAssetAtPath<Texture>($"{TP}/Sky/dreifaltigkeitsberg_2k.hdr");
            if (hdr)
            {
                var ti = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(hdr));
                if (ti.mipmapEnabled || ti.maxTextureSize != 2048)
                {
                    ti.mipmapEnabled = false; ti.maxTextureSize = 2048;
                    ti.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Android", overridden = true, maxTextureSize = 2048, format = TextureImporterFormat.RGB9E5 });
                    ti.SaveAndReimport();
                }
                var sky = Asset($"{Gen}/Sky.mat", () => new Material(Shader.Find("Skybox/Panoramic")));
                sky.SetTexture("_MainTex", hdr); sky.SetFloat("_Exposure", 1); sky.SetFloat("_Rotation", 0);
                RenderSettings.skybox = sky;
                RenderSettings.ambientMode = AmbientMode.Skybox;
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
                RenderSettings.defaultReflectionResolution = 128;
                Lightmapping.bakedGI = false; Lightmapping.realtimeGI = false;
                EditorSceneManager.SaveScene(scene, ScenePath);
                Lightmapping.Bake(); // no lightmaps: only the ambient probe and the sky reflection
            }
            else
            {
                RenderSettings.skybox = null;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.62f);
            }
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        static T Asset<T>(string path, Func<T> make) where T : UnityEngine.Object
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a) return a;
            a = make();
            AssetDatabase.CreateAsset(a, path);
            return a;
        }

        static Material TexMat(string name, string baseMap, string normalMap, float smoothness)
        {
            var b = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TP}/{baseMap}");
            if (!b) return null;
            Smooth($"{TP}/{baseMap}");
            if (normalMap != null) Smooth($"{TP}/{normalMap}");
            var m = Asset($"{Gen}/{name}.mat", () => new Material(Shader.Find("Universal Render Pipeline/Lit")));
            m.SetTexture("_BaseMap", b);
            m.SetFloat("_Smoothness", smoothness);
            if (normalMap != null && AssetDatabase.LoadAssetAtPath<Texture2D>($"{TP}/{normalMap}") is Texture2D n)
            {
                var ti = (TextureImporter)AssetImporter.GetAtPath($"{TP}/{normalMap}");
                if (ti.textureType != TextureImporterType.NormalMap) { ti.textureType = TextureImporterType.NormalMap; ti.SaveAndReimport(); }
                m.SetTexture("_BumpMap", n); m.SetFloat("_BumpScale", 0.6f); m.EnableKeyword("_NORMALMAP");
            }
            EditorUtility.SetDirty(m);
            return m;
        }

        // Ground textures seen at a grazing angle: trilinear + aniso 8, so the grain doesn't crawl or flicker.
        static void Smooth(string path)
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            if (!ti || (ti.mipmapEnabled && ti.filterMode == FilterMode.Trilinear && ti.anisoLevel == 8)) return;
            ti.mipmapEnabled = true; ti.filterMode = FilterMode.Trilinear; ti.anisoLevel = 8;
            ti.SaveAndReimport();
        }

        // Static TMP SDF font asset with only the characters the UI needs.
        static TMP_FontAsset Font(string file, int size, int padding, string chars)
        {
            var path = $"{Gen}/{file} SDF.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (existing) return existing;
            var src = AssetDatabase.LoadAssetAtPath<Font>($"{TP}/Fonts/{file}.ttf");
            if (!src) return null;
            var fa = TMP_FontAsset.CreateFontAsset(src, size, padding, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
            fa.TryAddCharacters(chars);
            fa.atlasPopulationMode = AtlasPopulationMode.Static;
            AssetDatabase.CreateAsset(fa, path);
            fa.atlasTextures[0].name = file + " Atlas";
            AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
            fa.material.name = file + " Material";
            AssetDatabase.AddObjectToAsset(fa.material, fa);
            EditorUtility.SetDirty(fa);
            return fa;
        }

        // HUD-Shadow preset (ui-spec 2): underlay black 0.55, offset y -0.6, softness 0.6.
        static Material Shadow(TMP_FontAsset fa)
        {
            if (!fa) return null;
            return Asset($"{Gen}/{fa.name} HUD-Shadow.mat", () =>
            {
                var m = new Material(fa.material);
                m.EnableKeyword("UNDERLAY_ON");
                m.SetColor("_UnderlayColor", new Color(0, 0, 0, 0.55f));
                m.SetFloat("_UnderlayOffsetY", -0.6f);
                m.SetFloat("_UnderlaySoftness", 0.6f);
                return m;
            });
        }

        // A model the user downloaded to Assets/ThirdParty/Vehicles/<Name>.glb (or .gltf / .fbx).
        static GameObject Model(string name)
        {
            foreach (var ext in new[] { ".glb", ".gltf", ".fbx" })
                if (AssetDatabase.LoadAssetAtPath<GameObject>($"{TP}/Vehicles/{name}{ext}") is GameObject g) return g;
            return null;
        }

        // New or removed model files in Vehicles/ and the player prefab, put into the saved scene without a full Setup.
        public static void RefreshModels()
        {
            var player = PlayerPrefab.Build();
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ScenePath);
            var loop = UnityEngine.Object.FindAnyObjectByType<GameLoop>();
            loop.art.vehicles = Road.Types.Select(t => new Models { list = ModelsFor(t.ModelKey) }).ToArray();
            if (player) loop.art.player = player;
            EditorUtility.SetDirty(loop);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            Debug.Log("MODELS " + string.Join(", ", loop.art.vehicles.Select(v => v.list.Length)));
        }

        // Every file Vehicles/<key>*.glb (Compact.glb, Compact2.glb, ...): the game picks one at random per spawn.
        static GameObject[] ModelsFor(string key)
        {
            var dir = $"{TP}/Vehicles";
            if (!Directory.Exists(dir)) return new GameObject[0];
            return Directory.GetFiles(dir, key + "*.glb").OrderBy(f => f)
                .Select(f => AssetDatabase.LoadAssetAtPath<GameObject>(f.Replace("\\", "/"))).Where(g => g != null).ToArray();
        }

        // URP mobile settings (docs/dev/unity-plan.md 1, look-spec 4).
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
                a.colorGradingMode = ColorGradingMode.LowDynamicRange;
                a.colorGradingLutSize = 32;
                EditorUtility.SetDirty(a);
            }
        }

        // Logo "Frontal" (docs/design/logo): adaptive icon layers, round and legacy from the 1024 master, themed (monochrome) layer.
        static void SetIcons()
        {
            Texture2D Tex(string name)
            {
                var path = $"Assets/Icon/{name}.png";
                var ti = (TextureImporter)AssetImporter.GetAtPath(path);
                if (ti.textureType != TextureImporterType.Default || ti.mipmapEnabled || ti.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    ti.textureType = TextureImporterType.Default; ti.mipmapEnabled = false; ti.alphaIsTransparency = true; ti.textureCompression = TextureImporterCompression.Uncompressed;
                    ti.SaveAndReimport();
                }
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            // The start-screen wordmark is loaded as a plain texture: keep its exact size, no mipmaps, no compression.
            var wm = (TextureImporter)AssetImporter.GetAtPath("Assets/UI/Resources/Icons/wordmark.png");
            if (wm.npotScale != TextureImporterNPOTScale.None || wm.mipmapEnabled || wm.textureCompression != TextureImporterCompression.Uncompressed)
            {
                wm.textureType = TextureImporterType.Default; wm.npotScale = TextureImporterNPOTScale.None; wm.mipmapEnabled = false;
                wm.alphaIsTransparency = true; wm.textureCompression = TextureImporterCompression.Uncompressed; wm.SaveAndReimport();
            }
            var master = Tex("icon-1024");
            void Fill(PlatformIconKind kind, params Texture2D[] layers)
            {
                var icons = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, kind);
                foreach (var icon in icons) for (int i = 0; i < layers.Length; i++) icon.SetTexture(layers[i], i);
                PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android, kind, icons);
            }
            Fill(AndroidPlatformIconKind.Adaptive, Tex("icon-background-432"), Tex("icon-foreground-432"));
            Fill(AndroidPlatformIconKind.Round, master);
            Fill(AndroidPlatformIconKind.Legacy, master);
            // Themed (monochrome) layer: only where this Unity version gives the adaptive icon a third layer.
            var ad = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Adaptive);
            foreach (var icon in ad) if (icon.maxLayerCount > 2) icon.SetTexture(Tex("icon-monochrome-432"), 2);
            PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android, AndroidPlatformIconKind.Adaptive, ad);
            Debug.Log("ICONS adaptive layers " + ad.Max(i => i.maxLayerCount));
        }

        public static void BuildAndroid()
        {
            bool release = Array.IndexOf(Environment.GetCommandLineArgs(), "-release") >= 0;
            if (!File.Exists(ScenePath)) Setup();

            PlayerSettings.companyName = "uncosens";
            PlayerSettings.productName = "Geisterfahrer";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.uncosens.geisterfahrer");
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            SetIcons();
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
