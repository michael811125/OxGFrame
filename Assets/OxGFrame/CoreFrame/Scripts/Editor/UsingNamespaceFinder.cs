using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OxGFrame.CoreFrame.Editor
{
    /// <summary>
    /// 依型別名稱查找可能的 using namespace (透過 TypeCache 索引)
    /// </summary>
    public static class UsingNamespaceFinder
    {
        /// <summary>
        /// 依組件名稱查找候選的 (namespace, assemblyName) 清單，全域 namespace 以空字串表示
        /// </summary>
        /// <param name="componentName"></param>
        /// <returns></returns>
        public static List<(string ns, string assemblyName)> FindCandidates(string componentName)
        {
            var candidates = new List<(string ns, string assemblyName)>();
            if (string.IsNullOrEmpty(componentName))
                return candidates;

            var seen = new HashSet<string>();

            // 優先透過 TypeCache 索引查找 Component 衍生型別 (毫秒級)
            foreach (var type in TypeCache.GetTypesDerivedFrom<Component>())
            {
                if (type.Name != componentName)
                    continue;

                string ns = type.Namespace ?? string.Empty;
                if (seen.Add(ns))
                    candidates.Add((ns, type.Assembly.GetName().Name));
            }

            // 查無 Component 衍生型別時，回退為全域型別掃描 (僅按鈕觸發，成本可接受)
            if (candidates.Count == 0)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch
                    {
                        /* Nothing to do */
                        continue;
                    }

                    foreach (var type in types)
                    {
                        if (type.Name != componentName)
                            continue;

                        string ns = type.Namespace ?? string.Empty;
                        if (seen.Add(ns))
                            candidates.Add((ns, assembly.GetName().Name));
                    }
                }
            }

            // 依 namespace 排序讓結果穩定
            candidates.Sort((a, b) => string.CompareOrdinal(a.ns, b.ns));
            return candidates;
        }

        /// <summary>
        /// 取得候選的顯示名稱
        /// </summary>
        /// <param name="ns"></param>
        /// <returns></returns>
        public static string GetDisplayName(string ns)
        {
            return string.IsNullOrEmpty(ns) ? "(global namespace)" : ns;
        }
    }
}
