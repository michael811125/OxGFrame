using System;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
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
#if YOOASSET_2
                string typeName = "YooAssetSettingsData";
                string propertyName = "Setting";
                string fieldName = "PackageManifestPrefix";
                return Convert.ToString(ReflectionHelper.GetFieldValueFromProperty(typeName, propertyName, fieldName));
#else
                // [YooAsset v3] YooAssetConfiguration.GetSettings().PackageFilePrefix
                string typeName = "YooAssetConfiguration";
                string methodName = "GetSettings";
                string fieldName = "PackageFilePrefix";
                return Convert.ToString(ReflectionHelper.GetFieldValueFromMethodResult(typeName, methodName, fieldName));
#endif
            }
            #endregion

            #region Methods
            /// <summary>
            /// 获取 YOO 默认的缓存文件根目录
            /// </summary>
            /// <returns></returns>
            public static string GetYooDefaultCacheRoot()
            {
#if YOOASSET_2
                string typeName = "YooAssetSettingsData";
                string methodName = "GetYooDefaultCacheRoot";
#else
                // [YooAsset v3] YooAssetConfiguration.GetDefaultCacheRoot()
                // 注意: v3 在 Editor 下的緩存根目錄由 <project>/yoo 移至 <project>/Library/yoo
                string typeName = "YooAssetConfiguration";
                string methodName = "GetDefaultCacheRoot";
#endif
                return Convert.ToString(ReflectionHelper.InvokeInternalMethod(typeName, methodName));
            }

            /// <summary>
            ///  获取 YOO 默认的内置文件根目录
            /// </summary>
            /// <returns></returns>
            public static string GetYooDefaultBuildinRoot()
            {
#if YOOASSET_2
                string typeName = "YooAssetSettingsData";
                string methodName = "GetYooDefaultBuildinRoot";
#else
                // [YooAsset v3] YooAssetConfiguration.GetDefaultBuiltinRoot()
                string typeName = "YooAssetConfiguration";
                string methodName = "GetDefaultBuiltinRoot";
#endif
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
#if YOOASSET_2
                string typeName = "DownloadSystemHelper";
                string methodName = "ConvertToWWWPath";
#else
                // [YooAsset v3] DownloadUrlHelper.ToLocalFileUrl()
                // 注意: v3 會對空字串拋出例外，並額外轉義 + # ? 字元
                string typeName = "DownloadUrlHelper";
                string methodName = "ToLocalFileUrl";
#endif
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
#if YOOASSET_2
                string typeName = "DefaultBuildinFileSystemDefine";
                string fieldName = "BuildinCatalogBinaryFileName";
#else
                // [YooAsset v3] BuiltinCatalogConsts.BinaryFileName
                // 注意: 檔名常數由 "BuildinCatalog.bytes" 改為 "BuiltinCatalog.bytes"
                string typeName = "BuiltinCatalogConsts";
                string fieldName = "BinaryFileName";
#endif
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
                if (_CheckTypeIsNull(type, typeName))
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
            /// 獲取內部方法回傳實例中的字段
            /// </summary>
            /// <param name="typeName"></param>
            /// <param name="methodName"></param>
            /// <param name="fieldName"></param>
            /// <returns></returns>
            public static object GetFieldValueFromMethodResult(string typeName, string methodName, string fieldName)
            {
                object instance = InvokeInternalMethod(typeName, methodName);
                if (_CheckInstanceIsNull(instance))
                    return null;

                FieldInfo fieldInfo = instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (_CheckFieldIsNull(fieldInfo, fieldName))
                    return null;

                return fieldInfo.GetValue(instance);
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
                if (type == null)
                    return null;

                PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (_CheckPropertyIsNull(property, propertyName))
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
                if (propertyInfo == null)
                    return null;

                object propertyInstance = propertyInfo.GetValue(null);
                if (_CheckInstanceIsNull(propertyInstance))
                    return null;

                FieldInfo fieldInfo = propertyInstance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (_CheckFieldIsNull(fieldInfo, fieldName))
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
                if (type == null)
                    return null;

                PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (_CheckPropertyIsNull(property, propertyName))
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
                if (type == null)
                    return null;

                FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (_CheckFieldIsNull(field, fieldName))
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
                if (type == null)
                    return null;

                // 以參數型別精確匹配，避免同名多載造成 AmbiguousMatchException (ex: v3 的 GetDefaultCacheRoot 具有多載)
                Type[] argTypes = Type.EmptyTypes;
                if (parameters != null && parameters.Length > 0)
                {
                    argTypes = new Type[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                        argTypes[i] = (parameters[i] != null) ? parameters[i].GetType() : typeof(object);
                }

                MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, argTypes, null);
                if (_CheckMethodIsNull(method, methodName))
                    return null;

                return method.Invoke(null, parameters);
            }

            private static bool _CheckTypeIsNull(Type type, string typeName)
            {
                if (type == null)
                {
                    Debug.LogError($"Unable to find class {typeName}");
                    return true;
                }
                return false;
            }

            private static bool _CheckInstanceIsNull(object instance)
            {
                if (instance == null)
                {
                    Debug.LogError("Failed to retrieve instance.");
                    return true;
                }
                return false;
            }

            private static bool _CheckPropertyIsNull(PropertyInfo property, string propertyName)
            {
                if (property == null)
                {
                    Debug.LogError($"Unable to find property {propertyName}");
                    return true;
                }
                return false;
            }

            private static bool _CheckFieldIsNull(FieldInfo field, string fieldName)
            {
                if (field == null)
                {
                    Debug.LogError($"Unable to find field {fieldName}");
                    return true;
                }
                return false;
            }

            private static bool _CheckMethodIsNull(MethodInfo method, string methodName)
            {
                if (method == null)
                {
                    Debug.LogError($"Unable to find method {methodName}");
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
