using UnityEditor;

public static class GSIFrameCreateScriptEditor
{
    // Template Game Stage Manager Path
    private const string TPL_GSM_SCRIPT_PATH = "TplScripts/GSIFrame/TplGameStageManager.cs.txt";
    // Template Game Stage Path
    private const string TPL_GSTAGE_SCRIPT_PATH = "TplScripts/GSIFrame/TplGameStage.cs.txt";

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
    [MenuItem(itemName: "Assets/Create/OxGFrame/GSIFrame/TplScripts/TplGameStage.cs (Game Stage)", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplGStage()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("GSIFrameCreateScriptEditor.cs", "") + TPL_GSTAGE_SCRIPT_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplGameStage.cs");
    }

    [MenuItem(itemName: "Assets/Create/OxGFrame/GSIFrame/TplScripts/TplGameStageManager.cs (Game Stage Manager)", isValidateFunction: false, priority: 51)]
    public static void CreateScriptTplGSM()
    {
        string currentPath = pathFinder;
        string finalPath = currentPath.Replace("GSIFrameCreateScriptEditor.cs", "") + TPL_GSM_SCRIPT_PATH;

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(finalPath, "NewTplGameStageManager.cs");
    }
    #endregion
}