using System;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.Common.Editor
{
    /// <summary>
    /// API 更新規則的擴充設定 (選用)
    /// <para>放置於專案任意位置即會自動被 API Updater 合併採用，可用於補充框架未涵蓋或專案自身的 API 遷移規則，無需修改任何代碼。</para>
    /// </summary>
    [CreateAssetMenu(fileName = nameof(APIUpdateRulesSettings), menuName = "OxGFrame/Create Settings/Create API Update Rules Settings")]
    public class APIUpdateRulesSettings : ScriptableObject
    {
        /// <summary>
        /// 規則條目
        /// </summary>
        [Serializable]
        public class RuleEntry
        {
            [Tooltip("Old API name (member, parameter or type name).")]
            public string oldApi;

            [Tooltip("New API name.")]
            public string newApi;

            [Tooltip("Qualified: static members, requires the declaring type prefix in code.\n\nInheritedMember: virtual or protected members, only matched inside verified subclasses.\n\nMemberAccess: public instance members, requires the member access form (.oldApi).\n\nNamedArgument: renamed named-parameters (oldApi:).\n\nTypeName: renamed types.")]
            public APIUpdateRule.MatchKind matchKind = APIUpdateRule.MatchKind.Qualified;

            [Tooltip("Simple name of the declaring type. Required for Qualified and InheritedMember, optional for MemberAccess.")]
            public string declaringTypeName;

            [Tooltip("Optional note shown in the API Updater window.")]
            public string description;

            [Tooltip("Optional version in which the change was introduced.")]
            public string sinceVersion;
        }

        [Tooltip("Additional API update rules merged into the API Updater.")]
        public List<RuleEntry> rules = new List<RuleEntry>();
    }
}
