using MyBox;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace OxGFrame.CoreFrame.Editor
{
    [MovedFrom("BindCodeSetting")]
    [CreateAssetMenu(fileName = nameof(CodeBindingSettings), menuName = "OxGFrame/Create Settings/Create Code Binding Settings")]
    public class CodeBindingSettings : ScriptableObject
    {
        public enum MethodType
        {
            Manual = 1,
            Auto = 0
        }

        [Serializable]
        public struct Pluralize
        {
            public byte endRemoveCount;
            public string endPluralTxt;

            public Pluralize(byte endRemoveCount, string endPluralTxt)
            {
                this.endRemoveCount = endRemoveCount;
                this.endPluralTxt = endPluralTxt;
            }
        }

        [Serializable]
        public struct ComponentInfo
        {
            public string componentName;
            [Tooltip("Using namespace of the component. If empty, no using will be inserted (add usings manually by yourself).\n\nex: UnityEngine.UI")]
            public string usingNamespace;
            public Pluralize pluralize;

            public ComponentInfo(string componentName, Pluralize pluralize)
            {
                this.componentName = componentName;
                this.usingNamespace = string.Empty;
                this.pluralize = pluralize;
            }

            public ComponentInfo(string componentName, string usingNamespace, Pluralize pluralize)
            {
                this.componentName = componentName;
                this.usingNamespace = usingNamespace;
                this.pluralize = pluralize;
            }
        }

        public enum CaseType
        {
            CamelCase,
            PascalCase,
        }

        public enum IndicateModifier
        {
            None,
            This
        }

        [Separator("Binding Method Settings")]
        [Tooltip("*[Auto] Automatically save binding content to script.\n\n*[Manual] Manually copy the binding content from the clipboard to the script.")]
        public MethodType methodType = MethodType.Auto;

        [Separator("Attribute Settings")]
        public AttrGenericDictionary<string, string> attrReferenceRules = new AttrGenericDictionary<string, string>()
        {
            { "[hi]", "[HideInInspector]" },
            { "[sf]", "[SerializeField]"}
        };

        [Separator("Variable Settings")]
        public CaseType variableCaseType = CaseType.CamelCase;
        [Tooltip("The first element will be default")]
        public VariableGenericDictionary<string, string> variableAccessRules = new VariableGenericDictionary<string, string>()
        {
            { "protected", "_" },
            { "private", "_" },
            { "public" , "" }
        };

        [Separator("Indicate Modifier Settings")]
        public IndicateModifier indicateModifier = IndicateModifier.This;

        [Separator("Tail Binding Settings")]
        public GenericDictionary<string, ComponentInfo> tailRules = _GetDefaultTailRules();

        /// <summary>
        /// 取得預設的 Tail Rules
        /// </summary>
        /// <returns></returns>
        private static GenericDictionary<string, ComponentInfo> _GetDefaultTailRules()
        {
            return new GenericDictionary<string, ComponentInfo>()
            {
                // Other
                { "Trans", new ComponentInfo("Transform", "UnityEngine", new Pluralize(0, "es")) },
                { "RectTrans", new ComponentInfo("RectTransform", "UnityEngine", new Pluralize(0, "es")) },

                // Legacy
                { "Img", new ComponentInfo("Image", "UnityEngine.UI", new Pluralize(0, "s")) },
                { "RawImg", new ComponentInfo("RawImage", "UnityEngine.UI", new Pluralize(0, "s")) },
                { "Txt", new ComponentInfo("Text", "UnityEngine.UI", new Pluralize(0, "s")) },
                { "Btn", new ComponentInfo("Button", "UnityEngine.UI", new Pluralize(0, "s")) },
                { "Tgl", new ComponentInfo("Toggle", "UnityEngine.UI", new Pluralize(0, "s")) },
                { "Sld", new ComponentInfo("Slider", "UnityEngine.UI", new Pluralize(0, "s")) },
                { "ScrBar", new ComponentInfo("Scrollbar", "UnityEngine.UI", new Pluralize(0, "s")) },
                { "ScrView", new ComponentInfo("ScrollRect", "UnityEngine.UI", new Pluralize(0, "s")) },
                { "Drd", new ComponentInfo("Dropdown", "UnityEngine.UI", new Pluralize(0, "s")) },
                { "Field", new ComponentInfo("InputField", "UnityEngine.UI", new Pluralize(0, "s")) },

                // TMP
                { "TmpTxt", new ComponentInfo("TMP_Text", "TMPro", new Pluralize(0, "s")) },
                { "TmpDrd", new ComponentInfo("TMP_Dropdown", "TMPro", new Pluralize(0, "s")) },
                { "TmpField", new ComponentInfo("TMP_InputField", "TMPro", new Pluralize(0, "s")) },

                // Custom
                { "BtnPlus", new ComponentInfo("ButtonPlus", "OxGKit.ButtonSystem", new Pluralize(0, "es"))},
                { "NodePool", new ComponentInfo("NodePool", "OxGKit.PoolSystem", new Pluralize(0, "s"))}
            };
        }

        public string GetIndicateModifier()
        {
            switch (this.indicateModifier)
            {
                case IndicateModifier.This:
                    return "this.";
                default:
                    return string.Empty;
            }
        }

        public string GetAttrReference(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            this.attrReferenceRules.TryGetValue(key, out var result);
            if (result == null)
                return string.Empty;

            return result;
        }

        public string[] GetVariableAccessPairs(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return new string[]
                {
                    this.variableAccessRules.Keys.First(),
                    this.variableAccessRules.Values.First()
                };
            }

            this.variableAccessRules.TryGetValue(key, out var result);
            if (result == null)
            {
                return new string[]
                {
                    this.variableAccessRules.Keys.First(),
                    this.variableAccessRules.Values.First()
                };
            }

            return new string[]
            {
                key,
                result
            };
        }

        public string GetMethodAccessModifier()
        {
            return "protected";
        }

        public string GetMethodName()
        {
            return "OnAutoBind";
        }

        #region ContextMenus
        [Space(2.5f)]
        [ButtonClicker(nameof(SortTailRules), "Sort Tail Rules (A-Z)", "#00ffd1")]
        public bool sortTailRules;

        [ContextMenu("Sort Tail Rules (A-Z)", false, 0)]
        public void SortTailRules()
        {
            this.tailRules.Sort();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        [Space(2.5f)]
        [ButtonClicker(nameof(TryFindAllUsingNamespaces), "Try Find All Using Namespaces", "#ffb824")]
        public bool tryFindAllUsingNamespaces;

        [ContextMenu("Try Find All Using Namespaces", false, 1)]
        public void TryFindAllUsingNamespaces()
        {
            int autoFilled = 0;
            int pendings = 0;
            int notFounds = 0;

            // 先快照 keys, 避免迭代中修改集合
            var keys = this.tailRules.Keys.ToList();
            foreach (string key in keys)
            {
                var rule = this.tailRules[key];

                // 已填寫的不做任何變更
                if (!string.IsNullOrEmpty(rule.usingNamespace))
                    continue;

                var candidates = UsingNamespaceFinder.FindCandidates(rule.componentName);
                if (candidates.Count == 1)
                {
                    rule.usingNamespace = candidates[0].ns;
                    this.tailRules[key] = rule;
                    autoFilled++;
                    Debug.Log($"<color=#00ffd1>[{nameof(CodeBindingSettings)}] Auto filled using namespace => {key} ({rule.componentName}): {UsingNamespaceFinder.GetDisplayName(candidates[0].ns)} ({candidates[0].assemblyName})</color>");
                }
                else if (candidates.Count > 1)
                {
                    pendings++;
                    string list = string.Join(", ", candidates.Select(candidate => UsingNamespaceFinder.GetDisplayName(candidate.ns)));
                    Debug.LogWarning($"[{nameof(CodeBindingSettings)}] Multiple candidates found for {key} ({rule.componentName}): {list}. Please use the [Find] button on the rule to choose one.");
                }
                else
                {
                    notFounds++;
                    Debug.LogWarning($"[{nameof(CodeBindingSettings)}] Cannot find any type named: {rule.componentName} ({key}). Please check the component name or install the related package.");
                }
            }

            if (autoFilled > 0)
            {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"<color=#00ffd1>[{nameof(CodeBindingSettings)}] Try find completed. Auto filled: {autoFilled}, Pending manual selection: {pendings}, Not found: {notFounds}.</color>");
        }
        #endregion
    }
}