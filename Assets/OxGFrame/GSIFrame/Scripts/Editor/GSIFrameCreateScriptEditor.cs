using UnityEngine;
using UnityEditor;

public static class GSIFrameCreateScriptEditor
{
    // Template Game System Integration Manager Path
    private const string TPL_GSIM_PATH = "TplScripts/TplGSIM.cs.txt";
    // Template Game Stage Path
    private const string TPL_GSTAGE_PATH = "TplScripts/TplGStage.cs.txt";

    // find current file path
    private static string pathFinder
    {
        get
        {
            var g = AssetDatabase.FindAssets("t:Script GSIFrameCreateScriptEditor");
            return AssetDatabase.GUIDToAssetPath(g[0]);
        }
    }

    #region GSIFrame Script Create
    [MenuItem(itemName: "Assets/Create/OxGFrame/GSIFrame/TplGStage.cs (Game Stage)", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplGameStage()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("GSIFrameCreateScriptEditor.cs", "") + TPL_GSTAGE_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplGStage.cs");
    }

    [MenuItem(itemName: "Assets/Create/OxGFrame/GSIFrame/TplGSIM.cs (Game System Integration Manager)", isValidateFunction: false, priority: 51)]
    public static void CreateScriptGameFrame()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("GSIFrameCreateScriptEditor.cs", "") + TPL_GSIM_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplGSIM.cs");
    }
    #endregion
}