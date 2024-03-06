using System;
using UnityEngine;
using MyBox;
using UnityEditor;

namespace OxGFrame.CoreFrame.Editor
{
    [CreateAssetMenu(fileName = nameof(BindCodeSetting), menuName = "OxGFrame/Create Settings/Create Bind Code Setting")]
    public class BindCodeSetting : ScriptableObject
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

        [Separator("Variable Setting")]
        public CaseType variableCaseType = CaseType.CamelCase;
        public string variableAccessModifier = "protected";
        public string variablePrefix = "_";

        [Separator("Method Setting")]
        [Tooltip("*[Auto] Automatically save binding content to script.\n\n*[Manual] Manually copy the binding content from the clipboard to the script.")]
        public MethodType methodType = MethodType.Auto;

        [Separator("Indicate Modifier Setting")]
        public IndicateModifier indicateModifier = IndicateModifier.This;

        [Separator("Tail Bind Setting")]
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