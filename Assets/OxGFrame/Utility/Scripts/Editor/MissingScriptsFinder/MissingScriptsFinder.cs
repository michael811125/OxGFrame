using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System;
using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace OxGFrame.Utility.MissingFinder.Editor
{
    public static class MissingScriptsFinder
    {
        private const string MENU_ROOT = "MissingScriptsFinder/";
        private const string MENU_SEARCH_IN_LOADED_SCENES = "Search in loaded scenes";
        private const string MENU_SEARCH_IN_BUILD_SETTINGS_SCENES = "Search in build settings scenes";
        private const string MENU_SEARCH_IN_BUILD_SETTINGS_SCENES_INCLUDE_DISABLE = "Search in build settings scenes (include disable)";
        private const string MENU_SEARCH_IN_PROJECT_ALL_SCENES = "Search in project all scenes";
        private const string MENU_SEARCH_IN_ALL_PREFABS = "Search in all prefabs";

        [MenuItem(MENU_ROOT + MENU_SEARCH_IN_LOADED_SCENES)]
        public static void FindMissingScriptsInLoadedScenes()
        {
            BeginSearch(MENU_SEARCH_IN_LOADED_SCENES);
            // Get all loaded scenes
            for (int i = 0, count = EditorSceneManager.loadedSceneCount; i < count; i++)
                SearchInScene(SceneManager.GetSceneAt(i).path, i, count);
            EndSearch();
        }

        [MenuItem(MENU_ROOT + MENU_SEARCH_IN_BUILD_SETTINGS_SCENES)]
        public static void FindMissingScriptsInBuildSettingsScenes()
        {
            BeginSearch(MENU_SEARCH_IN_BUILD_SETTINGS_SCENES);
            // Get all scenes in build settings
            var editorBuildSettingsScenes = EditorBuildSettings.scenes;
            for (int i = 0, count = editorBuildSettingsScenes.Length; i < count; i++)
            {
                if (editorBuildSettingsScenes[i].enabled)
                    SearchInScene(editorBuildSettingsScenes[i].path, i, count);
            }
            EndSearch();
        }

        [MenuItem(MENU_ROOT + MENU_SEARCH_IN_BUILD_SETTINGS_SCENES_INCLUDE_DISABLE)]
        public static void FindMissingScriptsInBuildSettingsScenesIncludeDisable()
        {
            BeginSearch(MENU_SEARCH_IN_BUILD_SETTINGS_SCENES_INCLUDE_DISABLE);
            // Get all scenes in build settings
            var editorBuildSettingsScenes = EditorBuildSettings.scenes;
            for (int i = 0, count = editorBuildSettingsScenes.Length; i < count; i++)
                SearchInScene(editorBuildSettingsScenes[i].path, i, count);
            EndSearch();
        }

        [MenuItem(MENU_ROOT + MENU_SEARCH_IN_PROJECT_ALL_SCENES)]
        public static void FindMissingScriptsInProjectAllScenes()
        {
            BeginSearch(MENU_SEARCH_IN_PROJECT_ALL_SCENES);
            var paths = AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath).ToArray();
            for (int i = 0, count = paths.Length; i < count; i++)
                SearchInScene(paths[i], i, count);
            EndSearch();
        }

        [MenuItem(MENU_ROOT + MENU_SEARCH_IN_ALL_PREFABS)]
        public static void FindMissingScriptsInAllPrefabs()
        {
            BeginSearch(MENU_SEARCH_IN_ALL_PREFABS);
            var allScriptAndDllGUIDs = FindAllScriptAndDllGUIDs();
            var missingScriptPrefabs = new HashSet<string>();
            foreach (var prefabPaths in FindAllPrefabScriptsRef()
                                                 .Where(kv => !allScriptAndDllGUIDs.Contains(kv.Key))
                                                 .Select(kv => kv.Value))
            {
                foreach (var prefabPath in prefabPaths)
                {
                    if (!missingScriptPrefabs.Contains(prefabPath))
                        missingScriptPrefabs.Add(prefabPath);
                }
            }

            //Maybe not missing.
            int notMissingCount = 0;
            missingScriptPrefabs.ForEachIndex((path, index, count) =>
            {
                EditorUtility.DisplayProgressBar("Check missing script prefabs", path, (float)index / count);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!FindMissingScriptsInGo(prefab, false))
                    notMissingCount++;
            });
            Debug.Log("Missing script prefabs count: " + (missingScriptPrefabs.Count - notMissingCount));
            EndSearch();
        }

        private static void BeginSearch(string menuItem)
        {
            Debug.Log("Begin " + menuItem.ToLower());
        }

        private static void EndSearch()
        {
            EditorUtility.ClearProgressBar();
            Debug.Log("Search end");
        }

        private static void SearchInScene(string sceneAssetPath, int index, int count)
        {
            var scene = SceneManager.GetSceneByPath(sceneAssetPath);
            bool needRemove = false;
            //If not loaded, can not GetRootGameObjects
            if (!scene.IsValid())
            {
                needRemove = true;
                try
                {
                    scene = EditorSceneManager.OpenScene(sceneAssetPath, OpenSceneMode.Additive);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return;
                }
            }
            if (!scene.IsValid()) return;

            var gameObjects = scene.GetRootGameObjects();
            string title = $"Check missing scripts ({index}/{count})";
            gameObjects.ForEachIndex((go, index1, count1) =>
            {
                EditorUtility.DisplayProgressBar(title, go.ToString(), (float)index1 / count1);
                FindMissingScriptsInGo(go, needRemove);
            });
            if (needRemove)
                EditorSceneManager.CloseScene(scene, true);
        }

        private static HashSet<string> FindAllScriptAndDllGUIDs()
        {
            //script GUIDs
            var scriptGUIDs = AssetDatabase.FindAssets("t:Script");

            //No need. All MonoBehaviour scripts guid are include in scriptGUIDs.
            // //Project dll GUIDs
            // var projectDllGUIDs = Directory.GetFiles("Assets/", "*.dll.meta", SearchOption.AllDirectories)
            //     .Select(p => File.ReadAllLines(p)[1].Substring(6)).Where(s => !string.IsNullOrEmpty(s));

            //No need
            // //App dll GUIDs
            // var appDllGUIDs = Directory.GetFiles(EditorApplication.applicationContentsPath, "*.dll", SearchOption.AllDirectories)
            //     .Select(p => AssetDatabase.AssetPathToGUID(p.Replace('\\', '/'))).Where(s => !string.IsNullOrEmpty(s));

            return new HashSet<string>(scriptGUIDs/*.Concat(projectDllGUIDs).Concat(appDllGUIDs)*/);
        }

        private static Dictionary<string, HashSet<string>> FindAllPrefabScriptsRef()
        {
            var allPrefabPaths = Directory.GetFiles("Assets/", "*.prefab", SearchOption.AllDirectories);
            EditorUtility.DisplayProgressBar("Scanning prefabs", "", 0);
            //guid prefabPaths
            Dictionary<string, HashSet<string>> references = new Dictionary<string, HashSet<string>>();

            for (int i = 0, length = allPrefabPaths.Length; i < length; ++i)
            {
                var prefabPath = allPrefabPaths[i];
                EditorUtility.DisplayProgressBar("Scanning prefabs", prefabPath, (float)i / length);
                foreach (var line in File.ReadAllLines(prefabPath))
                {
                    string guid = GetScriptGUID(line);
                    if (guid != null)
                    {
                        if (!references.TryGetValue(guid, out var prefabPaths))
                        {
                            prefabPaths = new HashSet<string>();
                            references.Add(guid, prefabPaths);
                        }
                        if (prefabPaths != null && !prefabPaths.Contains(prefabPath))
                            prefabPaths.Add(prefabPath);
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            return references;
        }

        private static bool FindMissingScriptsInGo(GameObject go, bool needSceneRemove)
        {
            var components = go.GetComponents<Component>();
            var transform = go.transform;
            bool missing = false;
            foreach (var c in components)
            {
                // Missing components will be null, we can't find their type, etc.
                if (c) continue;
                missing = true;
                var assetPath = AssetDatabase.GetAssetPath(go);
                //prefab
                if (!string.IsNullOrEmpty(assetPath))
                    Debug.Log("<color=#65ffd5>Missing script:</color> <color=#ff65d1>" + transform.GetTransformPath() + "</color> --> <color=#ffe265>" + assetPath + "</color>", transform.root.gameObject);
                else if (go.scene.IsValid())
                {
                    //scene
                    if (needSceneRemove)
                        Debug.Log("<color=#65ffd5>Missing script:</color> <color=#ff65d1>" + transform.GetTransformPath() + "</color> --> <color=#ffe265>" + go.scene.path + "</color>",
                            AssetDatabase.LoadAssetAtPath<SceneAsset>(go.scene.path));
                    else
                        Debug.Log("<color=#65ffd5>Missing script:</color> <color=#ff65d1>" + transform.GetTransformPath() + "</color>", go);
                }
                else
                    Debug.Log("<color=#65ffd5>Missing script:</color> <color=#ff65d1>" + transform.GetTransformPath() + "</color>", go);
            }

            //Find children
            for (int i = 0, childCount = transform.childCount; i < childCount; i++)
                missing |= FindMissingScriptsInGo(transform.GetChild(i).gameObject, needSceneRemove);
            return missing;
        }

        private static readonly Regex mScriptGUIDRegex = new Regex(@"m_Script: \{fileID: [0-9]+, guid: ([0-9a-f]{32}), type: 3\}");
        private static string GetScriptGUID(string line)
        {
            var m = mScriptGUIDRegex.Match(line);
            if (m.Success)
                return m.Groups[1].Value;
            if (line.Contains("m_Script: {fileID: 0}")) //missing script
                return "0";
            return null;
        }

        #region Extensions
        private static string GetTransformPath(this Transform transform)
        {
            StringBuilder sb = new StringBuilder(transform.name);
            while (transform.parent)
            {
                transform = transform.parent;
                sb.Insert(0, '/');
                sb.Insert(0, transform.name);
            }
            return sb.ToString();
        }

        private static void ForEachIndex<T>(this ICollection<T> source, Action<T, int, int> action)
        {
            int index = 0;
            int count = source.Count;
            foreach (T element in source)
            {
                action(element, index, count);
                index += 1;
            }
        }
        #endregion
    }
}