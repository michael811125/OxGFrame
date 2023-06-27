using UnityEngine;

namespace OxGFrame.CoreFrame.Editor
{
    [CreateAssetMenu(fileName = nameof(BindCodeSetting), menuName = "OxGFrame/Create Settings/Bind Code Setting")]
    public class BindCodeSetting : ScriptableObject
    {
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
        public GenericDictionary<string, string> _tailRules = new GenericDictionary<string, string>()
        {
            // Other
            { "Trans", "Transform" },
            { "RectTrans", "RectTransform" },

            // Legacy
            { "Img", "Image" },
            { "RawImg", "RawImage" },
            { "Txt", "Text" },
            { "Btn", "Button" },
            { "Tgl", "Toggle" },
            { "Sld", "Slider" },
            { "ScrBar", "Scrollbar" },
            { "ScrView", "ScrollRect" },
            { "Drd", "Dropdown" },
            { "Field", "InputField" },

            // TMP
            { "TmpTxt", "TMP_Text" },
            { "TmpDrd", "TMP_Dropdown" },
            { "TmpField", "TMP_InputField" }
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