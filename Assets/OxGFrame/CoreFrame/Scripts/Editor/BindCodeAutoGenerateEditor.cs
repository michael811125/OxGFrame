using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.CoreFrame.Editor
{
    public static class BindCodeAutoGenerateEditor
    {
        private class BindInfo
        {
            public string[] attrNames;
            public string variableAccessModifier;
            public string bindName;
            public string variableName;
            public string componentName;
            public BindCodeSetting.Pluralize pluralize;
            public int count;

            public BindInfo(string[] attrNames, string variableAccessModifier, string bindName, string variableName, string componentName, int count)
            {
                this.attrNames = attrNames;
                this.variableAccessModifier = variableAccessModifier;
                this.bindName = bindName;
                this.variableName = variableName;
                this.componentName = componentName;
                this.count = count;
            }
        }

        private const string _DEF_COMPONENT_NAME = "GameObject";

        /// <summary>
        /// 成員變數定義的表達式
        /// </summary>
        private const string _SEARCH_MEMBER_PATTERN = @"(?:\[\w+\]\s*)*(?:public|protected|private|internal)?\s*(?:static\s*)?\s*\w+(?:\[\])?\s+\w+\s*;";

        /// <summary>
        /// 方法內變數賦值的表達式
        /// </summary>
        private const string _SEARCH_ASSIGNMENT_PATTERN = @"(?:this\.)?_?\w+\s*=\s*(?:this\.)?collector\.GetNode(?:s|Component(?:s)?)?(?:<\w+>)?\(""[^""]+?""\);";

        /// <summary>
        /// 綁定區塊的表達式
        /// </summary>
        private const string _REPLACEMENT_PATTERN = @"#region Binding Components(.*?)#endregion";

        /// <summary>
        /// 綁定區塊 HEAD 的表達式
        /// </summary>
        private const string _REPLACEMENT_PATTERN_HEAD = "#region Binding Components";

        /// <summary>
        /// 綁定區塊 END 的表達式
        /// </summary>
        private const string _REPLACEMENT_PATTERN_END = "#endregion";

        private static BindCodeSetting _settings;
        private static FrameBase _script;
        private static Dictionary<string, BindInfo> _collectBindInfos = new Dictionary<string, BindInfo>();
        private static string _builder = string.Empty;

        #region MenuItem
        [MenuItem("GameObject/OxGFrame/Auto Generate Bind Codes (Shift+B) #b", false, 0)]
        public static void Execute()
        {
            if (Selection.gameObjects.Length == 0)
                return;

            // 載入配置檔
            _settings = EditorTool.LoadSettingData<BindCodeSetting>();

            // 檢查選擇物件是否包含子節點
            if (_CheckHasChildren())
                return;

            // 開始搜集綁定信息
            _StartCollect();

            // 生成代碼
            _GenerateCodes();
        }
        #endregion

        #region Check
        /// <summary>
        /// 檢查選擇物件是否包含子節點
        /// </summary>
        /// <returns></returns>
        private static bool _CheckHasChildren()
        {
            foreach (var go in Selection.gameObjects)
            {
                if (_HasChild(go, Selection.gameObjects))
                    return true;
            }

            return false;
        }

        private static bool _HasChild(GameObject selected, GameObject[] selections)
        {
            if (selected.transform.childCount > 0)
            {
                foreach (Transform child in selected.GetComponentInChildren<Transform>())
                {
                    foreach (var select in selections)
                    {
                        if (select.GetInstanceID() == child.gameObject.GetInstanceID())
                        {
                            Debug.Log($"<color=#ff2486>Including Child Node => Parent: <color=#ffb824>{selected.name}</color>, Child: <color=#ffec24>{child.name}</color></color>");
                            return true;
                        }
                    }

                    if (_HasChild(child.gameObject, selections)) return true;
                }
            }

            return false;
        }
        #endregion

        #region Collect
        /// <summary>
        /// 開始搜集綁定信息
        /// </summary>
        private static void _StartCollect()
        {
            _script = null;
            _builder = string.Empty;
            _collectBindInfos.Clear();
            foreach (var go in Selection.gameObjects)
                _CheckRuleAndCollect(go);
        }

        /// <summary>
        /// 檢查規則並且進行綁定信息的搜集
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        private static bool _CheckRuleAndCollect(GameObject go)
        {
            // Get script component
            if (_script == null)
                _script = go.GetComponent<FrameBase>();

            string name = go.name;

            // 檢查是否要結束綁定, 有檢查到【BIND_END】時, 則停止繼續搜尋綁定物件
            if (Binder.CheckNodeHasStopEnd(name))
                return false;

            // 這邊檢查有【BIND_PREFIX】時, 則進入判斷
            if (Binder.CheckNodeHasPrefix(name))
            {
                _CollectBindInfo(name);
            }

            // 依序綁定下一個子物件 (遞迴找到符合綁定條件)
            foreach (Transform child in go.GetComponentInChildren<Transform>())
            {
                if (!_CheckRuleAndCollect(child.gameObject))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 搜集物件綁定訊息
        /// </summary>
        /// <param name="name"></param>
        private static void _CollectBindInfo(string name)
        {
            // 綁定開頭檢測
            string[] heads = Binder.GetHeadSplitNameBySeparator(name);

            string bindType = heads[0]; // 綁定類型(會去查找 dictComponentFinder 裡面有沒有符合的類型)
            string bindInfo = heads[1]; // 要成為取得綁定物件後的 Key

            // 再去判斷取得後的字串陣列是否綁定格式資格
            if (heads == null ||
                heads.Length < 2 ||
                !FrameConfig.BIND_COMPONENTS.ContainsKey(bindType))
            {
                return;
            }

            #region Common with Binder
            // 變數存取權檢測
            string[] bindArgs = Binder.GetAccessModifierSplitNameBySeparator(bindInfo);

            // MyObj*Txt$public => ["MyObj*Txt", "public"]
            string bindName = bindArgs[0];
            string variableAccessModifier = (bindArgs.Length > 1) ? bindArgs[1] : null;

            // 匹配 Attr []
            string pattern = @"\[(.*?)\]";
            string[] attrNames = new string[] { };
            if (!string.IsNullOrEmpty(variableAccessModifier))
            {
                MatchCollection attrMatches = Regex.Matches(variableAccessModifier, pattern);
                if (attrMatches.Count > 0)
                {
                    // 將匹配的子字符串存入陣列
                    attrNames = attrMatches.Cast<Match>().Select(m => m.Value).ToArray();
                    // 將所有方括號替換為空字串
                    variableAccessModifier = Regex.Replace(variableAccessModifier, pattern, "");
                }
            }
            else
            {
                MatchCollection attrMatches = Regex.Matches(bindName, pattern);
                if (attrMatches.Count > 0)
                {
                    // 將匹配的子字符串存入陣列
                    attrNames = attrMatches.Cast<Match>().Select(m => m.Value).ToArray();
                    // 將所有方括號替換為空字串
                    bindName = Regex.Replace(bindName, pattern, "");
                }
            }
            #endregion

            // 組件結尾檢測
            string[] tails = Binder.GetTailSplitNameBySeparator(bindName);
            string variableName, componentName;

            if (tails != null &&
                tails.Length >= 2)
            // Component
            {
                variableName = tails[0] + tails[1];
                componentName = tails[1];
            }
            // GameObject
            else
            {
                variableName = bindName;
                componentName = _DEF_COMPONENT_NAME;
            }

            // 修正 Case
            switch (_settings.variableCaseType)
            {
                case BindCodeSetting.CaseType.CamelCase:
                    variableName = char.ToLower(variableName[0]) + variableName.Substring(1);
                    break;
                case BindCodeSetting.CaseType.PascalCase:
                    variableName = char.ToUpper(variableName[0]) + variableName.Substring(1);
                    break;
            }

            // 搜集綁定資訊
            if (_collectBindInfos.ContainsKey(bindName))
            {
                _collectBindInfos[bindName].count++;
            }
            else _collectBindInfos.Add(bindName, new BindInfo(attrNames, variableAccessModifier, bindName, variableName, componentName, 1));
        }
        #endregion

        #region Generate
        /// <summary>
        /// 生成綁定代碼
        /// </summary>
        private static void _GenerateCodes()
        {
            #region 組件規則檢查
            foreach (var bindInfo in _collectBindInfos)
            {
                foreach (var tail in _settings.tailRules)
                {
                    if (bindInfo.Value.componentName == tail.Key)
                    {
                        bindInfo.Value.componentName = tail.Value.componentName;
                        bindInfo.Value.pluralize = tail.Value.pluralize;
                        break;
                    }
                }
            }
            #endregion

            // Head of content ↓↓↓
            string space = _settings.methodType == BindCodeSetting.MethodType.Auto ? "    " : "";
            _builder += _REPLACEMENT_PATTERN_HEAD + "\n";

            #region 變數宣告生成
            foreach (var bindInfo in _collectBindInfos)
            {
                // Array
                if (bindInfo.Value.count > 1)
                {
                    // For GameObjects
                    if (bindInfo.Value.componentName == _DEF_COMPONENT_NAME)
                    {
                        // Attrs
                        for (int i = 0; i < bindInfo.Value.attrNames.Length; i++)
                            _builder += (i == 0 ? space : "") + $"{_settings.GetAttrReference(bindInfo.Value.attrNames[i])}";
                        // Access modifier
                        _builder += (bindInfo.Value.attrNames.Length > 0 ? " " : space) + $"{_settings.GetVariableAccessPairs(bindInfo.Value.variableAccessModifier)[0]} ";
                        // Type
                        _builder += $"{bindInfo.Value.componentName}[] ";
                        // Variable name
                        _builder += $"{_settings.GetVariableAccessPairs(bindInfo.Value.variableAccessModifier)[1]}{bindInfo.Value.variableName}s;\n";
                    }
                    else
                    {
                        // Attrs
                        for (int i = 0; i < bindInfo.Value.attrNames.Length; i++)
                            _builder += (i == 0 ? space : "") + $"{_settings.GetAttrReference(bindInfo.Value.attrNames[i])}";
                        // Access modifier
                        _builder += (bindInfo.Value.attrNames.Length > 0 ? " " : space) + $"{_settings.GetVariableAccessPairs(bindInfo.Value.variableAccessModifier)[0]} ";
                        // Type
                        _builder += $"{bindInfo.Value.componentName}[] ";
                        // Variable name
                        int varNameLength = bindInfo.Value.variableName.Length;
                        int endRemoveCount = (bindInfo.Value.pluralize.endRemoveCount > varNameLength) ? 0 : bindInfo.Value.pluralize.endRemoveCount;
                        string varName = bindInfo.Value.variableName.Substring(0, varNameLength - endRemoveCount);
                        string endPluralTxt = bindInfo.Value.pluralize.endPluralTxt;
                        string newVarName = $"{varName}{endPluralTxt}";
                        _builder += $"{_settings.GetVariableAccessPairs(bindInfo.Value.variableAccessModifier)[1]}{newVarName};\n";
                    }
                }
                // Single
                else
                {
                    // Attrs
                    for (int i = 0; i < bindInfo.Value.attrNames.Length; i++)
                        _builder += (i == 0 ? space : "") + $"{_settings.GetAttrReference(bindInfo.Value.attrNames[i])}";
                    // Access modifier
                    _builder += (bindInfo.Value.attrNames.Length > 0 ? " " : space) + $"{_settings.GetVariableAccessPairs(bindInfo.Value.variableAccessModifier)[0]} ";
                    // Type
                    _builder += $"{bindInfo.Value.componentName} ";
                    // Variable name
                    _builder += $"{_settings.GetVariableAccessPairs(bindInfo.Value.variableAccessModifier)[1]}{bindInfo.Value.variableName};\n";
                }
            }
            #endregion

            #region 方法定義生成
            _builder += "\n";
            _builder += space + "/// <summary>\n";
            _builder += space + "/// Auto Binding Section\n";
            _builder += space + "/// </summary>\n";
            _builder += space + $"{_settings.GetMethodAccessModifier()} override void {_settings.GetMethodName()}()\n";
            _builder += space + "{";
            _builder += "\n";
            _builder += space + $"    base.{_settings.GetMethodName()}();\n";

            foreach (var bindInfo in _collectBindInfos)
            {
                // Array
                if (bindInfo.Value.count > 1)
                {
                    // For GameObjects
                    if (bindInfo.Value.componentName == _DEF_COMPONENT_NAME)
                    {
                        // Variable
                        _builder += space + $"    {_settings.GetIndicateModifier()}{_settings.GetVariableAccessPairs(bindInfo.Value.variableAccessModifier)[1]}{bindInfo.Value.variableName}s = ";
                        // Assignment
                        _builder += $"{_settings.GetIndicateModifier()}collector.GetNodes(\"{bindInfo.Value.bindName}\");\n";
                    }
                    else
                    {
                        // Variable
                        _builder += $"    ";
                        int varNameLength = bindInfo.Value.variableName.Length;
                        int endRemoveCount = (bindInfo.Value.pluralize.endRemoveCount > varNameLength) ? 0 : bindInfo.Value.pluralize.endRemoveCount;
                        string varName = bindInfo.Value.variableName.Substring(0, varNameLength - endRemoveCount);
                        string endPluralTxt = bindInfo.Value.pluralize.endPluralTxt;
                        string newVarName = $"{varName}{endPluralTxt}";
                        _builder += space + $"{_settings.GetIndicateModifier()}{_settings.GetVariableAccessPairs(bindInfo.Value.variableAccessModifier)[1]}{newVarName} = ";
                        // Assignment
                        _builder += $"{_settings.GetIndicateModifier()}collector.GetNodeComponents<{bindInfo.Value.componentName}>(\"{bindInfo.Value.bindName}\");\n";
                    }
                }
                // Single
                else
                {
                    // For GameObject
                    if (bindInfo.Value.componentName == _DEF_COMPONENT_NAME)
                    {
                        // Variable
                        _builder += space + $"    {_settings.GetIndicateModifier()}{_settings.GetVariableAccessPairs(bindInfo.Value.variableAccessModifier)[1]}{bindInfo.Value.variableName} = ";
                        // Assignment
                        _builder += $"{_settings.GetIndicateModifier()}collector.GetNode(\"{bindInfo.Value.bindName}\");\n";
                    }
                    else
                    {
                        // Variable
                        _builder += space + $"    {_settings.GetIndicateModifier()}{_settings.GetVariableAccessPairs(bindInfo.Value.variableAccessModifier)[1]}{bindInfo.Value.variableName} = ";
                        // Assignment
                        _builder += $"{_settings.GetIndicateModifier()}collector.GetNodeComponent<{bindInfo.Value.componentName}>(\"{bindInfo.Value.bindName}\");\n";
                    }
                }
            }
            _builder += space + "}";
            #endregion

            _builder += "\n";
            _builder += space + _REPLACEMENT_PATTERN_END;
            // End of content ↑↑↑

            // Compare with parent class
            var scriptPaths = GetInheritedScriptPaths(_script, false);
            for (int i = 0; i < scriptPaths.Length; i++)
            {
                // 取得目標腳本的綁定區塊
                string targetCode = _GetBindingSectionText(scriptPaths[i]);

                // 沒找到綁定區塊, 直接略過
                if (string.IsNullOrEmpty(targetCode))
                    continue;

                Debug.Log($"<color=#39ffc2>Try comparing with parent class. ScriptPath: {scriptPaths[i]}</color>");
                _builder = _RemoveDuplicateLines(_builder, targetCode);
            }

            // > 1 表示有進行父類比對, 才需要調整格式
            if (scriptPaths.Length > 1)
                _builder = _AdjustMembersFormat(_builder);

            switch (_settings.methodType)
            {
                case BindCodeSetting.MethodType.Auto:
                    // Save to script
                    _InsertCodes(_builder);
                    break;
                default:
                    // Show on clipboard
                    _ShowClipboard(_builder);
                    break;
            }
        }

        /// <summary>
        /// 顯示剪貼簿
        /// </summary>
        /// <param name="content"></param>
        private static void _ShowClipboard(string content)
        {
            BindCodeClipboardWindow.ShowWindow(content);
            Debug.Log("<color=#02ff8e>[Manual] Copy binding content to script!!!</color>");
        }

        /// <summary>
        /// 儲存綁定區塊至腳本中
        /// </summary>
        /// <param name="content"></param>
        private static void _InsertCodes(string content)
        {
            // Get script path
            string scriptPath = _GetScriptPath(_script);

            if (string.IsNullOrEmpty(scriptPath))
                return;

            // Read all script content
            string scriptContent = System.IO.File.ReadAllText(scriptPath);

            // Expression pattern
            string pattern = _REPLACEMENT_PATTERN;
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            Match match = regex.Match(scriptContent);

            if (match.Success)
            {
                // Replace binding content into the script
                scriptContent = scriptContent.Remove(match.Groups[0].Index, match.Groups[0].Length);
                scriptContent = scriptContent.Insert(match.Groups[0].Index, content);

                // To Unix style "\n"
                scriptContent = scriptContent.Replace("\r\n", "\n").Replace("\r", "\n");

                // Write file
                System.IO.File.WriteAllText(scriptPath, scriptContent);

                AssetDatabase.Refresh();
                EditorUtility.SetDirty(_script);

                Debug.Log("<color=#02ff8e>[Auto] Completed automatically binding content to script!!!</color>", _script);
            }
            else
            {
                Debug.Log($"<color=#ff026e>Unable to find specific replacement string in script. <color=#ffbc02>Script Name:{_script.name}</color></color>", _script);
                Debug.Log($"<color=#ff026e>The pattern is ↓↓↓ (Copy the following into the script) ↓↓↓</color>", _script);
                Debug.Log($"<color=#ffbc02>{_REPLACEMENT_PATTERN_HEAD}\n{_REPLACEMENT_PATTERN_END}</color>");
            }
        }

        /// <summary>
        /// 取得腳本的路徑
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        private static string _GetScriptPath(MonoBehaviour script)
        {
            if (script == null)
            {
                Debug.Log("<color=#ff026e>Cannot find script (FrameBase => UIBase, SRBase, CPBase). Please drag the script onto the object!!!</color>");
                return null;
            }

            MonoScript monoScript = MonoScript.FromMonoBehaviour(script);
            string scriptRelativePath = AssetDatabase.GetAssetPath(monoScript);
            string scriptPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", scriptRelativePath));
            return scriptPath;
        }
        #endregion

        /// <summary>
        /// 取得腳本繼承基類的路徑
        /// </summary>
        /// <param name="script">腳本自身</param>
        /// <param name="includeSelf">是否包含腳本自身</param>
        /// <returns></returns>
        public static string[] GetInheritedScriptPaths(MonoBehaviour script, bool includeSelf)
        {
            List<string> scriptPaths = new List<string>();

            if (script == null)
                return scriptPaths.ToArray();

            Type currentType = script.GetType();
            while (currentType != null && currentType != typeof(MonoBehaviour))
            {
                // 跳過抽象類型
                if (!currentType.IsAbstract)
                {
                    // 嘗試取得 MonoScript 路徑
                    MonoScript monoScript = _GetMonoScriptFromType(currentType);

                    if (monoScript != null)
                    {
                        string scriptRelativePath = AssetDatabase.GetAssetPath(monoScript);
                        if (!string.IsNullOrEmpty(scriptRelativePath))
                        {
                            string scriptPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", scriptRelativePath));
                            scriptPaths.Add(scriptPath);
                        }
                    }
                }

                currentType = currentType.BaseType;
            }

            // 讓順序從基類到派生類
            // Reverse will be -> 0 = parent_a, 1 = parent_b, 2 = self
            scriptPaths.Reverse();

            // 移除最後一個元素 (就是腳本自身)
            if (!includeSelf)
                scriptPaths.RemoveAt(scriptPaths.Count - 1);

            return scriptPaths.ToArray();
        }

        /// <summary>
        /// 獲取 MonoScript
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static MonoScript _GetMonoScriptFromType(Type type)
        {
            // 確保傳入的類型是 MonoBehaviour 的子類型
            if (!typeof(MonoBehaviour).IsAssignableFrom(type))
                return null;

            // 嘗試通過反射訪問類型並獲取 MonoScript
            MonoBehaviour dummyInstance = new GameObject("Dummy").AddComponent(type) as MonoBehaviour;
            if (dummyInstance != null)
            {
                MonoScript monoScript = MonoScript.FromMonoBehaviour(dummyInstance);
                // 清理臨時對象
                UnityEngine.Object.DestroyImmediate(dummyInstance.gameObject);
                return monoScript;
            }

            return null;
        }

        /// <summary>
        /// 取得腳本中綁定區塊
        /// </summary>
        /// <param name="scriptPath"></param>
        /// <returns></returns>
        private static string _GetBindingSectionText(string scriptPath)
        {
            // 假設有一個方法來解析每個類的 Binding Components 部分
            string scriptContent = System.IO.File.ReadAllText(scriptPath);

            var match = Regex.Match(scriptContent, @"#region Binding Components(.*?)#endregion", RegexOptions.Singleline);
            if (match.Success)
            {
                string bindingSection = match.Groups[1].Value;
                return bindingSection;
            }

            return string.Empty;
        }

        /// <summary>
        /// 比對兩段程式碼, 移除來源中與目標重複的行
        /// </summary>
        /// <param name="sourceSection"></param>
        /// <param name="targetSection"></param>
        /// <returns></returns>
        private static string _RemoveDuplicateLines(string sourceSection, string targetSection)
        {
            // 成員變數定義的表達式
            Regex memberRegex = new Regex(_SEARCH_MEMBER_PATTERN);

            // 方法內變數賦值的表達式
            Regex assignmentRegex = new Regex(_SEARCH_ASSIGNMENT_PATTERN);

            // 儲存目標中的變數名稱
            HashSet<string> targetMembers = new HashSet<string>();
            HashSet<string> targetAssignments = new HashSet<string>();

            // 提取目標內容的變數名稱
            foreach (Match match in memberRegex.Matches(targetSection))
            {
                // 儲存完整變數定義行
                targetMembers.Add(match.Value.Trim());
            }
            foreach (Match match in assignmentRegex.Matches(targetSection))
            {
                // 儲存完整賦值行
                targetAssignments.Add(match.Value.Trim());
            }

            // 移除成員變數
            foreach (var v in targetMembers)
            {
                sourceSection = _RemoveLineContaining(sourceSection, v, false);
                Debug.Log($"<color=#ff7454>Removed the intersecting part of variables: <color=#ffb854>{v}</color></color>");
            }

            // 移除賦值
            foreach (var v in targetAssignments)
            {
                sourceSection = _RemoveLineContaining(sourceSection, v, false);
                Debug.Log($"<color=#ff7454>Removed the intersecting part of assignments: <color=#ffb854>{v}</color></color>");
            }

            return sourceSection;
        }

        /// <summary>
        /// 移除目標時, 並且移除不必要斷行
        /// </summary>
        /// <param name="input"></param>
        /// <param name="target"></param>
        /// <param name="preserveNewline"></param>
        /// <returns></returns>
        private static string _RemoveLineContaining(string input, string target, bool preserveNewline)
        {
            Regex regex = new Regex($@"^\s*{Regex.Escape(target)}\s*\r?\n", RegexOptions.Multiline);
            return regex.Replace(input, preserveNewline ? "\n" : "");
        }

        /// <summary>
        /// 調整與父類比對處理後的成員變數格式
        /// </summary>
        /// <param name="sourceSection"></param>
        /// <returns></returns>
        private static string _AdjustMembersFormat(string sourceSection)
        {
            // 成員變數定義的表達式
            Regex memberRegex = new Regex(_SEARCH_MEMBER_PATTERN);

            // 儲存來源中的變數名稱
            HashSet<string> sourceMembers = new HashSet<string>();

            // 提取來源內容的變數名稱
            foreach (Match match in memberRegex.Matches(sourceSection))
            {
                // 儲存完整變數定義行
                sourceMembers.Add(match.Value.Trim());
            }

            // 找到最後一個成員變數進行換行符添加
            int count = 0;
            foreach (var v in sourceMembers)
            {
                if (count == sourceMembers.Count - 1)
                {
                    // 檢查最後一個元素後是否有兩個換行, 如果沒有需要再 + 一個
                    if (sourceSection.IndexOf(v + "\n\n") == -1)
                    {
                        sourceSection = sourceSection.Replace(v, v + "\n");
                    }
                }
                count++;
            }

            return sourceSection;
        }
    }
}