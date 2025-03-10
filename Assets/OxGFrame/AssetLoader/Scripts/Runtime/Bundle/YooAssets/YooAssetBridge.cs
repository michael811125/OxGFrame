using System;
using System.Reflection;
using UnityEngine;

namespace OxGFrame.AssetLoader
{
    public static class YooAssetBridge
    {
        public static class YooAssetSettingsData
        {
            #region Setting Fields
            /// <summary>
            /// 获取 PackageManifestPrefix
            /// </summary>
            public static string GetPackageManifestPrefix()
            {
                return _GetStringTypeFieldFromSetting("PackageManifestPrefix");
            }
            #endregion

            #region Methods
            /// <summary>
            /// 获取 YOO 的 Resources 目录的全路径
            /// </summary>
            /// <returns></returns>
            public static string GetYooResourcesFullPath()
            {
                return _InvokeStringTypeMethod("GetYooResourcesFullPath");
            }

            /// <summary>
            /// 获取 YOO 默认的缓存文件根目录
            /// </summary>
            /// <returns></returns>
            public static string GetYooDefaultCacheRoot()
            {
                return _InvokeStringTypeMethod("GetYooDefaultCacheRoot");
            }

            /// <summary>
            ///  获取 YOO 默认的内置文件根目录
            /// </summary>
            /// <returns></returns>
            public static string GetYooDefaultBuildinRoot()
            {
                return _InvokeStringTypeMethod("GetYooDefaultBuildinRoot");
            }
            #endregion

            private static string _GetStringTypeFieldFromSetting(string fieldName)
            {
                Type settingsDataType = typeof(YooAsset.YooAssetSettingsData);
                PropertyInfo settingProperty = settingsDataType.GetProperty("Setting", BindingFlags.NonPublic | BindingFlags.Static);
                if (settingProperty == null)
                {
                    Debug.LogError("Property 'Setting' not found in YooAssetSettingsData.");
                    return null;
                }

                object settingsInstance = settingProperty.GetValue(null);
                if (settingsInstance == null)
                {
                    Debug.LogError("Failed to retrieve Setting instance.");
                    return null;
                }

                Type settingsType = settingsInstance.GetType();
                FieldInfo targetField = settingsType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (targetField == null)
                {
                    Debug.LogError($"Field '{fieldName}' not found in Setting instance.");
                    return null;
                }

                return targetField.GetValue(settingsInstance) as string;
            }

            /// <summary>
            /// 透過 Reflection 方式取得 Yoo 內部 Setting 屬性中的參數
            /// </summary>
            private static string _GetStringTypePropertyFromSetting(string propertyName)
            {
                Type settingsDataType = typeof(YooAsset.YooAssetSettingsData);
                PropertyInfo settingProperty = settingsDataType.GetProperty("Setting", BindingFlags.NonPublic | BindingFlags.Static);
                if (settingProperty == null)
                {
                    Debug.LogError("Property 'Setting' not found in YooAssetSettingsData.");
                    return null;
                }

                object settingsInstance = settingProperty.GetValue(null);
                if (settingsInstance == null)
                {
                    Debug.LogError("Failed to retrieve Setting instance.");
                    return null;
                }

                Type settingsType = settingsInstance.GetType();
                PropertyInfo targetProperty = settingsType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (targetProperty == null)
                {
                    Debug.LogError($"Property '{propertyName}' not found in Setting instance.");
                    return null;
                }

                return targetProperty.GetValue(settingsInstance) as string;
            }

            /// <summary>
            /// 透過 Reflection 方式取得 Yoo 內部參數
            /// </summary>
            /// <param name="methodName"></param>
            /// <returns></returns>
            private static string _InvokeStringTypeMethod(string methodName)
            {
                Type type = typeof(YooAsset.YooAssetSettingsData);
                MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    return method.Invoke(null, null) as string;
                }
                Debug.LogError($"Method: \"{methodName}\" not found.");
                return null;
            }
        }
    }
}
