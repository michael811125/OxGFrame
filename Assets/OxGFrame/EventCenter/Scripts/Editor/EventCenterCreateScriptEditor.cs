using UnityEditor;

namespace OxGFrame.EventCenter.Editor
{
    public static class EventCenterCreateScriptEditor
    {
        // EventCenter
        private const string TPL_EVENT_BASE_SCRIPT_PATH = "TplScripts/EventCenter/TplEvent.cs.txt";
        private const string TPL_EVENT_CENTER_SCRIPT_PATH = "TplScripts/EventCenter/TplEventCenter.cs.txt";

        // find current file path
        private static string pathFinder
        {
            get
            {
                var g = AssetDatabase.FindAssets("t:Script EventCenterCreateScriptEditor");
                return AssetDatabase.GUIDToAssetPath(g[0]);
            }
        }

        #region EventCenter Script Create
        [MenuItem(itemName: "Assets/Create/OxGFrame/EventCenter/TplScripts/TplEvent.cs (Event)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplEventBase()
        {
            string currentPath = pathFinder;
            string finalPath = currentPath.Replace("EventCenterCreateScriptEditor.cs", "") + TPL_EVENT_BASE_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplEvent.cs");
        }

        [MenuItem(itemName: "Assets/Create/OxGFrame/EventCenter/TplScripts/TplEventCenter.cs (Event Center Manager)", isValidateFunction: false, priority: 51)]
        public static void CreateScriptTplEventCenter()
        {
            string currentPath = pathFinder;
            string finalPath = currentPath.Replace("EventCenterCreateScriptEditor.cs", "") + TPL_EVENT_CENTER_SCRIPT_PATH;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplEventCenter.cs");
        }
        #endregion
    }
}
