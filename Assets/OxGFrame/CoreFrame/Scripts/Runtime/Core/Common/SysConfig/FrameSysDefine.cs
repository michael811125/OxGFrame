using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.CoreFrame
{
    public class FrameSysDefine
    {
        /* 規範符號 */
        public static readonly char BIND_PREFIX = '_';
        public static readonly char BIND_PREFIX_ENTITY = '~';
        public static readonly char BIND_SEPARATOR = '@';
        public static readonly char BIND_END = '#';

        /* 綁定對應表 */
        public static readonly Dictionary<string, string> BIND_COMPONENTS = new Dictionary<string, string>()
        {
            { "_Node", "GameObject"},
            { "~Node", "GameObject"}
        };
    }
}