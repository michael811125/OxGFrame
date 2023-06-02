using UnityEditor;

namespace OxGFrame.CoreFrame.Editor
{
    public static class CoreFrameCreateScriptEditor
    {
        // SRFrame
        private const string TPL_SR_SCRIPT_PATH = "TplScripts/SRFrame/TplSR.cs.txt";

        // UIFrame
        private const string TPL_UI_SCRIPT_PATH = "TplScripts/UIFrame/TplUI.cs.txt";

        // CPFrame
        private const string TPL_CP_SCRIPT_PATH = "TplScripts/CPFrame/TplCP.cs.txt";

        // find current file path
        private static string pathFinder
        {
            get
            {
                var g = AssetDatabase.FindAssets("t:Script CoreFrameCreateScriptEditor");
                return AssetDatabase.GUIDToAssetPath(g[0]);
            }
        }

        #region SRFrame Script Create
        [MenuItem(itemName: "Assets/Create/OxGFrame/Core Frame/SR Frame/Template Scripts/Template SR.cs (For Scene Resource Prefab)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplSR()
        {
            string currentPath = pathFinder;
            string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_SR_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplSR.cs");
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

        #region CPFrame Script Create
        [MenuItem(itemName: "Assets/Create/OxGFrame/Core Frame/CP Frame/Template Scripts/Template CP.cs (For Clone Prefab)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplCP()
        {
            string currentPath = pathFinder;
            string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_CP_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplCP.cs");
        }
        #endregion
    }
}