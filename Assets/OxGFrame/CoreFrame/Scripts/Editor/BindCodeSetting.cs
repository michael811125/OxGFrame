using System;
using UnityEngine;

namespace OxGFrame.CoreFrame.Editor
{
    [CreateAssetMenu(fileName = nameof(BindCodeSetting), menuName = "OxGFrame/Create Settings/Create Bind Code Setting")]
    public class BindCodeSetting : ScriptableObject
    {
        [Serializable]
        public struct ComponentInfo
        {
            public string componentName;
            public string pluralRule;

            public ComponentInfo(string componentName, string pluralRule)
            {
                this.componentName = componentName;
                this.pluralRule = pluralRule;
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

        [Header("Variable Setting")]
        public CaseType variableCaseType = CaseType.CamelCase;
        public string variableAccessModifier = "protected";
        public string variablePrefix = "_";

        [Header("Method Setting")]
        public string methodAccessModifier = "protected";
        public string methodPrefix = "";
        public string methodName = "InitComponents";

        [Header("Indicate Modifier Setting")]
        public IndicateModifier indicateModifier = IndicateModifier.This;

        [Header("Tail Bind Setting")]
        public GenericDictionary<string, ComponentInfo> _tailRules = new GenericDictionary<string, ComponentInfo>()
        {
            // Other
            { "Trans", new ComponentInfo("Transform", "es") },
            { "RectTrans", new ComponentInfo("RectTransform", "es") },

            // Legacy
            { "Img", new ComponentInfo("Image", "s") },
            { "RawImg", new ComponentInfo("RawImage", "s") },
            { "Txt", new ComponentInfo("Text", "s") },
            { "Btn", new ComponentInfo("Button", "s") },
            { "Tgl", new ComponentInfo("Toggle", "s") },
            { "Sld", new ComponentInfo("Slider", "s") },
            { "ScrBar", new ComponentInfo("Scrollbar", "s") },
            { "ScrView", new ComponentInfo("ScrollRect", "s") },
            { "Drd", new ComponentInfo("Dropdown", "s") },
            { "Field", new ComponentInfo("InputField", "s") },

            // TMP
            { "TmpTxt", new ComponentInfo("TMP_Text", "s") },
            { "TmpDrd", new ComponentInfo("TMP_Dropdown", "s") },
            { "TmpField", new ComponentInfo("TMP_InputField", "s") },

            // Custom
            { "BtnPlus", new ComponentInfo("ButtonPlus", "es")},
            { "NodePool", new ComponentInfo("NodePool", "s")}
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
    }
}