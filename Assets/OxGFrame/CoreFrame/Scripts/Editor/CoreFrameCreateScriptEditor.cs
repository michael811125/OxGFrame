using UnityEngine;
using UnityEditor;

public static class CoreFrameCreateScriptEditor
{
    // GSFrame
    private const string TPL_GS_SCRIPT_PATH = "TplScripts/GSFrame/TplGS.cs.txt";

    // UIFrame
    private const string TPL_UI_SCRIPT_PATH = "TplScripts/UIFrame/TplUI.cs.txt";

    // EntityFrame
    private const string TPL_ENTITY_PATH = "TplScripts/EntityFrame/TplEntity.cs.txt";

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
    [MenuItem(itemName: "Assets/Create/OxGFrame/CoreFrame/GSFrame/TplScripts/TplGS.cs", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplGS()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_GS_SCRIPT_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplGS.cs");
    }
    #endregion

    #region UIFrame Script Create
    [MenuItem(itemName: "Assets/Create/OxGFrame/CoreFrame/UIFrame/TplScripts/TplUI.cs", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplUI()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_UI_SCRIPT_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplUI.cs");
    }
    #endregion

    #region EntityFrame Script Create
    [MenuItem(itemName: "Assets/Create/OxGFrame/CoreFrame/EntityFrame/TplScripts/TplEntity.cs", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplEntity()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("CoreFrameCreateScriptEditor.cs", "") + TPL_ENTITY_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplEntity.cs");
    }
    #endregion
}