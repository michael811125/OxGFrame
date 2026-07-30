using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.Common.Editor
{
    /// <summary>
    /// API 更新器 (掃描專案中的過時 API，並於預覽確認後套用更新)
    /// </summary>
    public static class APIUpdater
    {
        /// <summary>
        /// 型別上下文的驗證結果
        /// </summary>
        private enum ContextVerification
        {
            /// <summary>
            /// 與規則的宣告型別無關 (不列入結果)
            /// </summary>
            Unrelated,

            /// <summary>
            /// 已由反射驗證衍生關係
            /// </summary>
            Verified,

            /// <summary>
            /// 僅由基底子句文字推測 (需人工複核)
            /// </summary>
            TextOnly
        }

        /// <summary>
        /// 套用結果
        /// </summary>
        public class ApplyResult
        {
            /// <summary>
            /// 實際替換的匹配數量
            /// </summary>
            public int appliedCount;

            /// <summary>
            /// 略過的匹配數量 (檔案已變更或缺少可套用的新 API)
            /// </summary>
            public int skippedCount;

            /// <summary>
            /// 實際變更的檔案數量
            /// </summary>
            public int changedFileCount;
        }

        /// <summary>
        /// 記錄上次檢查的框架版本 (儲存於 Library/EditorUserSettings.asset，專案內有效)
        /// </summary>
        private const string _LAST_CHECKED_VERSION_KEY = "OxGFrame.APIUpdater.LastCheckedVersion";

        /// <summary>
        /// 框架組件名稱前綴
        /// </summary>
        private const string _ASSEMBLY_PREFIX = "OxGFrame.";

        /// <summary>
        /// 框架的套件名稱 (向上查找根目錄時驗證用，避免誤認各模組內嵌的子套件描述檔)
        /// </summary>
        private const string _PACKAGE_NAME = "com.michaelo.oxgframe";

        /// <summary>
        /// 更新器自身的腳本名稱 (框架根目錄偵測失敗時的第二層防護，避免規則表字串被替換)
        /// </summary>
        private static readonly HashSet<string> _SELF_FILE_NAMES = new HashSet<string>()
        {
            $"{nameof(APIUpdater)}.cs",
            $"{nameof(APIUpdateRule)}.cs",
            $"{nameof(APIUpdateRules)}.cs",
            $"{nameof(APIUpdateRulesSettings)}.cs",
            $"{nameof(APIUpdaterWindow)}.cs",
            $"{nameof(APIUpdaterAutoCheck)}.cs"
        };

        #region MenuItem
        [MenuItem("OxGFrame/Others/API Updater (Scan and update deprecated APIs)", false, 999)]
        public static void OpenAPIUpdater()
        {
            var result = Scan();
            APIUpdaterWindow.ShowWindow(result);
        }
        #endregion

        #region Scan
        /// <summary>
        /// 掃描專案中的過時 API
        /// </summary>
        /// <returns></returns>
        public static APIUpdateScanResult Scan()
        {
            var result = new APIUpdateScanResult();
            result.frameworkRoot = GetFrameworkRootAssetPath();
            result.frameworkVersion = GetFrameworkVersion();

            APIUpdateRules.CollectRules(out var rules, out var manualOnlyRules);
            result.rules = rules;
            result.manualOnlyRules = manualOnlyRules;

            if (rules.Count == 0)
                return result;

            // 以 AssetDatabase 列舉 Unity 實際可見的腳本 (自動排除 ~ 結尾資料夾等 Unity 忽略的內容)
            string[] guids;
            try
            {
                guids = AssetDatabase.FindAssets("t:MonoScript");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return result;
            }

            string frameworkRootPrefix = string.IsNullOrEmpty(result.frameworkRoot) ? null : $"{result.frameworkRoot}/";

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // 僅掃描專案自身的腳本
                if (string.IsNullOrEmpty(assetPath) ||
                    !assetPath.StartsWith("Assets/", StringComparison.Ordinal) ||
                    !assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    continue;

                // 排除框架源碼
                if (frameworkRootPrefix != null &&
                    assetPath.StartsWith(frameworkRootPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (_SELF_FILE_NAMES.Contains(Path.GetFileName(assetPath)))
                    continue;

                // 取得腳本的型別上下文 (同時作為框架組件的第二層排除依據)
                Type scriptType = _GetScriptType(assetPath);
                if (scriptType != null &&
                    _IsFrameworkType(scriptType))
                    continue;

                string content;
                try
                {
                    content = File.ReadAllText(assetPath);
                }
                catch
                {
                    /* Nothing to do */
                    continue;
                }

                result.scannedFileCount++;

                // 快速預篩, 僅在檔案含有任一舊 API 文字時才進行完整比對
                var hitRules = rules.Where(rule => content.IndexOf(rule.oldApi, StringComparison.Ordinal) >= 0).ToArray();
                if (hitRules.Length == 0)
                    continue;

                bool[] codeMask = _BuildCodeMask(content);
                int[] lineStarts = _BuildLineStarts(content);

                // 同一位置僅保留先到的規則, 避免重複替換
                var occupiedSpans = new HashSet<long>();

                foreach (var rule in hitRules)
                {
                    // 規則層級的複核原因 (適用於該檔案的所有匹配)
                    var ruleReviewReasons = new List<string>();

                    if (rule.alwaysNeedsReview &&
                        !string.IsNullOrEmpty(rule.reviewNote))
                        ruleReviewReasons.Add(rule.reviewNote);

                    // 繼承成員必須驗證使用者類別確實衍生自宣告型別
                    if (rule.matchKind == APIUpdateRule.MatchKind.InheritedMember)
                    {
                        var verification = _VerifyInheritedContext(scriptType, content, rule);
                        if (verification == ContextVerification.Unrelated)
                            continue;

                        if (verification == ContextVerification.TextOnly)
                            ruleReviewReasons.Add($"The type context could not be verified by reflection (base clause of {rule.declaringTypeName} found by text only)");
                        else if (rule.isCommonName)
                            ruleReviewReasons.Add($"The member name is common, please confirm it belongs to {rule.declaringTypeName}");
                    }

                    // 成員存取形式建議確認宣告型別是否出現於同一檔案
                    if (rule.matchKind == APIUpdateRule.MatchKind.MemberAccess &&
                        !string.IsNullOrEmpty(rule.declaringTypeName) &&
                        content.IndexOf(rule.declaringTypeName, StringComparison.Ordinal) < 0)
                        ruleReviewReasons.Add($"{rule.declaringTypeName} is not referenced in this file");

                    MatchCollection regexMatches;
                    try
                    {
                        regexMatches = rule.GetRegex().Matches(content);
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        Debug.LogWarning($"[OxGFrame] APIUpdater: Regex timeout on {assetPath} for rule {rule}. Skipped.");
                        continue;
                    }

                    foreach (Match match in regexMatches)
                    {
                        long spanKey = ((long)match.Index << 32) | (uint)match.Length;
                        if (!occupiedSpans.Add(spanKey))
                            continue;

                        var reviewReasons = new List<string>(ruleReviewReasons);

                        // 歧義規則依呼叫引數自動決議新 API (與編譯器的多載決議一致)
                        string resolvedNewApi = null;
                        string resolutionNote = null;
                        if (rule.isAmbiguous)
                        {
                            resolvedNewApi = _TryResolveAmbiguousNewApi(content, match.Index + match.Length, rule);
                            if (resolvedNewApi != null)
                                resolutionNote = "Resolved by call arguments";
                            else
                                reviewReasons.Add($"Ambiguous overloads ({rule.newApi}), please choose the new API from the dropdown");
                        }

                        bool isInCommentOrString = !codeMask[match.Index];
                        if (isInCommentOrString)
                            reviewReasons.Add("Inside a comment or string literal");

                        bool needsReview = reviewReasons.Count > 0;
                        int lineNumber = _GetLineNumber(lineStarts, match.Index);
                        result.matches.Add(new APIUpdateMatch()
                        {
                            filePath = Path.GetFullPath(assetPath),
                            assetPath = assetPath,
                            lineNumber = lineNumber,
                            startIndex = match.Index,
                            length = match.Length,
                            lineText = _GetLineText(content, lineStarts, lineNumber),
                            isInCommentOrString = isInCommentOrString,
                            needsReview = needsReview,
                            reviewReason = needsReview ? string.Join("; ", reviewReasons) : null,
                            resolvedNewApi = resolvedNewApi,
                            resolutionNote = resolutionNote,
                            rule = rule,
                            selected = !needsReview
                        });
                    }
                }
            }

            // 依檔案與位置排序, 讓預覽結果穩定
            result.matches = result.matches
                .OrderBy(match => match.assetPath, StringComparer.Ordinal)
                .ThenBy(match => match.startIndex)
                .ToList();

            return result;
        }

        /// <summary>
        /// 依呼叫引數嘗試決議歧義規則的新 API
        /// <para>空括號呼叫必然繫結無必填參數的多載；帶引數呼叫必然繫結具參數的多載，與編譯器的多載決議一致。</para>
        /// <para>非呼叫形式 (方法群組、nameof 等) 或仍有多個候選時回傳 null，交由使用者於下拉選單指定。</para>
        /// </summary>
        /// <param name="content"></param>
        /// <param name="matchEndIndex"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        private static string _TryResolveAmbiguousNewApi(string content, int matchEndIndex, APIUpdateRule rule)
        {
            if (rule.candidates == null ||
                rule.candidates.Count == 0)
                return null;

            int index = matchEndIndex;
            while (index < content.Length && char.IsWhiteSpace(content[index]))
                index++;

            // 非呼叫形式無法由引數決議
            if (index >= content.Length ||
                content[index] != '(')
                return null;

            index++;
            while (index < content.Length && char.IsWhiteSpace(content[index]))
                index++;

            bool isEmptyCall = index < content.Length && content[index] == ')';

            List<APIUpdateCandidate> matchedCandidates;
            if (isEmptyCall)
                matchedCandidates = rule.candidates.Where(candidate => candidate.requiredParameterCount == 0).ToList();
            else
                matchedCandidates = rule.candidates.Where(candidate => candidate.parameterCount > 0).ToList();

            return (matchedCandidates.Count == 1) ? matchedCandidates[0].newApi : null;
        }

        /// <summary>
        /// 驗證腳本的型別上下文是否衍生自規則的宣告型別
        /// </summary>
        /// <param name="scriptType"></param>
        /// <param name="content"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        private static ContextVerification _VerifyInheritedContext(Type scriptType, string content, APIUpdateRule rule)
        {
            // 反射驗證 (最可靠)
            if (scriptType != null &&
                APIUpdateRules.IsDerivedFrom(scriptType, rule.declaringType))
                return ContextVerification.Verified;

            // 檔案可能含有多個型別或名稱與檔名不符, 退而檢查基底子句文字
            if (!string.IsNullOrEmpty(rule.declaringTypeName))
            {
                string pattern = $@"\b(?:class|struct)\s+\w+\s*(?:<[^>]*>)?\s*:\s*[^{{]*\b{Regex.Escape(rule.declaringTypeName)}\b";
                try
                {
                    if (Regex.IsMatch(content, pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2.0)))
                        return ContextVerification.TextOnly;
                }
                catch (RegexMatchTimeoutException)
                {
                    /* Nothing to do */
                }
            }

            return ContextVerification.Unrelated;
        }
        #endregion

        #region Apply
        /// <summary>
        /// 套用勾選的更新項
        /// </summary>
        /// <param name="matches"></param>
        /// <returns></returns>
        public static ApplyResult Apply(IEnumerable<APIUpdateMatch> matches)
        {
            var result = new ApplyResult();
            if (matches == null)
                return result;

            var selectedMatches = matches.Where(match => match.selected).ToArray();
            if (selectedMatches.Length == 0)
                return result;

            var logBuilder = new StringBuilder();

            foreach (var group in selectedMatches.GroupBy(match => match.assetPath))
            {
                string assetPath = group.Key;

                string content;
                try
                {
                    content = File.ReadAllText(assetPath);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    continue;
                }

                // 保留原始檔案的 BOM 狀態 (換行符不做任何正規化)
                bool hasBom = _HasBom(assetPath);
                bool dirty = false;

                // 由後往前替換, 避免索引位移
                foreach (var match in group.OrderByDescending(match => match.startIndex))
                {
                    // 歧義規則必須已具備決議後的新 API
                    string effectiveNewApi = match.GetEffectiveNewApi();
                    if (string.IsNullOrEmpty(effectiveNewApi))
                    {
                        result.skippedCount++;
                        logBuilder.AppendLine($"Skipped (no new API chosen): {assetPath}:{match.lineNumber} => {match.rule}");
                        continue;
                    }

                    // 二次確認該位置的文字仍與規則相符 (防止掃描後檔案已被外部修改)
                    if (match.startIndex < 0 ||
                        match.startIndex + match.length > content.Length ||
                        string.CompareOrdinal(content.Substring(match.startIndex, match.length), match.rule.oldApi) != 0)
                    {
                        result.skippedCount++;
                        logBuilder.AppendLine($"Skipped (file changed since scan): {assetPath}:{match.lineNumber} => {match.rule}");
                        continue;
                    }

                    content = content.Remove(match.startIndex, match.length).Insert(match.startIndex, effectiveNewApi);
                    result.appliedCount++;
                    dirty = true;
                    logBuilder.AppendLine($"Updated: {assetPath}:{match.lineNumber} => {match.rule.oldApi} -> {effectiveNewApi}");
                }

                if (!dirty)
                    continue;

                try
                {
                    File.WriteAllText(assetPath, content, new UTF8Encoding(hasBom));
                    result.changedFileCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            logBuilder.AppendLine($"API update completed. Applied: {result.appliedCount}, Skipped: {result.skippedCount}, Changed files: {result.changedFileCount}.");
            Debug.Log($"<color=#02ff8e>{logBuilder}</color>");

            if (result.changedFileCount > 0)
                AssetDatabase.Refresh();

            // 更新完成後記錄當前版本
            SaveLastCheckedVersion(GetFrameworkVersion());

            return result;
        }
        #endregion

        #region Version
        /// <summary>
        /// 取得框架源碼的根目錄 (資源路徑形式)
        /// </summary>
        /// <returns></returns>
        public static string GetFrameworkRootAssetPath()
        {
            // 由自身腳本位置往上尋找框架的 package.json (對內嵌於 Assets 與 UPM 安裝皆有效)
            // 各模組資料夾內另有子套件描述檔 (例如 com.michaelo.oxgframe.coreframe), 必須驗證套件名稱才能找到真正的框架根目錄
            try
            {
                foreach (string guid in AssetDatabase.FindAssets($"{nameof(APIUpdater)} t:MonoScript"))
                {
                    string scriptAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileName(scriptAssetPath) != $"{nameof(APIUpdater)}.cs")
                        continue;

                    string directory = Path.GetDirectoryName(scriptAssetPath).Replace('\\', '/');
                    while (!string.IsNullOrEmpty(directory) &&
                           directory.IndexOf('/') > 0)
                    {
                        if (_IsFrameworkPackageJson($"{directory}/package.json"))
                            return directory;
                        directory = Path.GetDirectoryName(directory).Replace('\\', '/');
                    }
                }
            }
            catch
            {
                /* Nothing to do */
            }

            // 回退透過 PackageInfo 取得 (以 UPM 安裝時適用)
            try
            {
                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(APIUpdater).Assembly);
                if (packageInfo != null &&
                    !string.IsNullOrEmpty(packageInfo.assetPath))
                    return packageInfo.assetPath;
            }
            catch
            {
                /* Nothing to do */
            }

            return string.Empty;
        }

        /// <summary>
        /// 取得框架版本號
        /// </summary>
        /// <returns></returns>
        public static string GetFrameworkVersion()
        {
            // 以實際運行的源碼目錄為準, 直接讀取 package.json
            try
            {
                string frameworkRoot = GetFrameworkRootAssetPath();
                if (!string.IsNullOrEmpty(frameworkRoot))
                {
                    string packagePath = $"{frameworkRoot}/package.json";
                    if (File.Exists(packagePath))
                    {
                        var match = Regex.Match(File.ReadAllText(packagePath), @"""version""\s*:\s*""([^""]+)""");
                        if (match.Success)
                            return match.Groups[1].Value;
                    }
                }
            }
            catch
            {
                /* Nothing to do */
            }

            // 回退透過 PackageInfo 取得
            try
            {
                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(APIUpdater).Assembly);
                if (packageInfo != null &&
                    !string.IsNullOrEmpty(packageInfo.version))
                    return packageInfo.version;
            }
            catch
            {
                /* Nothing to do */
            }

            return string.Empty;
        }

        /// <summary>
        /// 取得上次檢查的框架版本
        /// </summary>
        /// <returns></returns>
        public static string GetLastCheckedVersion()
        {
            return EditorUserSettings.GetConfigValue(_LAST_CHECKED_VERSION_KEY) ?? string.Empty;
        }

        /// <summary>
        /// 記錄上次檢查的框架版本
        /// </summary>
        /// <param name="version"></param>
        public static void SaveLastCheckedVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                return;
            EditorUserSettings.SetConfigValue(_LAST_CHECKED_VERSION_KEY, version);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// 檢查指定路徑是否為框架自身的 package.json
        /// </summary>
        /// <param name="packagePath"></param>
        /// <returns></returns>
        private static bool _IsFrameworkPackageJson(string packagePath)
        {
            try
            {
                if (!File.Exists(packagePath))
                    return false;
                var match = Regex.Match(File.ReadAllText(packagePath), @"""name""\s*:\s*""([^""]+)""");
                return match.Success && match.Groups[1].Value == _PACKAGE_NAME;
            }
            catch
            {
                /* Nothing to do */
                return false;
            }
        }

        /// <summary>
        /// 取得腳本的型別
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        private static Type _GetScriptType(string assetPath)
        {
            try
            {
                var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                return monoScript?.GetClass();
            }
            catch
            {
                /* Nothing to do */
                return null;
            }
        }

        /// <summary>
        /// 檢查型別是否屬於框架組件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool _IsFrameworkType(Type type)
        {
            try
            {
                string assemblyName = type.Assembly.GetName().Name;
                return assemblyName != null && assemblyName.StartsWith(_ASSEMBLY_PREFIX, StringComparison.Ordinal);
            }
            catch
            {
                /* Nothing to do */
                return false;
            }
        }

        /// <summary>
        /// 檢查檔案是否含有 UTF-8 BOM
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static bool _HasBom(string filePath)
        {
            try
            {
                using (var fs = File.OpenRead(filePath))
                {
                    byte[] preamble = new byte[3];
                    if (fs.Read(preamble, 0, 3) == 3)
                        return preamble[0] == 0xEF && preamble[1] == 0xBB && preamble[2] == 0xBF;
                }
            }
            catch
            {
                /* Nothing to do */
            }
            return false;
        }

        /// <summary>
        /// 建立程式碼遮罩 (true 表示該字元屬於實際程式碼，false 表示位於註解或字串常值中)
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static bool[] _BuildCodeMask(string content)
        {
            bool[] mask = new bool[content.Length + 1];
            int index = 0;

            while (index < content.Length)
            {
                char current = content[index];
                char next = (index + 1 < content.Length) ? content[index + 1] : '\0';

                // 行註解
                if (current == '/' && next == '/')
                {
                    while (index < content.Length && content[index] != '\n')
                        index++;
                    continue;
                }

                // 區塊註解
                if (current == '/' && next == '*')
                {
                    index += 2;
                    while (index < content.Length)
                    {
                        if (content[index] == '*' && index + 1 < content.Length && content[index + 1] == '/')
                        {
                            index += 2;
                            break;
                        }
                        index++;
                    }
                    continue;
                }

                // 逐字字串 (@"..." 以連續雙引號表示引號本身)
                if (current == '@' && next == '"')
                {
                    index += 2;
                    while (index < content.Length)
                    {
                        if (content[index] == '"')
                        {
                            if (index + 1 < content.Length && content[index + 1] == '"')
                            {
                                index += 2;
                                continue;
                            }
                            index++;
                            break;
                        }
                        index++;
                    }
                    continue;
                }

                // 一般字串與內插字串
                if (current == '"')
                {
                    index++;
                    while (index < content.Length)
                    {
                        if (content[index] == '\\')
                        {
                            index += 2;
                            continue;
                        }
                        if (content[index] == '"')
                        {
                            index++;
                            break;
                        }
                        if (content[index] == '\n')
                            break;
                        index++;
                    }
                    continue;
                }

                // 字元常值
                if (current == '\'')
                {
                    index++;
                    while (index < content.Length)
                    {
                        if (content[index] == '\\')
                        {
                            index += 2;
                            continue;
                        }
                        if (content[index] == '\'')
                        {
                            index++;
                            break;
                        }
                        if (content[index] == '\n')
                            break;
                        index++;
                    }
                    continue;
                }

                mask[index] = true;
                index++;
            }

            return mask;
        }

        /// <summary>
        /// 建立每行起始索引表
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static int[] _BuildLineStarts(string content)
        {
            var lineStarts = new List<int>() { 0 };
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '\n')
                    lineStarts.Add(i + 1);
            }
            return lineStarts.ToArray();
        }

        /// <summary>
        /// 取得索引所在的行號 (1 起算)
        /// </summary>
        /// <param name="lineStarts"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static int _GetLineNumber(int[] lineStarts, int index)
        {
            int low = 0;
            int high = lineStarts.Length - 1;
            while (low < high)
            {
                int middle = (low + high + 1) / 2;
                if (lineStarts[middle] <= index)
                    low = middle;
                else
                    high = middle - 1;
            }
            return low + 1;
        }

        /// <summary>
        /// 取得指定行的原文
        /// </summary>
        /// <param name="content"></param>
        /// <param name="lineStarts"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        private static string _GetLineText(string content, int[] lineStarts, int lineNumber)
        {
            int start = lineStarts[lineNumber - 1];
            int end = (lineNumber < lineStarts.Length) ? lineStarts[lineNumber] : content.Length;
            return content.Substring(start, end - start).TrimEnd('\n', '\r').Trim();
        }
        #endregion
    }
}
