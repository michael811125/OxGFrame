using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.Common.Editor
{
    /// <summary>
    /// API 更新器預覽視窗 (勾選確認後才會實際寫入檔案)
    /// </summary>
    public class APIUpdaterWindow : EditorWindow
    {
        private static readonly Vector2 _MIN_SIZE = new Vector2(860f, 480f);

        /// <summary>
        /// 掃描結果 (跨組件重載後會遺失，需重新掃描)
        /// </summary>
        private APIUpdateScanResult _result;

        /// <summary>
        /// 是否顯示需人工複核的匹配
        /// </summary>
        private bool _showReviewMatches = false;

        /// <summary>
        /// 是否展開規則總覽
        /// </summary>
        private bool _showAllRules = false;

        /// <summary>
        /// 是否展開需人工遷移的規則清單
        /// </summary>
        private bool _showManualOnlyRules = false;

        /// <summary>
        /// 各檔案的折疊狀態
        /// </summary>
        private readonly Dictionary<string, bool> _fileFoldouts = new Dictionary<string, bool>();

        /// <summary>
        /// 延遲執行的操作 (於下一次繪製前執行，避免在佈局流程中變更集合)
        /// </summary>
        private Action _pendingAction;

        private Vector2 _scrollPosition;

        public static void ShowWindow(APIUpdateScanResult result)
        {
            var window = GetWindow<APIUpdaterWindow>(true, "OxGFrame API Updater", true);
            window.minSize = _MIN_SIZE;
            window._result = result;
            window._fileFoldouts.Clear();
            window.Show();
        }

        private void OnGUI()
        {
            // 先處理延遲操作 (在任何佈局開始前)
            if (this._pendingAction != null)
            {
                var action = this._pendingAction;
                this._pendingAction = null;
                action.Invoke();
            }

            if (this._result == null)
            {
                EditorGUILayout.HelpBox("The scan result is no longer available (scripts were reloaded). Please scan again.", MessageType.Info);
                if (GUILayout.Button("Scan", GUILayout.Height(24f)))
                    this._Defer(() => this._result = APIUpdater.Scan());
                return;
            }

            this._DrawHeader();
            this._DrawAllRules();
            this._DrawManualOnlyRules();
            this._DrawToolbar();

            if (this._result.matches.Count == 0)
            {
                EditorGUILayout.HelpBox("APIs are already up-to-date. No deprecated API usage was found in the project scripts.", MessageType.Info);
                return;
            }

            this._DrawMatches();
            this._DrawFooter();
        }

        private void _DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                string version = string.IsNullOrEmpty(this._result.frameworkVersion) ? "unknown" : this._result.frameworkVersion;
                string frameworkRoot = string.IsNullOrEmpty(this._result.frameworkRoot) ? "not detected" : this._result.frameworkRoot;
                int obsoleteRuleCount = this._result.rules.Count(rule => rule.sourceType == APIUpdateRule.SourceType.Obsolete);
                int manualRuleCount = this._result.rules.Count - obsoleteRuleCount;

                EditorGUILayout.LabelField($"OxGFrame Version: {version}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Framework Source: {frameworkRoot} (excluded from the scan)");
                EditorGUILayout.LabelField($"Rules: {this._result.rules.Count} (auto from [Obsolete]: {obsoleteRuleCount}, manual: {manualRuleCount})");
                EditorGUILayout.LabelField($"Scanned Scripts: {this._result.scannedFileCount}");
                EditorGUILayout.LabelField($"Matches: {this._result.GetReadyMatchCount()} ready to apply, {this._result.GetReviewMatchCount()} need review");
            }
            EditorGUILayout.EndVertical();
        }

        private void _DrawAllRules()
        {
            if (this._result.rules.Count == 0)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                bool showAllRules = EditorGUILayout.Foldout(this._showAllRules, $"Update rules ({this._result.rules.Count}) — every rule the scanner is matching against", true);
                if (showAllRules != this._showAllRules)
                    this._Defer(() => this._showAllRules = showAllRules);

                if (this._showAllRules)
                {
                    foreach (var rule in this._result.rules.OrderBy(rule => rule.declaringTypeName, StringComparer.Ordinal).ThenBy(rule => rule.oldApi, StringComparer.Ordinal))
                    {
                        string sinceVersion = string.IsNullOrEmpty(rule.sinceVersion) ? string.Empty : $"   (since {rule.sinceVersion})";
                        string ambiguousTag = rule.isAmbiguous ? "   [ambiguous]" : string.Empty;
                        EditorGUILayout.LabelField(new GUIContent($"    [{rule.matchKind}]  {rule.GetDisplayName()}{sinceVersion}{ambiguousTag}", rule.description), EditorStyles.miniLabel);
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void _DrawManualOnlyRules()
        {
            if (this._result.manualOnlyRules.Count == 0)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                bool showManualOnlyRules = EditorGUILayout.Foldout(this._showManualOnlyRules, $"Requires manual migration ({this._result.manualOnlyRules.Count}) — cannot be resolved safely by text analysis", true);
                if (showManualOnlyRules != this._showManualOnlyRules)
                    this._Defer(() => this._showManualOnlyRules = showManualOnlyRules);

                if (this._showManualOnlyRules)
                {
                    foreach (var rule in this._result.manualOnlyRules)
                    {
                        string sinceVersion = string.IsNullOrEmpty(rule.sinceVersion) ? string.Empty : $"   (since {rule.sinceVersion})";
                        EditorGUILayout.LabelField(new GUIContent($"    {rule.GetDisplayName()}{sinceVersion}", rule.description), EditorStyles.miniLabel);
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void _DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Rescan", EditorStyles.miniButton, GUILayout.Width(70f)))
                {
                    this._Defer(() =>
                    {
                        this._result = APIUpdater.Scan();
                        this._fileFoldouts.Clear();
                    });
                }

                if (GUILayout.Button("Select All", EditorStyles.miniButton, GUILayout.Width(80f)))
                {
                    var visibleMatches = this._GetVisibleMatches().ToArray();
                    this._Defer(() =>
                    {
                        foreach (var match in visibleMatches)
                        {
                            if (match.CanApply())
                                match.selected = true;
                        }
                    });
                }

                if (GUILayout.Button("Deselect All", EditorStyles.miniButton, GUILayout.Width(90f)))
                {
                    var visibleMatches = this._GetVisibleMatches().ToArray();
                    this._Defer(() =>
                    {
                        foreach (var match in visibleMatches)
                            match.selected = false;
                    });
                }

                GUILayout.FlexibleSpace();

                bool showReviewMatches = GUILayout.Toggle(this._showReviewMatches, new GUIContent("Show matches that need review", "Matches without a verified type context, unresolved ambiguous overloads, named arguments, and matches inside comments or string literals are hidden by default."), EditorStyles.miniButton, GUILayout.Width(240f));
                if (showReviewMatches != this._showReviewMatches)
                    this._Defer(() => this._showReviewMatches = showReviewMatches);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawMatches()
        {
            var visibleMatches = this._GetVisibleMatches().ToArray();
            if (visibleMatches.Length == 0)
            {
                EditorGUILayout.HelpBox("No matches are ready to apply. Enable [Show matches that need review] to inspect the remaining ones.", MessageType.Info);
                return;
            }

            this._scrollPosition = EditorGUILayout.BeginScrollView(this._scrollPosition);
            {
                foreach (var group in visibleMatches.GroupBy(match => match.assetPath))
                {
                    string assetPath = group.Key;
                    var matches = group.ToArray();

                    if (!this._fileFoldouts.TryGetValue(assetPath, out bool foldout))
                        foldout = true;

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            var selectableMatches = matches.Where(match => match.CanApply()).ToArray();
                            bool allSelected = selectableMatches.Length > 0 && selectableMatches.All(match => match.selected);
                            EditorGUI.BeginDisabledGroup(selectableMatches.Length == 0);
                            {
                                bool newAllSelected = EditorGUILayout.Toggle(allSelected, GUILayout.Width(16f));
                                if (newAllSelected != allSelected)
                                {
                                    foreach (var match in selectableMatches)
                                        match.selected = newAllSelected;
                                }
                            }
                            EditorGUI.EndDisabledGroup();

                            foldout = EditorGUILayout.Foldout(foldout, $"{assetPath}  ({matches.Length})", true);
                            this._fileFoldouts[assetPath] = foldout;
                        }
                        EditorGUILayout.EndHorizontal();

                        if (foldout)
                        {
                            foreach (var match in matches)
                                this._DrawMatchRow(match);
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void _DrawMatchRow(APIUpdateMatch match)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(16f);

                // 未具備可套用的新 API 前不可勾選 (歧義且尚未指定候選)
                EditorGUI.BeginDisabledGroup(!match.CanApply());
                {
                    bool selected = EditorGUILayout.Toggle(match.selected, GUILayout.Width(16f));
                    if (selected != match.selected)
                        match.selected = selected && match.CanApply();
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button(new GUIContent($"L{match.lineNumber}", "Open the script at this line."), EditorStyles.miniButton, GUILayout.Width(52f)))
                {
                    var target = match;
                    this._Defer(() => this._OpenScript(target));
                }

                string sourceTag = (match.rule.sourceType == APIUpdateRule.SourceType.Obsolete) ? "Obsolete" : "Manual";
                string reviewTag = match.needsReview ? "  [review]" : string.Empty;

                // 顯示實際套用的新 API (歧義規則決議或指定後即時反映)
                string effectiveNewApi = match.GetEffectiveNewApi();
                string newApiDisplay = string.IsNullOrEmpty(effectiveNewApi) ? match.rule.newApi : effectiveNewApi;
                string declaringPrefix = string.IsNullOrEmpty(match.rule.declaringTypeName) ? string.Empty : $"{match.rule.declaringTypeName}.";

                string tooltip = $"{match.rule.description}";
                if (!string.IsNullOrEmpty(match.rule.sinceVersion))
                    tooltip += $"\nSince: {match.rule.sinceVersion}";
                if (!string.IsNullOrEmpty(match.resolutionNote))
                    tooltip += $"\n{match.resolutionNote}";
                if (!string.IsNullOrEmpty(match.reviewReason))
                    tooltip += $"\nNeeds review: {match.reviewReason}";
                tooltip += $"\n\n{match.lineText}";

                var apiContent = new GUIContent($"{declaringPrefix}{match.rule.oldApi}  ->  {newApiDisplay}   ({sourceTag}, {match.rule.matchKind}){reviewTag}", tooltip);
                EditorGUILayout.LabelField(apiContent, GUILayout.Width(440f));

                // 未決議的歧義匹配由使用者於下拉選單指定新 API
                if (match.rule.isAmbiguous &&
                    string.IsNullOrEmpty(match.resolvedNewApi) &&
                    match.rule.candidates != null)
                {
                    var options = new string[match.rule.candidates.Count + 1];
                    options[0] = "Choose new API...";
                    for (int i = 0; i < match.rule.candidates.Count; i++)
                        options[i + 1] = match.rule.candidates[i].newApi;

                    int currentIndex = match.chosenCandidateIndex + 1;
                    int newIndex = EditorGUILayout.Popup(currentIndex, options, GUILayout.Width(150f));
                    if (newIndex != currentIndex)
                    {
                        var target = match;
                        this._Defer(() =>
                        {
                            target.chosenCandidateIndex = newIndex - 1;
                            if (!target.CanApply())
                                target.selected = false;
                        });
                    }
                }

                string lineDisplay = match.needsReview && !string.IsNullOrEmpty(match.reviewReason) ?
                    $"{match.lineText}   — {match.reviewReason}" :
                    match.lineText;
                EditorGUILayout.LabelField(new GUIContent(lineDisplay, tooltip), EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void _DrawFooter()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                int selectedCount = this._result.GetSelectedCount();
                EditorGUILayout.LabelField($"Selected: {selectedCount}", EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(selectedCount == 0);
                {
                    if (GUILayout.Button("Apply Selected", GUILayout.Width(150f), GUILayout.Height(24f)))
                    {
                        var matches = this._result.matches.ToArray();
                        this._Defer(() =>
                        {
                            int applyCount = matches.Count(match => match.selected && match.CanApply());
                            if (!EditorUtility.DisplayDialog(
                                "API Update",
                                $"Apply {applyCount} update(s) to the project scripts?\n\nThe affected script files will be modified. It is recommended to commit or back up your project (version control) first.",
                                "Apply",
                                "Cancel"))
                                return;

                            var applyResult = APIUpdater.Apply(matches);
                            this._result = APIUpdater.Scan();
                            this._fileFoldouts.Clear();

                            EditorUtility.DisplayDialog(
                                "API Update",
                                $"Applied {applyResult.appliedCount} update(s) in {applyResult.changedFileCount} file(s).\nSkipped: {applyResult.skippedCount}.",
                                "OK");
                        });
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 延遲執行操作至下一次繪製前
        /// </summary>
        /// <param name="action"></param>
        private void _Defer(Action action)
        {
            this._pendingAction = action;
            this.Repaint();
        }

        /// <summary>
        /// 取得目前顯示的匹配項
        /// </summary>
        /// <returns></returns>
        private IEnumerable<APIUpdateMatch> _GetVisibleMatches()
        {
            if (this._showReviewMatches)
                return this._result.matches;
            return this._result.matches.Where(match => !match.needsReview);
        }

        /// <summary>
        /// 於編輯器中開啟腳本並定位行號
        /// </summary>
        /// <param name="match"></param>
        private void _OpenScript(APIUpdateMatch match)
        {
            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(match.assetPath);
            if (asset != null)
                AssetDatabase.OpenAsset(asset, match.lineNumber);
            else
                Debug.LogWarning($"Cannot open script: {match.assetPath}");
        }
    }
}
