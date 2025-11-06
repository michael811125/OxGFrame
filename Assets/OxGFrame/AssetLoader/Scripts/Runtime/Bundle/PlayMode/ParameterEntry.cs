using System;

namespace OxGFrame.AssetLoader.Bundle
{
    [Serializable]
    public class ParameterEntry
    {
        public enum ParameterTarget
        {
            All = 0,
            BuiltinFileSystem = 1,
            CacheFileSystem = 2
        }

        /// <summary>
        /// 參數設置目標
        /// </summary>
        public ParameterTarget parameterTarget = ParameterTarget.All;

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
