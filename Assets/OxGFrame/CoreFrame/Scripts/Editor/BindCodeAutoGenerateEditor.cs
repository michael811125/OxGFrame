using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.CoreFrame.Editor
{
    public static class BindCodeAutoGenerateEditor
    {
        private class BindInfo
        {
            public string bindName;
            public string variableName;
            public string componentName;
            public BindCodeSetting.Pluralize pluralize;
            public int count;

            public BindInfo(string bindeName, string variableName, string componentName, int count)
            {
                this.bindName = bindeName;
                this.variableName = variableName;
                this.componentName = componentName;
                this.count = count;
            }
        }

        private const string _DEF_COMPONENT_NAME = "GameObject";

        private const string _REPLACEMENT_PATTERN = @"#region Binding Components(.*?)#endregion";
        private const string _REPLACEMENT_PATTERN_HEAD = "#region Binding Components";
        private const string _REPLACEMENT_PATTERN_END = "#endregion";

        private static BindCodeSetting _settings;
        private static FrameBase _script;
        private static Dictionary<string, BindInfo> _collectBindInfos = new Dictionary<string, BindInfo>();
        private static string _builder = string.Empty;

        #region MenuItem
        [MenuItem("GameObject/OxGFrame/Auto Generate Bind Codes (Shift+B) #b", false, 0)]
        public static void Execute()
        {
            if (Selection.gameObjects.Length == 0) return;

            // 載入配置檔
            _settings = EditorTool.LoadSettingData<BindCodeSetting>();

            // 檢查選擇物件是否包含子節點
            if (_CheckHasChildren()) return;

            // 開始搜集綁定資訊
            _StartCollect();

            // 生成代碼
            _GenerateCodes();
        }
        #endregion

        #region Check
        private static bool _CheckHasChildren()
        {
            foreach (var go in Selection.gameObjects)
            {
                if (_HasChild(go, Selection.gameObjects)) return true;
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
        private static void _StartCollect()
        {
            _script = null;
            _builder = string.Empty;
            _collectBindInfos.Clear();
            foreach (var go in Selection.gameObjects) _Collect(go);
        }

        private static bool _Collect(GameObject go)
        {
            // Get script component
            if (_script == null)
                _script = go.GetComponent<FrameBase>();

            string name = go.name;

            // 檢查是否要結束綁定, 有檢查到【BIND_END】時, 則停止繼續搜尋綁定物件
            if (Binder.CheckNodeHasStopEnd(name)) return false;

            // 這邊檢查有【BIND_PREFIX】時, 則進入判斷
            if (Binder.CheckNodeHasPrefix(name))
            {
                _CollectBindInfo(name);
            }

            // 依序綁定下一個子物件 (遞迴找到符合綁定條件)
            foreach (Transform child in go.GetComponentInChildren<Transform>())
            {
                if (!_Collect(child.gameObject)) return false;
            }

            return true;
        }

        private static void _CollectBindInfo(string name)
        {
            // 綁定開頭檢測
            string[] heads = Binder.GetHeadSplitNameBySeparator(name);

            string bindType = heads[0]; // 綁定類型(會去查找 dictComponentFinder 裡面有沒有符合的類型)
            string bindName = heads[1]; // 要成為取得綁定物件後的Key

            // 再去判斷取得後的字串陣列是否綁定格式資格
            if (heads == null || heads.Length < 2 || !FrameConfig.BIND_COMPONENTS.ContainsKey(bindType))
            {
                return;
            }

            // 組件結尾檢測
            string[] tails = Binder.GetTailSplitNameBySeparator(bindName);
            string variableName = bindName.Replace(" ", string.Empty);
            string componentName = _DEF_COMPONENT_NAME;

            if (tails != null && tails.Length >= 2)
            {
                variableName = tails[0] + tails[1];
                variableName.Replace(" ", string.Empty);
                componentName = tails[1];
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
            else _collectBindInfos.Add(bindName, new BindInfo(bindName, variableName, componentName, 1));
        }
        #endregion

        #region Generate
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
                        _builder += space + $"{_settings.variableAccessModifier} ";
                        _builder += $"{bindInfo.Value.componentName}[] ";
                        _builder += $"{_settings.variablePrefix}{bindInfo.Value.variableName}s;\n";
                    }
                    else
                    {
                        _builder += space + $"{_settings.variableAccessModifier} ";
                        _builder += $"{bindInfo.Value.componentName}[] ";
                        int varNameLength = bindInfo.Value.variableName.Length;
                        int endRemoveCount = (bindInfo.Value.pluralize.endRemoveCount > varNameLength) ? 0 : bindInfo.Value.pluralize.endRemoveCount;
                        string varName = bindInfo.Value.variableName.Substring(0, varNameLength - endRemoveCount);
                        string endPluralTxt = bindInfo.Value.pluralize.endPluralTxt;
                        string newVarName = $"{varName}{endPluralTxt}";
                        _builder += $"{_settings.variablePrefix}{newVarName};\n";
                    }
                }
                // Single
                else
                {
                    _builder += space + $"{_settings.variableAccessModifier} ";
                    _builder += $"{bindInfo.Value.componentName} ";
                    _builder += $"{_settings.variablePrefix}{bindInfo.Value.variableName};\n";
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
                        _builder += space + $"    {_settings.GetIndicateModifier()}{_settings.variablePrefix}{bindInfo.Value.variableName}s = ";
                        _builder += $"{_settings.GetIndicateModifier()}collector.GetNodes(\"{bindInfo.Value.bindName}\");\n";
                    }
                    else
                    {
                        _builder += $"    ";
                        int varNameLength = bindInfo.Value.variableName.Length;
                        int endRemoveCount = (bindInfo.Value.pluralize.endRemoveCount > varNameLength) ? 0 : bindInfo.Value.pluralize.endRemoveCount;
                        string varName = bindInfo.Value.variableName.Substring(0, varNameLength - endRemoveCount);
                        string endPluralTxt = bindInfo.Value.pluralize.endPluralTxt;
                        string newVarName = $"{varName}{endPluralTxt}";
                        _builder += space + $"{_settings.GetIndicateModifier()}{_settings.variablePrefix}{newVarName} = ";
                        _builder += $"{_settings.GetIndicateModifier()}collector.GetNodeComponents<{bindInfo.Value.componentName}>(\"{bindInfo.Value.bindName}\");\n";
                    }
                }
                // Single
                else
                {
                    // For GameObject
                    if (bindInfo.Value.componentName == _DEF_COMPONENT_NAME)
                    {
                        _builder += space + $"    {_settings.GetIndicateModifier()}{_settings.variablePrefix}{bindInfo.Value.variableName} = ";
                        _builder += $"{_settings.GetIndicateModifier()}collector.GetNode(\"{bindInfo.Value.bindName}\");\n";
                    }
                    else
                    {
                        _builder += space + $"    {_settings.GetIndicateModifier()}{_settings.variablePrefix}{bindInfo.Value.variableName} = ";
                        _builder += $"{_settings.GetIndicateModifier()}collector.GetNodeComponent<{bindInfo.Value.componentName}>(\"{bindInfo.Value.bindName}\");\n";
                    }
                }
            }
            _builder += space + "}";
            #endregion

            _builder += "\n";
            _builder += space + _REPLACEMENT_PATTERN_END;
            // End of content ↑↑↑

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

        private static void _ShowClipboard(string content)
        {
            BindCodeClipboardWindow.ShowWindow(content);
            Debug.Log("<color=#02ff8e>[Manual] Copy binding content to script!!!</color>");
        }

        private static void _InsertCodes(string content)
        {
            if (_script == null)
            {
                Debug.Log("<color=#ff026e>Cannot find script (FrameBase => UIBase, SRBase, CPBase). Please drag the script onto the object!!!</color>");
                return;
            }

            string scriptPath = _GetScriptPath(_script);

            // Read all script content
            string scriptContent = System.IO.File.ReadAllText(scriptPath);

            // Expression pattern
            string pattern = _REPLACEMENT_PATTERN;
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Singleline);
            System.Text.RegularExpressions.Match match = regex.Match(scriptContent);

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

        private static string _GetScriptPath(MonoBehaviour script)
        {
            MonoScript monoScript = MonoScript.FromMonoBehaviour(script);
            string scriptRelativePath = AssetDatabase.GetAssetPath(monoScript);
            string scriptPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", scriptRelativePath));
            return scriptPath;
        }
        #endregion
    }
}