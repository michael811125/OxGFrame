using OxGFrame.CoreFrame.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.CoreFrame.Editor
{
    public static class CodeBindingAutoGenerateEditor
    {
        private class BindInfo
        {
            public string[] attrNames;
            public string variableAccessModifier;
            public string bindName;
            public string variableName;
            public string componentName;
            public string usingNamespace;
            public CodeBindingSettings.Pluralize pluralize;
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

        private static CodeBindingSettings _settings;
        private static FrameBase _script;
        private static Dictionary<string, BindInfo> _collectBindInfos = new Dictionary<string, BindInfo>();
        private static string _builder = string.Empty;
        private static readonly HashSet<string> _requiredNamespaces = new HashSet<string>();

        #region MenuItem
        [MenuItem("GameObject/OxGFrame/Auto Generate Bind Codes (Shift+B) #b", false, 0)]
        public static void Execute()
        {
            if (Selection.gameObjects.Length == 0)
                return;

            // 載入配置文件
            _settings = AssetUtility.LoadSettingsData<CodeBindingSettings>();

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
                foreach (Transform child in selected.transform)
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

            // 檢查是否要結束綁定，有檢查到【BIND_END】時，則停止繼續搜尋綁定物件
            if (Binder.CheckNodeHasStopEnd(name))
                return false;

            // 這邊檢查有【BIND_PREFIX】時，則進入判斷
            if (Binder.CheckNodeHasPrefix(name))
            {
                _CollectBindInfo(name);
            }

            // 依序綁定下一個子物件 (遞迴找到符合綁定條件)
            foreach (Transform child in go.transform)
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

            // 必須先判斷字串陣列是否符合綁定格式資格，再進行取值，避免無效命名造成 IndexOutOfRangeException
            if (heads == null ||
                heads.Length < 2 ||
                !FrameConfig.BIND_COMPONENTS.ContainsKey(heads[0]))
            {
                return;
            }

            string bindType = heads[0]; // 綁定類型(會去查找 dictComponentFinder 裡面有沒有符合的類型)
            string bindInfo = heads[1]; // 要成為取得綁定物件後的 Key

            #region Common with Binder
            // 變數存取權檢測
            string[] bindArgs = Binder.GetAccessModifierSplitNameBySeparator(bindInfo);

            // MyObj*Txt$public => ["MyObj*Txt", "public"]
            string fullBindName = bindArgs[0];
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
                MatchCollection attrMatches = Regex.Matches(fullBindName, pattern);
                if (attrMatches.Count > 0)
                {
                    // 將匹配的子字符串存入陣列
                    attrNames = attrMatches.Cast<Match>().Select(m => m.Value).ToArray();
                    // 將所有方括號替換為空字串
                    fullBindName = Regex.Replace(fullBindName, pattern, "");
                }
            }
            #endregion

            // 組件結尾檢測 => fullBindName = MyObj*Txt*Img = [MyObj, Txt, Img]
            string[] tails = Binder.GetTailSplitNameBySeparator(fullBindName);
            string variableName;
            List<string> bindNames = new List<string>();
            List<string> componentNames = new List<string>();

            if (tails != null &&
                tails.Length >= 2)
            // Component
            {
                variableName = tails[0];
                // Multi Components
                for (int i = 1; i < tails.Length; i++)
                {
                    // 合併綁定名稱 = MyObj*Txt
                    bindNames.Add($"{tails[0]}*{tails[i]}");
                    componentNames.Add(tails[i]);
                }
            }
            // GameObject
            else
            {
                variableName = tails[0];
                bindNames.Add(tails[0]);
                componentNames.Add(_DEF_COMPONENT_NAME);
            }

            for (int i = 0; i < bindNames.Count; i++)
            {
                string bindName = bindNames[i];
                string componentName = componentNames[i];
                string finalBindName = componentName.Equals(_DEF_COMPONENT_NAME) ? variableName : variableName + componentName;
                string finalVariableName = string.Empty;

                // 修正 Case
                switch (_settings.variableCaseType)
                {
                    case CodeBindingSettings.CaseType.CamelCase:
                        finalVariableName = char.ToLower(finalBindName[0]) + finalBindName.Substring(1);
                        break;
                    case CodeBindingSettings.CaseType.PascalCase:
                        finalVariableName = char.ToUpper(finalBindName[0]) + finalBindName.Substring(1);
                        break;
                }

                // 搜集綁定資訊
                if (_collectBindInfos.ContainsKey(bindName))
                {
                    _collectBindInfos[bindName].count++;
                }
                else
                    _collectBindInfos.Add(bindName, new BindInfo(attrNames, variableAccessModifier, bindName, finalVariableName, componentName, 1));
            }
        }
        #endregion

        #region Generate
        /// <summary>
        /// 生成綁定代碼
        /// </summary>
        private static void _GenerateCodes()
        {
            #region 組件規則檢查
            _requiredNamespaces.Clear();
            foreach (var bindInfo in _collectBindInfos)
            {
                foreach (var tail in _settings.tailRules)
                {
                    if (bindInfo.Value.componentName == tail.Key)
                    {
                        bindInfo.Value.componentName = tail.Value.componentName;
                        bindInfo.Value.usingNamespace = tail.Value.usingNamespace;
                        bindInfo.Value.pluralize = tail.Value.pluralize;
                        break;
                    }
                }

                // 收集本次綁定實際用到的 using namespace (規則未填寫 usingNamespace 表示由使用者自行引用)
                if (bindInfo.Value.componentName == _DEF_COMPONENT_NAME)
                    _requiredNamespaces.Add("UnityEngine");
                else if (!string.IsNullOrEmpty(bindInfo.Value.usingNamespace))
                    _requiredNamespaces.Add(bindInfo.Value.usingNamespace);
            }
            #endregion

            // Head of content ↓↓↓
            // 生成內容一律使用相對縮排 (region 與成員為第 0 級), 待寫入腳本時再依插入點縮排自適應對齊
            string space = string.Empty;
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

                // 沒找到綁定區塊，直接略過
                if (string.IsNullOrEmpty(targetCode))
                    continue;

                Debug.Log($"<color=#39ffc2>Try comparing with parent class. ScriptPath: {scriptPaths[i]}</color>");
                _builder = _RemoveDuplicateLines(_builder, targetCode);
            }

            // > 1 表示有進行父類比對，才需要調整格式
            if (scriptPaths.Length > 1)
                _builder = _AdjustMembersFormat(_builder);

            switch (_settings.methodType)
            {
                case CodeBindingSettings.MethodType.Auto:
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
            // 附上所需的 using namespaces 提示 (由使用者自行貼至腳本頂部)
            if (_requiredNamespaces.Count > 0)
            {
                string usingText = "// Please paste the following usings at the top of the script file.\n";
                foreach (string ns in _requiredNamespaces.OrderBy(ns => ns, StringComparer.Ordinal))
                    usingText += $"using {ns};\n";
                content = usingText + "\n" + content;
            }

            CodeBindingClipboardWindow.ShowWindow(content);
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

            // 偵測原始編碼是否含 BOM (寫回時保持一致)
            bool hasBom = false;
            try
            {
                using (var fs = System.IO.File.OpenRead(scriptPath))
                {
                    byte[] preamble = new byte[3];
                    if (fs.Read(preamble, 0, 3) == 3)
                        hasBom = preamble[0] == 0xEF && preamble[1] == 0xBB && preamble[2] == 0xBF;
                }
            }
            catch
            {
                /* Nothing to do */
            }

            // Read all script content
            string scriptContent = System.IO.File.ReadAllText(scriptPath);

            // Expression pattern
            string pattern = _REPLACEMENT_PATTERN;
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            Match match = regex.Match(scriptContent);

            if (match.Success)
            {
                // 量測插入點所在行的縮排，讓生成內容自適應 namespace 與巢狀層級
                string baseIndent = _MeasureBaseIndent(scriptContent, match.Groups[0].Index);

                // 將相對縮排的生成內容依插入點縮排逐行對齊
                string indentedContent = _ApplyBaseIndent(content, baseIndent);

                // Replace binding content into the script
                scriptContent = scriptContent.Remove(match.Groups[0].Index, match.Groups[0].Length);
                scriptContent = scriptContent.Insert(match.Groups[0].Index, indentedContent);

                // 插入缺少的 using namespaces
                scriptContent = _InsertUsingNamespaces(scriptContent, _requiredNamespaces);

                // To Unix style "\n"
                scriptContent = scriptContent.Replace("\r\n", "\n").Replace("\r", "\n");

                // Write file
                System.IO.File.WriteAllText(scriptPath, scriptContent, new System.Text.UTF8Encoding(hasBom));

                AssetDatabase.Refresh();
                EditorUtility.SetDirty(_script);

                Debug.Log("<color=#02ff8e>[Auto] Completed automatically binding content to script!!!</color>", _script);
            }
            else
            {
                Debug.LogWarning($"Unable to find specific replacement string in script. Script Name:{_script.name}", _script);
                Debug.LogWarning($"The pattern is ↓↓↓ (Copy the following into the script) ↓↓↓", _script);
                Debug.LogWarning($"{_REPLACEMENT_PATTERN_HEAD}\n{_REPLACEMENT_PATTERN_END}");
            }
        }

        /// <summary>
        /// 量測插入點所在行的縮排字串 (取行首至插入點之間的空白)
        /// </summary>
        /// <param name="scriptContent"></param>
        /// <param name="insertIndex"></param>
        /// <returns></returns>
        private static string _MeasureBaseIndent(string scriptContent, int insertIndex)
        {
            int lineStart = scriptContent.LastIndexOf('\n', Math.Max(0, insertIndex - 1)) + 1;
            string leading = scriptContent.Substring(lineStart, insertIndex - lineStart);

            // 僅接受純空白的前綴，避免同行前方有其他代碼造成錯誤縮排
            foreach (char c in leading)
            {
                if (c != ' ' && c != '\t')
                    return string.Empty;
            }

            return leading;
        }

        /// <summary>
        /// 將相對縮排的生成內容逐行加上基準縮排 (首行由原插入點自帶縮排，空行不做處理)
        /// </summary>
        /// <param name="content"></param>
        /// <param name="baseIndent"></param>
        /// <returns></returns>
        private static string _ApplyBaseIndent(string content, string baseIndent)
        {
            if (string.IsNullOrEmpty(baseIndent))
                return content;

            string[] lines = content.Split('\n');
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Length > 0)
                    lines[i] = baseIndent + lines[i];
            }

            return string.Join("\n", lines);
        }

        /// <summary>
        /// 將缺少的 using namespaces 插入至腳本頂部的 using 區塊 (已存在的不重複插入)
        /// </summary>
        /// <param name="scriptContent"></param>
        /// <param name="requiredNamespaces"></param>
        /// <returns></returns>
        private static string _InsertUsingNamespaces(string scriptContent, HashSet<string> requiredNamespaces)
        {
            if (requiredNamespaces == null ||
                requiredNamespaces.Count == 0)
                return scriptContent;

            // 收集腳本中已存在的 using (寬鬆掃描全檔，排除 alias 與 static 形式)
            HashSet<string> existings = new HashSet<string>();
            foreach (Match m in Regex.Matches(scriptContent, @"^[ \t]*using\s+(?!static\s)([\w.]+)\s*;", RegexOptions.Multiline))
                existings.Add(m.Groups[1].Value);

            // 過濾出缺少的 using 並依字母排序 (讓插入結果穩定)
            List<string> missings = requiredNamespaces.Where(ns => !existings.Contains(ns)).OrderBy(ns => ns, StringComparer.Ordinal).ToList();
            if (missings.Count == 0)
                return scriptContent;

            // 尋找檔案頂部最後一個 using 行的結尾作為插入點 (略過 namespace 或型別宣告之後的 using)
            int insertIndex = 0;
            foreach (Match m in Regex.Matches(scriptContent, @"^[ \t]*using\s+[^;\r\n]+;[ \t]*\r?\n", RegexOptions.Multiline))
            {
                string head = scriptContent.Substring(0, m.Index);
                if (Regex.IsMatch(head, @"^\s*(namespace|class|struct|interface|enum)\s", RegexOptions.Multiline))
                    break;
                insertIndex = m.Index + m.Length;
            }

            string insertText = string.Empty;
            foreach (string ns in missings)
                insertText += $"using {ns};\n";

            return scriptContent.Insert(insertIndex, insertText);
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
                Debug.LogWarning("Cannot find script (FrameBase => UIBase, SRBase, CPBase). Please drag the script onto the object!!!");
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
        /// 比對兩段程式碼，移除來源中與目標重複的行
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
        /// 移除目標時，並且移除不必要斷行
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
                    // 檢查最後一個元素後是否有兩個換行，如果沒有需要再 + 一個
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
