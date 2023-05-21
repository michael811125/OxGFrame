using UnityEditor;

namespace OxGFrame.CoreFrame.Editor
{
    public static class CoreFrameCreateScriptEditor
    {
        // GSFrame
        private const string TPL_GS_SCRIPT_PATH = "TplScripts/GSFrame/TplGS.cs.txt";

        // UIFrame
        private const string TPL_UI_SCRIPT_PATH = "TplScripts/UIFrame/TplUI.cs.txt";

        // EPFrame
        private const string TPL_EP_SCRIPT_PATH = "TplScripts/EPFrame/TplEP.cs.txt";

        // find current file path
        private static string pathFinder
        {
            get
            {
                var g = AssetDatabase.FindAssets("t:Script CoreFrameCreateScriptEditor");
                return AssetDatabase.GUIDToAssetPath(g[0]);
            }
        }

        #region GSFrame Script Create
        [MenuItem(itemName: "Assets/Create/OxGFrame/Core Frame/GS Frame/Template Scripts/Template GS.cs (For Game Scene Prefab)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplGS()
        {
            string currentPath = pathFinder;
            string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_GS_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplGS.cs");
        }
        #endregion

        #region UIFrame Script Create
        [MenuItem(itemName: "Assets/Create/OxGFrame/Core Frame/UI Frame/Template Scripts/Template UI.cs (For UGUI Prefab)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplUI()
        {
            string currentPath = pathFinder;
            string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_UI_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplUI.cs");
        }
        #endregion

        #region EPFrame Script Create
        [MenuItem(itemName: "Assets/Create/OxGFrame/Core Frame/EP Frame/Template Scripts/Template EP.cs (For Entity Prefab)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplEP()
        {
            string currentPath = pathFinder;
            string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_EP_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplEP.cs");
        }
        #endregion
    }
}