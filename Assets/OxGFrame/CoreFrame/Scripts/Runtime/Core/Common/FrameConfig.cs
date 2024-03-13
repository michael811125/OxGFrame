using System.Collections.Generic;

namespace OxGFrame.CoreFrame
{
    public class FrameConfig
    {
        /* 規範符號 */
        public static readonly string BIND_HEAD_SEPARATOR = "@";
        public static readonly string BIND_TAIL_SEPARATOR = "*";
        public static readonly string BIND_ACCESS_MODIFIER_SEPARATOR = "$";
        public static readonly string BIND_STOP_END = "#";

        /* 綁定規範前墜 */
        public static readonly string[] BIND_PREFIXES = new string[]
        {
            "_",
            "~"
        };

        /* 綁定對應表 */
        public static readonly Dictionary<string, string> BIND_COMPONENTS = new Dictionary<string, string>()
        {
            { "_Node", "GameObject"},
            { "~Node", "GameObject"}
        };
    }
}