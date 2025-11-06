using System;
using System.Collections.Generic;

namespace OxGFrame.AssetLoader.Bundle
{
    /// <summary>
    /// 參數型別解析器
    /// </summary>
    public class ParameterParser
    {
        /// <summary>
        /// 支持的型別對應表
        /// </summary>
        private static readonly Dictionary<string, Type> _supportedTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            // Boolean
            { "bool", typeof(bool) },
            { "boolean", typeof(bool) },
            
            // Integer types
            { "int", typeof(int) },
            { "int32", typeof(int) },
            { "uint", typeof(uint) },
            { "uint32", typeof(uint) },
            { "byte", typeof(byte) },
            { "sbyte", typeof(sbyte) },
            { "short", typeof(short) },
            { "int16", typeof(short) },
            { "ushort", typeof(ushort) },
            { "uint16", typeof(ushort) },
            { "long", typeof(long) },
            { "int64", typeof(long) },
            { "ulong", typeof(ulong) },
            { "uint64", typeof(ulong) },
            
            // Floating point types
            { "float", typeof(float) },
            { "single", typeof(float) },
            { "double", typeof(double) },
            { "decimal", typeof(decimal) },
            
            // Other common types
            { "string", typeof(string) },
            { "char", typeof(char) }
        };

        /// <summary>
        /// 解析參數並返回 object
        /// </summary>
        /// <param name="parameterValue"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static object Parse(string parameterValue, string parameterType)
        {
            if (string.IsNullOrEmpty(parameterType))
                throw new ArgumentException("parameterType cannot be null or empty");

            if (!_supportedTypes.TryGetValue(parameterType, out Type targetType))
            {
                throw new NotSupportedException($"Type '{parameterType}' is not supported. " + $"Supported types: {string.Join(", ", GetSupportedTypeNames())}");
            }

            return _ParseValue(parameterValue, targetType, parameterType);
        }

        /// <summary>
        /// 解析參數並返回指定型別
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterValue"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        public static T Parse<T>(string parameterValue, string parameterType)
        {
            object result = Parse(parameterValue, parameterType);

            if (result is T typedResult)
                return typedResult;

            // 嘗試轉換
            try
            {
                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Cannot convert value '{result}' of type '{result.GetType()}' to type '{typeof(T)}'", ex);
            }
        }

        /// <summary>
        /// 嘗試解析參數
        /// </summary>
        /// <param name="parameterValue"></param>
        /// <param name="parameterType"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse(string parameterValue, string parameterType, out object result)
        {
            result = null;
            try
            {
                result = Parse(parameterValue, parameterType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 嘗試解析參數並返回指定型別
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameterValue"></param>
        /// <param name="parameterType"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse<T>(string parameterValue, string parameterType, out T result)
        {
            result = default(T);
            try
            {
                result = Parse<T>(parameterValue, parameterType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 檢查型別是否支持
        /// </summary>
        /// <param name="typeName">型別名稱</param>
        /// <returns>是否支持</returns>
        public static bool IsTypeSupported(string typeName)
        {
            return !string.IsNullOrEmpty(typeName) && _supportedTypes.ContainsKey(typeName);
        }

        /// <summary>
        /// 取得所有支持的型別名稱
        /// </summary>
        /// <returns>型別名稱列表</returns>
        public static IEnumerable<string> GetSupportedTypeNames()
        {
            var uniqueTypes = new HashSet<string>();
            foreach (var key in _supportedTypes.Keys)
            {
                // 只顯示主要名稱 (如 int, bool, float 等)
                if (!key.Contains("32") &&
                    !key.Contains("16") &&
                    !key.Contains("64") &&
                    key != "single" &&
                    key != "boolean")
                {
                    uniqueTypes.Add(key);
                }
            }
            return uniqueTypes;
        }

        /// <summary>
        /// 核心解析方法
        /// </summary>
        private static object _ParseValue(string value, Type targetType, string typeName)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (targetType == typeof(string))
                    return string.Empty;

                throw new ArgumentException($"Value cannot be null or empty for type '{typeName}'");
            }

            try
            {
                // Boolean
                if (targetType == typeof(bool))
                {
                    if (bool.TryParse(value, out bool boolResult))
                        return boolResult;
                    // 支持 1/0, yes/no
                    if (value == "1" ||
                        value.Equals("yes", StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (value == "0" ||
                        value.Equals("no", StringComparison.OrdinalIgnoreCase))
                        return false;
                    throw new FormatException($"Cannot parse '{value}' as boolean");
                }

                // String
                if (targetType == typeof(string))
                    return value;

                // Char
                if (targetType == typeof(char))
                {
                    if (value.Length == 1)
                        return value[0];
                    throw new FormatException($"Cannot parse '{value}' as char (must be single character)");
                }

                // Numeric types
                if (targetType == typeof(int))
                    return int.Parse(value);
                if (targetType == typeof(uint))
                    return uint.Parse(value);
                if (targetType == typeof(byte))
                    return byte.Parse(value);
                if (targetType == typeof(sbyte))
                    return sbyte.Parse(value);
                if (targetType == typeof(short))
                    return short.Parse(value);
                if (targetType == typeof(ushort))
                    return ushort.Parse(value);
                if (targetType == typeof(long))
                    return long.Parse(value);
                if (targetType == typeof(ulong))
                    return ulong.Parse(value);
                if (targetType == typeof(float))
                    return float.Parse(value);
                if (targetType == typeof(double))
                    return double.Parse(value);
                if (targetType == typeof(decimal))
                    return decimal.Parse(value);

                throw new NotSupportedException($"Type '{targetType}' is not supported");
            }
            catch (FormatException)
            {
                throw new FormatException($"Cannot parse value '{value}' as type '{typeName}'");
            }
            catch (OverflowException)
            {
                throw new OverflowException($"Value '{value}' is out of range for type '{typeName}'");
            }
        }
    }
}