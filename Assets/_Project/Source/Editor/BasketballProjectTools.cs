using System;
using System.IO;
using Basketball.Application;
using Bootstrap;
using GameScreen.Presentation;
using SplashScreen.Presentation;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditorTools
{
    /// <summary>
    /// One-shot setup: Addressables tuning + gameplay prefabs, UI screen prefabs, bootstrap scene entry,
    /// and optional addressables content build.
    /// </summary>
    public static class BasketballProjectTools
    {
        const string AddressablesBasketballDir = "Assets/_Project/Addressables/Basketball";
        const string TuningPath = AddressablesBasketballDir + "/BasketballTuning.asset";
        const string PrefabDir = "Assets/_Project/Prefabs/UI";
        const string GameplayPrefabDir = "Assets/_Project/Prefabs/Gameplay";
        const string GamePrefabPath = PrefabDir + "/Screen_Game.prefab";
        const string SplashPrefabPath = PrefabDir + "/Screen_Splash.prefab";
        const string PlayFieldPrefabPath = GameplayPrefabDir + "/PlayField.prefab";
        const string BallPrefabPath = GameplayPrefabDir + "/Ball.prefab";
        const string ApplauseCheer1Path = "Assets/_Project/Audio/Sounds/freesound_community-short-crowd-cheer-2-88701.mp3";
        const string ApplauseCheer2Path = "Assets/_Project/Audio/Sounds/freesound_community-short-crowd-cheer-6713.mp3";
        const string HitsEffectsDir = "Assets/_Project/3rdParties/Matthew Guz/Hits Effects FREE/Prefab";
        const string VfxScoreHitBasicPath = HitsEffectsDir + "/Basic Hit .prefab";
        const string VfxScoreHitBasic2Path = HitsEffectsDir + "/Basic Hit 2.prefab";
        const string VfxScoreHitBasic7Path = HitsEffectsDir + "/Basic Hit 7.prefab";
        const string VfxScoreHitLightningBluePath = HitsEffectsDir + "/Lightning Hit Blue.prefab";
        const string VfxScoreHitMagic2Path = HitsEffectsDir + "/Magic Hit 2.prefab";
        const string BootstrapScenePath = "Assets/_Project/Scenes/BootstrapScene.unity";

        [MenuItem("Basketball/Project/Bootstrap Content (Scene + Prefabs + Addressables)")]
        public static void MenuBootstrap() => BootstrapAll(buildAddressablesPlayerContent: true);

        /// <summary>Unity batchmode: -executeMethod EditorTools.BasketballProjectTools.BatchBootstrap</summary>
        public static void BatchBootstrap()
        {
            try
            {
                BootstrapAll(buildAddressablesPlayerContent: true);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                EditorApplication.Exit(1);
                return;
            }

            EditorApplication.Exit(0);
        }

        static void BootstrapAll(bool buildAddressablesPlayerContent)
        {
            EnsureFolder("Assets/_Project/Addressables");
            EnsureFolder(AddressablesBasketballDir);
            EnsureFolder(PrefabDir);
            EnsureFolder(GameplayPrefabDir);
            CreateOrUpdateTuningAsset();
            CreateScreenPrefabIfMissing(GamePrefabPath, "Screen_Game", typeof(GameScreenController));
            CreateScreenPrefabIfMissing(SplashPrefabPath, "Screen_Splash", typeof(SplashScreenController));
            EnsureAppEntryPointInBootstrapScene();
            RegisterAddressablesEntries();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (buildAddressablesPlayerContent)
                AddressableAssetSettings.BuildPlayerContent();

            Debug.Log("[Basketball] Project bootstrap finished.");
        }

        static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
                return;
            var parent = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            var name = Path.GetFileName(assetPath);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
                return;
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        static void CreateOrUpdateTuningAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<BasketballTuningConfig>(TuningPath);
            if (existing != null)
                return;

            var tuning = ScriptableObject.CreateInstance<BasketballTuningConfig>();
            if (tuning.throwChargeWeight == null || tuning.throwChargeWeight.length == 0)
                tuning.throwChargeWeight = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            AssetDatabase.CreateAsset(tuning, TuningPath);
            EditorUtility.SetDirty(tuning);
        }

        static void CreateScreenPrefabIfMissing(string prefabPath, string rootName, Type controllerType)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                return;

            var go = new GameObject(rootName, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            go.AddComponent(controllerType);

            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath) ?? PrefabDir);
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            UnityEngine.Object.DestroyImmediate(go);
        }

        static void EnsureAppEntryPointInBootstrapScene()
        {
            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponent<AppEntryPoint>() != null)
                {
                    EditorSceneManager.SaveScene(scene);
                    return;
                }
            }

            var go = new GameObject("AppEntryPoint");
            go.AddComponent<AppEntryPoint>();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static void RegisterAddressablesEntries()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null)
            {
                Debug.LogError("[Basketball] AddressableAssetSettings could not be created.");
                return;
            }

            var localGroup = settings.DefaultGroup;
            if (localGroup == null)
            {
                Debug.LogError("[Basketball] Addressables DefaultGroup is null.");
                return;
            }

            var uiGroup = settings.FindGroup("UI_Screens");
            if (uiGroup == null)
                uiGroup = settings.CreateGroup("UI_Screens", false, false, true, null, typeof(BundledAssetGroupSchema));

            void Ensure(string path, string address, AddressableAssetGroup group)
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogError($"[Basketball] Missing asset for Addressables: {path}");
                    return;
                }

                var entry = settings.CreateOrMoveEntry(guid, group, false, false);
                entry.SetAddress(address, false);
            }

            Ensure(TuningPath, BasketballAddressKeys.Config, localGroup);
            Ensure(PlayFieldPrefabPath, BasketballAddressKeys.PlayField, localGroup);
            Ensure(BallPrefabPath, BasketballAddressKeys.Ball, localGroup);
            Ensure(ApplauseCheer1Path, BasketballAddressKeys.ApplauseCheerShort1, localGroup);
            Ensure(ApplauseCheer2Path, BasketballAddressKeys.ApplauseCheerShort2, localGroup);
            Ensure(VfxScoreHitBasicPath, BasketballAddressKeys.VfxScoreHitBasic, localGroup);
            Ensure(VfxScoreHitBasic2Path, BasketballAddressKeys.VfxScoreHitBasic2, localGroup);
            Ensure(VfxScoreHitBasic7Path, BasketballAddressKeys.VfxScoreHitBasic7, localGroup);
            Ensure(VfxScoreHitLightningBluePath, BasketballAddressKeys.VfxScoreHitLightningBlue, localGroup);
            Ensure(VfxScoreHitMagic2Path, BasketballAddressKeys.VfxScoreHitMagic2, localGroup);
            Ensure(GamePrefabPath, "Screen_Game", uiGroup);
            Ensure(SplashPrefabPath, "Screen_Splash", uiGroup);

            EditorUtility.SetDirty(settings);
        }
    }
}
