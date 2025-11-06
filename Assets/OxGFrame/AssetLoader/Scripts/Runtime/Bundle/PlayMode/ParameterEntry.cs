using System;

namespace OxGFrame.AssetLoader.Bundle
{
    [Serializable]
    public class ParameterEntry
    {
        /// <summary>
        /// 參數是否設置給內置文件系統
        /// </summary>
        public bool isSetForBuiltinFileSystem = false;

        /// <summary>
        /// 參數鍵值
        /// </summary>
        public string parameterKey;

        /// <summary>
        /// 參數數值
        /// </summary>
        public string parameterValue;

        /// <summary>
        /// 參數型別
        /// </summary>
        public string parameterType;
    }
}
