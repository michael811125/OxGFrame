using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OxGFrame.Common.Editor
{
    /// <summary>
    /// API 更新規則提供者 (自動解析框架的 [Obsolete] 標記 + 手動補充表，並依成員種類決定安全的比對方式)
    /// </summary>
    public static class APIUpdateRules
    {
        /// <summary>
        /// 框架組件名稱前綴
        /// </summary>
        private const string _ASSEMBLY_PREFIX = "OxGFrame.";

        /// <summary>
        /// [Obsolete] 訊息的解析表達式
        /// <para>容許多種常見寫法，讓後續新增的過時標記無需額外維護規則表即可被自動解析。</para>
        /// <para>捕獲值允許帶型別前綴與參數括號 (例如 "Use CoreFrames.UIFrame.Show() instead")，實際取用最後一段成員名稱。</para>
        /// </summary>
        private static readonly Regex[] _OBSOLETE_MESSAGE_REGEXES = new Regex[]
        {
            new Regex(@"\bUse\s+([\w.]+)\s*(?:\([^()]*\))?\s+instead", RegexOptions.IgnoreCase),
            new Regex(@"\bRenamed\s+to\s+([\w.]+)", RegexOptions.IgnoreCase),
            new Regex(@"\bReplaced\s+(?:by|with)\s+([\w.]+)", RegexOptions.IgnoreCase),
            new Regex(@"\bUse\s+([\w.]+)\s*(?:\([^()]*\))?\s*\.?\s*$", RegexOptions.IgnoreCase)
        };

        /// <summary>
        /// 遷移提示關鍵字 (訊息含這些字樣但無法解析出新 API 時，列入需人工遷移的報告而非無聲略過)
        /// </summary>
        private static readonly string[] _MIGRATION_HINT_KEYWORDS = new string[] { "instead", "renamed", "replaced", "use " };

        /// <summary>
        /// 常見名稱清單 (Unity 魔術方法與通用命名)
        /// <para>這些名稱在缺乏型別上下文時絕不比對，避免誤傷無關代碼 (例如 stopwatch.Start()、MonoBehaviour 自身的 Update())。</para>
        /// </summary>
        private static readonly HashSet<string> _COMMON_NAMES = new HashSet<string>()
        {
            // Unity 魔術方法
            "Awake", "Start", "Update", "FixedUpdate", "LateUpdate", "OnEnable", "OnDisable", "OnDestroy",
            "OnGUI", "Reset", "OnValidate", "OnApplicationQuit", "OnApplicationPause", "OnApplicationFocus",
            "OnBecameVisible", "OnBecameInvisible", "OnTriggerEnter", "OnTriggerExit", "OnTriggerStay",
            "OnCollisionEnter", "OnCollisionExit", "OnCollisionStay", "OnMouseDown", "OnMouseUp",
            "OnMouseEnter", "OnMouseExit", "OnMouseOver", "OnMouseDrag", "OnPreRender", "OnPostRender",
            "OnRenderObject", "OnWillRenderObject", "OnDrawGizmos", "OnDrawGizmosSelected", "OnAudioFilterRead",
            "OnLevelWasLoaded", "OnParticleCollision", "OnJointBreak", "OnControllerColliderHit",

            // 通用命名
            "Stop", "Play", "Pause", "Resume", "Add", "Remove", "Clear", "Get", "Set", "Init", "Initialize",
            "Release", "Dispose", "Close", "Open", "Load", "Unload", "Send", "Connect", "Refresh", "Run",
            "Execute", "Invoke", "Tick", "Draw", "Render", "Show", "Hide", "Enable", "Disable", "Next",
            "Previous", "Contains", "Count", "Length", "Name", "Value", "Data", "Size", "Index", "Key",
            "Type", "Id", "Enter", "Exit", "Begin", "End", "Read", "Write", "Push", "Pop", "Peek", "Find",
            "Search", "Sort", "Copy", "Clone", "Equals", "ToString", "GetHashCode"
        };

        /// <summary>
        /// 具名參數規則的複核說明
        /// </summary>
        private const string _NAMED_ARGUMENT_REVIEW_NOTE = "Named arguments cannot be verified against the called method. Do not apply if the parameter belongs to your own method";

        /// <summary>
        /// 手動補充的規則表
        /// <para>適用於無法由 [Obsolete] 自動解析的變更：具名參數、序列化欄位、介面成員與已完全移除的舊成員。</para>
        /// </summary>
        private static readonly APIUpdateRule[] _MANUAL_RULES = new APIUpdateRule[]
        {
            // v2.13.0 (舊虛擬方法, 僅於 UIBase 衍生類別中比對)
            new APIUpdateRule("ShowAnimation", "OnShowAnimation", APIUpdateRule.MatchKind.InheritedMember, APIUpdateRule.SourceType.Manual, "UIBase", "UIBase animation method renamed") { sinceVersion = "2.13.0" },
            new APIUpdateRule("HideAnimation", "OnCloseAnimation", APIUpdateRule.MatchKind.InheritedMember, APIUpdateRule.SourceType.Manual, "UIBase", "UIBase animation method renamed") { sinceVersion = "2.13.0" },

            // v3.7.0
            new APIUpdateRule("disabledPreClose", "disableOnPreClose", APIUpdateRule.MatchKind.NamedArgument, APIUpdateRule.SourceType.Manual, null, "CoreFrames Close/Hide/CloseAll named parameter renamed") { sinceVersion = "3.7.0" },
            new APIUpdateRule("disabledEndEvent", "disableEndEvent", APIUpdateRule.MatchKind.NamedArgument, APIUpdateRule.SourceType.Manual, null, "MediaFrames Stop/StopAll named parameter renamed") { sinceVersion = "3.7.0" },
            new APIUpdateRule("scuredStringType", "securedStringType", APIUpdateRule.MatchKind.MemberAccess, APIUpdateRule.SourceType.Manual, "DecryptInfo", "DecryptInfo serialized field renamed") { sinceVersion = "3.7.0" },
            new APIUpdateRule("CheckIsIntialized", "CheckIsInitialized", APIUpdateRule.MatchKind.InheritedMember, APIUpdateRule.SourceType.Manual, "IDecryptInitialize", "IDecryptInitialize member renamed") { sinceVersion = "3.7.0" },
            new APIUpdateRule("_hearBeatTicker", "_heartBeatTicker", APIUpdateRule.MatchKind.InheritedMember, APIUpdateRule.SourceType.Manual, "NetNode", "NetNode protected field renamed") { sinceVersion = "3.7.0" }
        };

        /// <summary>
        /// 收集所有更新規則
        /// </summary>
        /// <param name="rules">可自動比對的規則</param>
        /// <param name="manualOnlyRules">無法由文字分析安全處理的規則 (需人工遷移)</param>
        public static void CollectRules(out List<APIUpdateRule> rules, out List<APIUpdateRule> manualOnlyRules)
        {
            rules = new List<APIUpdateRule>();
            manualOnlyRules = new List<APIUpdateRule>();

            var allTypes = _CollectAllTypes();
            var seenKeys = new HashSet<string>();

            // 手動補充表優先
            foreach (var rule in _MANUAL_RULES)
            {
                if (!seenKeys.Add($"{rule.declaringTypeName}.{rule.oldApi}"))
                    continue;

                // 解析宣告型別 (供衍生關係驗證使用)
                if (!string.IsNullOrEmpty(rule.declaringTypeName))
                    rule.declaringType = _FindTypeByName(allTypes, rule.declaringTypeName);

                rule.isCommonName = _COMMON_NAMES.Contains(rule.oldApi);
                _Dispatch(rule, allTypes, rules, manualOnlyRules);
            }

            // 專案中的擴充設定 (選用, 讓維護者與使用者皆可補充規則而無需修改代碼)
            foreach (var rule in _CollectSettingsRules(allTypes))
            {
                if (!seenKeys.Add($"{rule.declaringTypeName}.{rule.oldApi}"))
                    continue;
                _Dispatch(rule, allTypes, rules, manualOnlyRules);
            }

            // 自動解析框架組件中的 [Obsolete] 成員
            foreach (var rule in _CollectObsoleteRules(allTypes, manualOnlyRules))
            {
                if (!seenKeys.Add($"{rule.declaringTypeName}.{rule.oldApi}"))
                    continue;
                _Dispatch(rule, allTypes, rules, manualOnlyRules);
            }
        }

        /// <summary>
        /// 收集專案中 APIUpdateRulesSettings 資產所提供的擴充規則
        /// </summary>
        /// <param name="allTypes"></param>
        /// <returns></returns>
        private static IEnumerable<APIUpdateRule> _CollectSettingsRules(List<Type> allTypes)
        {
            var rules = new List<APIUpdateRule>();

            string[] guids;
            try
            {
                guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(APIUpdateRulesSettings)}");
            }
            catch
            {
                /* Nothing to do */
                return rules;
            }

            foreach (string guid in guids)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<APIUpdateRulesSettings>(assetPath);
                if (settings == null ||
                    settings.rules == null)
                    continue;

                foreach (var entry in settings.rules)
                {
                    if (entry == null ||
                        string.IsNullOrEmpty(entry.oldApi) ||
                        string.IsNullOrEmpty(entry.newApi) ||
                        entry.oldApi == entry.newApi)
                        continue;

                    var rule = new APIUpdateRule(entry.oldApi, entry.newApi, entry.matchKind, APIUpdateRule.SourceType.Manual, entry.declaringTypeName, string.IsNullOrEmpty(entry.description) ? $"From {assetPath}" : entry.description);
                    rule.sinceVersion = entry.sinceVersion;
                    rule.isCommonName = _COMMON_NAMES.Contains(entry.oldApi);
                    if (!string.IsNullOrEmpty(entry.declaringTypeName))
                        rule.declaringType = _FindTypeByName(allTypes, entry.declaringTypeName);
                    rules.Add(rule);
                }
            }

            return rules;
        }

        /// <summary>
        /// 依規則的比對方式與名稱風險，分派至可自動比對或需人工遷移的清單
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="allTypes"></param>
        /// <param name="rules"></param>
        /// <param name="manualOnlyRules"></param>
        private static void _Dispatch(APIUpdateRule rule, List<Type> allTypes, List<APIUpdateRule> rules, List<APIUpdateRule> manualOnlyRules)
        {
            switch (rule.matchKind)
            {
                case APIUpdateRule.MatchKind.ManualOnly:
                    manualOnlyRules.Add(rule);
                    return;

                case APIUpdateRule.MatchKind.Qualified:
                    {
                        // 靜態成員必須帶型別前綴; 允許的前綴為宣告型別本身與其所有衍生型別 (C# 可透過衍生型別名稱呼叫繼承的靜態成員)
                        if (rule.declaringType == null)
                        {
                            rule.description = $"{rule.description}\nCannot resolve the declaring type, so the qualified usage cannot be verified.";
                            manualOnlyRules.Add(rule);
                            return;
                        }
                        rule.qualifierNames = _CollectQualifierNames(allTypes, rule.declaringType);
                        rules.Add(rule);
                        return;
                    }

                case APIUpdateRule.MatchKind.InheritedMember:
                    {
                        // 需要宣告型別才能驗證使用者類別的衍生關係
                        if (rule.declaringType == null)
                        {
                            rule.description = $"{rule.description}\nCannot resolve the declaring type, so subclass usage cannot be verified.";
                            manualOnlyRules.Add(rule);
                            return;
                        }
                        rules.Add(rule);
                        return;
                    }

                case APIUpdateRule.MatchKind.NamedArgument:
                    {
                        // 具名參數僅能比對 name: 形式, 無法驗證被呼叫的方法是否屬於框架, 一律需人工複核
                        rule.alwaysNeedsReview = true;
                        rule.reviewNote = _NAMED_ARGUMENT_REVIEW_NOTE;
                        rules.Add(rule);
                        return;
                    }

                case APIUpdateRule.MatchKind.MemberAccess:
                case APIUpdateRule.MatchKind.TypeName:
                    {
                        // 常見名稱在缺乏型別上下文時不予比對
                        if (rule.isCommonName)
                        {
                            rule.description = $"{rule.description}\nThe name is too common to be matched safely without type context.";
                            manualOnlyRules.Add(rule);
                            return;
                        }
                        rules.Add(rule);
                        return;
                    }

                default:
                    rules.Add(rule);
                    return;
            }
        }

        /// <summary>
        /// 自動解析框架組件中的 [Obsolete] 成員，生成更新規則
        /// <para>同一 (宣告型別, 舊 API) 因多載對應不同新 API 時，合併為單一歧義規則並保留所有候選與其參數資訊，供掃描時依呼叫引數自動決議。</para>
        /// </summary>
        /// <param name="allTypes"></param>
        /// <param name="manualOnlyRules">訊息含遷移提示但無法解析出新 API 的成員，列入此清單供人工參考</param>
        /// <returns></returns>
        private static IEnumerable<APIUpdateRule> _CollectObsoleteRules(List<Type> allTypes, List<APIUpdateRule> manualOnlyRules)
        {
            var rawRules = new List<APIUpdateRule>();

            foreach (var type in allTypes)
            {
                string assemblyName = _GetAssemblyName(type);
                if (assemblyName == null ||
                    !assemblyName.StartsWith(_ASSEMBLY_PREFIX, StringComparison.Ordinal))
                    continue;

                // 型別自身的標記
                _TryCollect(rawRules, manualOnlyRules, type, type, type.Name, assemblyName);

                MemberInfo[] members;
                try
                {
                    members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
                }
                catch
                {
                    /* Nothing to do */
                    continue;
                }

                foreach (var member in members)
                    _TryCollect(rawRules, manualOnlyRules, member, type, member.Name, assemblyName);
            }

            // 合併多載 (依宣告型別 + 舊 API 分組)
            var mergedRules = new List<APIUpdateRule>();
            foreach (var group in rawRules.GroupBy(rule => $"{rule.declaringTypeName}.{rule.oldApi}"))
            {
                var firstRule = group.First();

                // 依新 API 名稱歸併候選 (同名新 API 的多個多載取可行性最寬鬆的參數資訊)
                var candidates = group
                    .GroupBy(rule => rule.newApi)
                    .Select(candidateGroup => new APIUpdateCandidate(
                        candidateGroup.Key,
                        candidateGroup.Max(rule => (rule.candidates != null && rule.candidates.Count > 0) ? rule.candidates[0].parameterCount : 0),
                        candidateGroup.Min(rule => (rule.candidates != null && rule.candidates.Count > 0) ? rule.candidates[0].requiredParameterCount : 0)))
                    .ToList();

                if (candidates.Count > 1)
                {
                    firstRule.isAmbiguous = true;
                    firstRule.candidates = candidates;
                    firstRule.newApi = string.Join(" / ", candidates.Select(candidate => candidate.newApi));
                    firstRule.description = $"{firstRule.description}\nAmbiguous: overloads map to different APIs. Calls are resolved by their arguments automatically; unresolved usages require choosing the new API manually.";
                }
                else
                {
                    // 非歧義規則不需要候選資訊
                    firstRule.candidates = null;
                }

                mergedRules.Add(firstRule);
            }

            return mergedRules;
        }

        /// <summary>
        /// 嘗試由成員的 [Obsolete] 標記生成規則
        /// </summary>
        /// <param name="rawRules"></param>
        /// <param name="manualOnlyRules"></param>
        /// <param name="member"></param>
        /// <param name="declaringType"></param>
        /// <param name="memberName"></param>
        /// <param name="assemblyName"></param>
        private static void _TryCollect(List<APIUpdateRule> rawRules, List<APIUpdateRule> manualOnlyRules, MemberInfo member, Type declaringType, string memberName, string assemblyName)
        {
            if (string.IsNullOrEmpty(memberName))
                return;

            // 略過編譯器生成的存取子與運算子, 以及泛型型別的名稱修飾
            if (memberName.IndexOf('`') >= 0 ||
                memberName.StartsWith("get_", StringComparison.Ordinal) ||
                memberName.StartsWith("set_", StringComparison.Ordinal) ||
                memberName.StartsWith("add_", StringComparison.Ordinal) ||
                memberName.StartsWith("remove_", StringComparison.Ordinal) ||
                memberName.StartsWith("op_", StringComparison.Ordinal) ||
                memberName.StartsWith("<", StringComparison.Ordinal))
                return;

            ObsoleteAttribute obsolete;
            try
            {
                obsolete = member.GetCustomAttribute<ObsoleteAttribute>(false);
            }
            catch
            {
                /* Nothing to do */
                return;
            }

            if (obsolete == null ||
                string.IsNullOrEmpty(obsolete.Message))
                return;

            string declaringTypeName = GetSimpleTypeName(declaringType);

            // 解析訊息中的新 API 名稱
            string newApi = null;
            foreach (var messageRegex in _OBSOLETE_MESSAGE_REGEXES)
            {
                var messageMatch = messageRegex.Match(obsolete.Message);
                if (!messageMatch.Success)
                    continue;
                newApi = messageMatch.Groups[1].Value;
                break;
            }

            // 帶型別前綴時取最後一段成員名稱
            if (!string.IsNullOrEmpty(newApi))
            {
                newApi = newApi.Trim('.');
                int lastDotIndex = newApi.LastIndexOf('.');
                if (lastDotIndex >= 0)
                    newApi = newApi.Substring(lastDotIndex + 1);
            }

            if (string.IsNullOrEmpty(newApi))
            {
                // 訊息含遷移提示但無法解析出新 API 時, 列入需人工遷移的報告 (單純的說明性訊息則略過)
                if (_MIGRATION_HINT_KEYWORDS.Any(keyword => obsolete.Message.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    var manualRule = new APIUpdateRule(memberName, "(see note)", APIUpdateRule.MatchKind.ManualOnly, APIUpdateRule.SourceType.Obsolete, declaringTypeName, $"[Obsolete] {obsolete.Message}");
                    manualOnlyRules.Add(manualRule);
                }
                return;
            }

            if (newApi == memberName)
                return;

            bool isType = member is Type;
            bool isCommonName = _COMMON_NAMES.Contains(memberName);
            APIUpdateRule.MatchKind matchKind;

            if (isType)
            {
                matchKind = APIUpdateRule.MatchKind.TypeName;
            }
            else
            {
                _GetMemberTraits(member, out bool isStatic, out bool isOverridable, out bool isProtected);
                if (isStatic)
                    matchKind = APIUpdateRule.MatchKind.Qualified;
                else if (isOverridable || isProtected)
                    matchKind = APIUpdateRule.MatchKind.InheritedMember;
                else
                    matchKind = APIUpdateRule.MatchKind.MemberAccess;
            }

            // 記錄對應多載的參數資訊 (供歧義規則依呼叫引數自動決議)
            int parameterCount = 0;
            int requiredParameterCount = 0;
            if (member is MethodInfo methodInfo)
            {
                try
                {
                    var parameters = methodInfo.GetParameters();
                    parameterCount = parameters.Length;
                    requiredParameterCount = parameters.Count(parameter => !parameter.IsOptional);
                }
                catch
                {
                    /* Nothing to do */
                }
            }

            var rule = new APIUpdateRule(memberName, newApi, matchKind, APIUpdateRule.SourceType.Obsolete, declaringTypeName, $"[Obsolete] {declaringTypeName}.{memberName} in {assemblyName}", false, isCommonName);
            rule.declaringType = isType ? declaringType : (member.DeclaringType ?? declaringType);
            rule.candidates = new List<APIUpdateCandidate>() { new APIUpdateCandidate(newApi, parameterCount, requiredParameterCount) };
            rawRules.Add(rule);
        }

        /// <summary>
        /// 取得成員特徵 (是否靜態、可覆寫與保護層級)
        /// </summary>
        /// <param name="member"></param>
        /// <param name="isStatic"></param>
        /// <param name="isOverridable"></param>
        /// <param name="isProtected"></param>
        private static void _GetMemberTraits(MemberInfo member, out bool isStatic, out bool isOverridable, out bool isProtected)
        {
            isStatic = false;
            isOverridable = false;
            isProtected = false;

            MethodInfo probeMethod = null;
            if (member is MethodInfo method)
                probeMethod = method;
            else if (member is PropertyInfo property)
                probeMethod = property.GetMethod ?? property.SetMethod;
            else if (member is EventInfo eventInfo)
                probeMethod = eventInfo.AddMethod ?? eventInfo.RemoveMethod;
            else if (member is FieldInfo field)
            {
                isStatic = field.IsStatic;
                isProtected = field.IsFamily || field.IsFamilyOrAssembly;
                return;
            }

            if (probeMethod == null)
                return;

            isStatic = probeMethod.IsStatic;
            isOverridable = probeMethod.IsVirtual || probeMethod.IsAbstract;
            isProtected = probeMethod.IsFamily || probeMethod.IsFamilyOrAssembly;
        }

        /// <summary>
        /// 收集所有已載入型別
        /// </summary>
        /// <returns></returns>
        private static List<Type> _CollectAllTypes()
        {
            var allTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    allTypes.AddRange(assembly.GetTypes());
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // 部分型別載入失敗時仍保留可用的型別 (整批略過會導致該組件的規則全數遺失)
                    allTypes.AddRange(ex.Types.Where(type => type != null));
                }
                catch
                {
                    /* Nothing to do */
                }
            }
            return allTypes;
        }

        /// <summary>
        /// 依名稱尋找型別 (優先框架組件，其次任一已載入組件)
        /// </summary>
        /// <param name="allTypes"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static Type _FindTypeByName(List<Type> allTypes, string typeName)
        {
            var frameworkType = _FindFrameworkType(allTypes, typeName);
            if (frameworkType != null)
                return frameworkType;

            foreach (var type in allTypes)
            {
                if (GetSimpleTypeName(type) == typeName)
                    return type;
            }
            return null;
        }

        /// <summary>
        /// 於框架組件中依名稱尋找型別
        /// </summary>
        /// <param name="allTypes"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static Type _FindFrameworkType(List<Type> allTypes, string typeName)
        {
            foreach (var type in allTypes)
            {
                if (GetSimpleTypeName(type) != typeName)
                    continue;
                string assemblyName = _GetAssemblyName(type);
                if (assemblyName != null &&
                    assemblyName.StartsWith(_ASSEMBLY_PREFIX, StringComparison.Ordinal))
                    return type;
            }
            return null;
        }

        /// <summary>
        /// 收集可作為靜態成員前綴的型別名稱 (宣告型別本身與其所有衍生型別)
        /// </summary>
        /// <param name="allTypes"></param>
        /// <param name="declaringType"></param>
        /// <returns></returns>
        private static string[] _CollectQualifierNames(List<Type> allTypes, Type declaringType)
        {
            var names = new HashSet<string>() { GetSimpleTypeName(declaringType) };
            foreach (var type in allTypes)
            {
                if (type == declaringType)
                    continue;
                if (IsDerivedFrom(type, declaringType))
                    names.Add(GetSimpleTypeName(type));
            }
            return names.Where(name => !string.IsNullOrEmpty(name)).ToArray();
        }

        /// <summary>
        /// 檢查型別是否衍生自 (或實作) 指定型別，支援開放泛型基類與介面
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static bool IsDerivedFrom(Type type, Type baseType)
        {
            if (type == null ||
                baseType == null)
                return false;

            try
            {
                if (baseType.IsInterface)
                    return baseType.IsAssignableFrom(type);

                if (baseType.IsGenericTypeDefinition)
                {
                    for (var current = type; current != null; current = current.BaseType)
                    {
                        if (current.IsGenericType &&
                            current.GetGenericTypeDefinition() == baseType)
                            return true;
                    }
                    return false;
                }

                return baseType.IsAssignableFrom(type);
            }
            catch
            {
                /* Nothing to do */
                return false;
            }
        }

        /// <summary>
        /// 取得型別的簡短名稱 (去除泛型參數個數修飾)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetSimpleTypeName(Type type)
        {
            if (type == null)
                return null;
            string name = type.Name;
            int backtickIndex = name.IndexOf('`');
            return (backtickIndex >= 0) ? name.Substring(0, backtickIndex) : name;
        }

        /// <summary>
        /// 取得型別所屬的組件名稱
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string _GetAssemblyName(Type type)
        {
            try
            {
                return type.Assembly.GetName().Name;
            }
            catch
            {
                /* Nothing to do */
                return null;
            }
        }
    }
}
