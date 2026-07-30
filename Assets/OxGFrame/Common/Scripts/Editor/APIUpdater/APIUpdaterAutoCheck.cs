using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.Common.Editor
{
    /// <summary>
    /// 框架版本變更時的 API 更新偵測 (僅偵測與詢問，未經確認不會修改任何檔案)
    /// </summary>
    [InitializeOnLoad]
    internal static class APIUpdaterAutoCheck
    {
        static APIUpdaterAutoCheck()
        {
            // 批次模式 (CI 建置) 不進行任何詢問
            if (Application.isBatchMode)
                return;

            // 延遲至編輯器就緒後才檢查
            EditorApplication.delayCall += _CheckOnVersionChanged;
        }

        private static void _CheckOnVersionChanged()
        {
            string currentVersion = APIUpdater.GetFrameworkVersion();
            if (string.IsNullOrEmpty(currentVersion))
                return;

            string lastVersion = APIUpdater.GetLastCheckedVersion();

            // 版本未變更不做任何處理
            if (string.CompareOrdinal(lastVersion, currentVersion) == 0)
                return;

            // 首次記錄 (新專案或首次安裝) 不打擾使用者
            if (string.IsNullOrEmpty(lastVersion))
            {
                APIUpdater.SaveLastCheckedVersion(currentVersion);
                return;
            }

            // 版本已變更，進行背景掃描 (有任何匹配才詢問，包含需人工複核者)
            var result = APIUpdater.Scan();
            int readyMatchCount = result.GetReadyMatchCount();
            int reviewMatchCount = result.GetReviewMatchCount();

            // 無論選擇皆記錄版本，避免每次重載重複詢問 (仍可由選單手動開啟)
            APIUpdater.SaveLastCheckedVersion(currentVersion);

            if (readyMatchCount == 0 &&
                reviewMatchCount == 0)
                return;

            int fileCount = result.matches
                .Select(match => match.assetPath)
                .Distinct()
                .Count();

            bool openUpdater = EditorUtility.DisplayDialog(
                "OxGFrame API Update",
                $"OxGFrame has been updated ({lastVersion} -> {currentVersion}).\n\n" +
                $"Found {readyMatchCount} update(s) ready to apply and {reviewMatchCount} usage(s) that need review in {fileCount} script(s).\n\n" +
                $"Open the API Updater to review them? Nothing will be modified without your confirmation.",
                "Open API Updater",
                "Later");

            if (openUpdater)
                APIUpdaterWindow.ShowWindow(result);
            else
                Debug.Log($"<color=#ffb824>[OxGFrame] Found {readyMatchCount + reviewMatchCount} deprecated API usage(s). You can review them anytime from the menu [OxGFrame/Others/API Updater].</color>");
        }
    }
}
