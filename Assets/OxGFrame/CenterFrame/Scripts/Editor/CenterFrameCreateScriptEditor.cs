using UnityEditor;

namespace OxGFrame.CenterFrame.Editor
{
    public static class CenterFrameCreateScriptEditor
    {
        // EventCenter
        private const string TPL_EVENT_BASE_SCRIPT_PATH = "TplScripts/EventCenter/TplEvent.cs.txt";
        private const string TPL_EVENT_CENTER_SCRIPT_PATH = "TplScripts/EventCenter/TplEventCenter.cs.txt";

        // APICenter
        private const string TPL_API_CENTER_SCRIPT_PATH = "TplScripts/APICenter/TplAPICenter.cs.txt";
        private const string TPL_API_BASE_SCRIPT_PATH = "TplScripts/APICenter/TplAPI.cs.txt";

        // find current file path
        private static string _pathFinder
        {
            get
            {
                var g = AssetDatabase.FindAssets("t:Script CenterFrameCreateScriptEditor");
                return AssetDatabase.GUIDToAssetPath(g[0]);
            }
        }

        #region EventCenter Script Create
        [MenuItem(itemName: "Assets/Create/OxGFrame/Center Frame/Event Center/Template Scripts/Template Event.cs (Event)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplEventBase()
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("CenterFrameCreateScriptEditor.cs", "") + TPL_EVENT_BASE_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplEvent.cs");
        }

        [MenuItem(itemName: "Assets/Create/OxGFrame/Center Frame/Event Center/Template Scripts/Template EventCenter.cs (Event Center Manager)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplEventCenter()
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("CenterFrameCreateScriptEditor.cs", "") + TPL_EVENT_CENTER_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplEventCenter.cs");
        }
        #endregion

        #region APICenter Script Create
        [MenuItem(itemName: "Assets/Create/OxGFrame/Center Frame/API Center/Template Scripts/Template API.cs (API)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplAPIBase()
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("CenterFrameCreateScriptEditor.cs", "") + TPL_API_BASE_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplAPI.cs");
        }

        [MenuItem(itemName: "Assets/Create/OxGFrame/Center Frame/API Center/Template Scripts/Template APICenter.cs (API Center Manager)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplAPICenter()
        {
            string currentPath = _pathFinder;
            string finalPath = currentPath.Replace("CenterFrameCreateScriptEditor.cs", "") + TPL_API_CENTER_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplAPICenter.cs");
        }
        #endregion
    }
}