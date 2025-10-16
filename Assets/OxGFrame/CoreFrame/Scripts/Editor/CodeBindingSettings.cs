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
            public Pluralize pluralize;

            public ComponentInfo(string componentName, Pluralize pluralize)
            {
                this.componentName = componentName;
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
        public GenericDictionary<string, ComponentInfo> tailRules = new GenericDictionary<string, ComponentInfo>()
        {
            // Other
            { "Trans", new ComponentInfo("Transform", new Pluralize(0, "es")) },
            { "RectTrans", new ComponentInfo("RectTransform", new Pluralize(0, "es")) },

            // Legacy
            { "Img", new ComponentInfo("Image", new Pluralize(0, "s")) },
            { "RawImg", new ComponentInfo("RawImage", new Pluralize(0, "s")) },
            { "Txt", new ComponentInfo("Text", new Pluralize(0, "s")) },
            { "Btn", new ComponentInfo("Button", new Pluralize(0, "s")) },
            { "Tgl", new ComponentInfo("Toggle", new Pluralize(0, "s")) },
            { "Sld", new ComponentInfo("Slider", new Pluralize(0, "s")) },
            { "ScrBar", new ComponentInfo("Scrollbar", new Pluralize(0, "s")) },
            { "ScrView", new ComponentInfo("ScrollRect", new Pluralize(0, "s")) },
            { "Drd", new ComponentInfo("Dropdown", new Pluralize(0, "s")) },
            { "Field", new ComponentInfo("InputField", new Pluralize(0, "s")) },

            // TMP
            { "TmpTxt", new ComponentInfo("TMP_Text", new Pluralize(0, "s")) },
            { "TmpDrd", new ComponentInfo("TMP_Dropdown", new Pluralize(0, "s")) },
            { "TmpField", new ComponentInfo("TMP_InputField", new Pluralize(0, "s")) },

            // Custom
            { "BtnPlus", new ComponentInfo("ButtonPlus", new Pluralize(0, "es"))},
            { "NodePool", new ComponentInfo("NodePool", new Pluralize(0, "s"))}
        };

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
        #endregion
    }
}