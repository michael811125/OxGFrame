using System;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader
{
    public static class YooAssetBridge
    {
        public static class YooAssetSettingsData
        {
            #region Settings Fields
            /// <summary>
            /// 获取 PackageManifestPrefix
            /// </summary>
            public static string GetPackageManifestPrefix()
            {
                string typeName = "YooAssetSettingsData";
                string propertyName = "Setting";
                string fieldName = "PackageManifestPrefix";
                return Convert.ToString(ReflectionHelper.GetFieldValueFromProperty(typeName, propertyName, fieldName));
            }
            #endregion

            #region Methods
            /// <summary>
            /// 获取 YOO 默认的缓存文件根目录
            /// </summary>
            /// <returns></returns>
            public static string GetYooDefaultCacheRoot()
            {
                string typeName = "YooAssetSettingsData";
                string methodName = "GetYooDefaultCacheRoot";
                return Convert.ToString(ReflectionHelper.InvokeInternalMethod(typeName, methodName));
            }

            /// <summary>
            ///  获取 YOO 默认的内置文件根目录
            /// </summary>
            /// <returns></returns>
            public static string GetYooDefaultBuildinRoot()
            {
                string typeName = "YooAssetSettingsData";
                string methodName = "GetYooDefaultBuildinRoot";
                return Convert.ToString(ReflectionHelper.InvokeInternalMethod(typeName, methodName));
            }
            #endregion
        }

        public static class DownloadSystemHelper
        {
            #region Methods
            /// <summary>
            /// 获取 WWW 加载本地资源的路径
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string ConvertToWWWPath(string path)
            {
                string typeName = "DownloadSystemHelper";
                string methodName = "ConvertToWWWPath";
                return Convert.ToString(ReflectionHelper.InvokeInternalMethod(typeName, methodName, path));
            }
            #endregion
        }

        public static class DefaultBuildinFileSystemDefine
        {
            #region Fields
            /// <summary>
            /// 内置清单二进制文件名称
            /// </summary>
            /// <returns></returns>
            public static string BuildinCatalogBinaryFileName()
            {
                string typeName = "DefaultBuildinFileSystemDefine";
                string fieldName = "BuildinCatalogBinaryFileName";
                return Convert.ToString(ReflectionHelper.GetInternalField(typeName, fieldName));
            }
            #endregion
        }

        #region Reflection Helper
        internal static class ReflectionHelper
        {
            /// <summary>
            /// 緩存 YooAssembly
            /// </summary>
            private static Assembly _yooAssembly = null;

            /// <summary>
            /// 獲取內部類型
            /// </summary>
            /// <param name="typeName"></param>
            /// <returns></returns>
            public static Type GetInternalType(string typeName)
            {
                _TryCacheYooAssembly();
                Type type = _yooAssembly.GetType($"YooAsset.{typeName}");
                if (_CheckTypeIsNull(type))
                    return null;
                return type;
            }

            /// <summary>
            /// 獲取內部屬性中的字段
            /// </summary>
            /// <param name="typeName"></param>
            /// <param name="propertyName"></param>
            /// <param name="fieldName"></param>
            /// <returns></returns>
            public static object GetFieldValueFromProperty(string typeName, string propertyName, string fieldName)
            {
                PropertyInfo property = GetInternalPropertyInfo(typeName, propertyName);
                return GetFieldValueFromPropertyInfo(property, fieldName);
            }

            /// <summary>
            /// 獲取內部屬性信息
            /// </summary>
            /// <param name="typeName"></param>
            /// <param name="propertyName"></param>
            /// <returns></returns>
            public static PropertyInfo GetInternalPropertyInfo(string typeName, string propertyName)
            {
                Type type = GetInternalType(typeName);

                PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (_CheckPropertyIsNull(property))
                    return null;

                return property;
            }

            /// <summary>
            /// 獲取內部屬性中的字段
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="fieldName"></param>
            /// <returns></returns>
            public static object GetFieldValueFromPropertyInfo(PropertyInfo propertyInfo, string fieldName)
            {
                object propertyInstance = propertyInfo.GetValue(null);
                if (_CheckPropertyInstanceIsNull(propertyInstance))
                    return null;

                FieldInfo fieldInfo = propertyInstance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (_CheckFieldIsNull(fieldInfo))
                    return null;

                return fieldInfo.GetValue(propertyInstance);
            }

            /// <summary>
            /// 獲取內部屬性
            /// </summary>
            /// <param name="typeName"></param>
            /// <param name="propertyName"></param>
            /// <returns></returns>
            public static object GetInternalProperty(string typeName, string propertyName)
            {
                Type type = GetInternalType(typeName);

                PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (_CheckPropertyIsNull(property))
                    return null;

                return property.GetValue(null);
            }

            /// <summary>
            /// 獲取內部字段
            /// </summary>
            /// <param name="typeName"></param>
            /// <param name="fieldName"></param>
            /// <returns></returns>
            public static object GetInternalField(string typeName, string fieldName)
            {
                Type type = GetInternalType(typeName);

                FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (_CheckFieldIsNull(field))
                    return null;

                return field.GetValue(null);
            }

            /// <summary>
            /// 獲取內部方法
            /// </summary>
            /// <param name="typeName"></param>
            /// <param name="methodName"></param>
            /// <param name="parameters"></param>
            /// <returns></returns>
            public static object InvokeInternalMethod(string typeName, string methodName, params object[] parameters)
            {
                Type type = GetInternalType(typeName);

                MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (_CheckMethodIsNull(method))
                    return null;

                return method.Invoke(null, parameters);
            }

            private static bool _CheckTypeIsNull(Type type)
            {
                if (type == null)
                {
                    Debug.LogError($"Unable to find class {type.FullName}");
                    return true;
                }
                return false;
            }

            private static bool _CheckPropertyInstanceIsNull(object propertyInstance)
            {
                if (propertyInstance == null)
                {
                    Debug.LogError("Failed to retrieve Property instance.");
                    return true;
                }
                return false;
            }

            private static bool _CheckPropertyIsNull(PropertyInfo property)
            {
                if (property == null)
                {
                    Debug.LogError($"Unable to find property {property.Name}");
                    return true;
                }
                return false;
            }

            private static bool _CheckFieldIsNull(FieldInfo field)
            {
                if (field == null)
                {
                    Debug.LogError($"Unable to find field {field.Name}");
                    return true;
                }
                return false;
            }

            private static bool _CheckMethodIsNull(MethodInfo method)
            {
                if (method == null)
                {
                    Debug.LogError($"Unable to find method {method.Name}");
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 緩存 YooAssembly
            /// </summary>
            private static void _TryCacheYooAssembly()
            {
                if (_yooAssembly == null)
                    _yooAssembly = typeof(YooAssets).Assembly;
            }
        }
        #endregion
    }
}
