using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GraphicRaycaster;

namespace OxGFrame.CoreFrame.Editor
{
    public static class CoreFrameCreatePrefabEditor
    {
        class DoCreatePrefabAsset : EndNameEditAction
        {
            // Subclass and override this method to create specialised prefab asset creation functions
            protected virtual GameObject CreateGameObject(string name)
            {
                return new GameObject(name);
            }

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                GameObject go = CreateGameObject(Path.GetFileNameWithoutExtension(pathName));
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, pathName);
                GameObject.DestroyImmediate(go);
            }
        }

        class DoCreateUIPrefabAsset : DoCreatePrefabAsset
        {
            protected override GameObject CreateGameObject(string name)
            {
                var obj = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));

                // calibrate RectTransform
                RectTransform objRect = obj.GetComponent<RectTransform>();
                objRect.anchorMin = Vector2.zero;
                objRect.anchorMax = Vector2.one;
                objRect.sizeDelta = Vector2.zero;
                objRect.localScale = Vector3.one;
                objRect.localPosition = Vector3.zero;

                // calibrate Canvas
                Canvas objCanvas = obj.GetComponent<Canvas>();
                objCanvas.overridePixelPerfect = true;
                objCanvas.pixelPerfect = true;
                objCanvas.overrideSorting = false;
                objCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;

                // calibrate  Graphic Raycaster
                GraphicRaycaster objGraphicRaycaster = obj.GetComponent<GraphicRaycaster>();
                objGraphicRaycaster.ignoreReversedGraphics = false;
                objGraphicRaycaster.blockingObjects = BlockingObjects.None;
                objGraphicRaycaster.blockingMask = 0;

                return obj;
            }
        }

        static void CreatePrefabAsset(string name, DoCreatePrefabAsset createAction)
        {
            string directory = GetSelectedAssetDirectory();
            string path = Path.Combine(directory, $"{name}.prefab");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, createAction, path, EditorGUIUtility.FindTexture("Prefab Icon"), null);
        }

        static string GetSelectedAssetDirectory()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Directory.Exists(path))
                return path;
            else
                return Path.GetDirectoryName(path);
        }

        [MenuItem("Assets/Create/OxGFrame/Core Frame/GS Frame/Template Prefabs/Template GS (Game Scene Prefab)", isValidateFunction: false, priority: 51)]
        public static void CreateTplGS()
        {
            CreatePrefabAsset("NewTplGS", ScriptableObject.CreateInstance<DoCreatePrefabAsset>());
        }

        [MenuItem("Assets/Create/OxGFrame/Core Frame/UI Frame/Template Prefabs/Template UI (UGUI Prefab)", isValidateFunction: false, priority: 51)]
        public static void CreateTplUI()
        {
            CreatePrefabAsset("NewTplUI", ScriptableObject.CreateInstance<DoCreateUIPrefabAsset>());
        }
    }
}