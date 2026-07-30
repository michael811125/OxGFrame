using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OxGFrame.Common.Editor
{
    /// <summary>
    /// 歧義規則的新 API 候選 (同名舊 API 因多載對應不同的新 API 時，每個新 API 為一個候選)
    /// </summary>
    public class APIUpdateCandidate
    {
        /// <summary>
        /// 新 API 名稱
        /// </summary>
        public string newApi;

        /// <summary>
        /// 對應多載的參數個數
        /// </summary>
        public int parameterCount;

        /// <summary>
        /// 對應多載的必填參數個數 (0 表示可以無引數呼叫)
        /// </summary>
        public int requiredParameterCount;

        public APIUpdateCandidate(string newApi, int parameterCount, int requiredParameterCount)
        {
            this.newApi = newApi;
            this.parameterCount = parameterCount;
            this.requiredParameterCount = requiredParameterCount;
        }
    }

    /// <summary>
    /// API 更新規則
    /// </summary>
    public class APIUpdateRule
    {
        /// <summary>
        /// 規則來源類型
        /// </summary>
        public enum SourceType
        {
            /// <summary>
            /// 由框架的 [Obsolete] 標記自動解析
            /// </summary>
            Obsolete,

            /// <summary>
            /// 由手動維護的補充表提供 (無法以 [Obsolete] 轉接的變更，如具名參數、序列化欄位、已完全移除的成員)
            /// </summary>
            Manual
        }

        /// <summary>
        /// 比對方式 (決定該規則需要何種型別上下文才成立)
        /// </summary>
        public enum MatchKind
        {
            /// <summary>
            /// 需帶宣告型別前綴 (靜態成員與型別限定存取)，例如 AssetLoaders.OldApi
            /// </summary>
            Qualified,

            /// <summary>
            /// 僅於衍生自宣告型別的使用者類別中比對 (虛擬、抽象與保護成員)
            /// </summary>
            InheritedMember,

            /// <summary>
            /// 成員存取形式 (.OldApi)，適用於名稱具辨識度的公開實例成員
            /// </summary>
            MemberAccess,

            /// <summary>
            /// 具名參數形式 (oldApi:)，因無法驗證被呼叫的方法屬於框架，一律需人工複核
            /// </summary>
            NamedArgument,

            /// <summary>
            /// 型別名稱
            /// </summary>
            TypeName,

            /// <summary>
            /// 無法由文字分析安全處理，僅列入需人工遷移的報告
            /// </summary>
            ManualOnly
        }

        /// <summary>
        /// 舊 API 名稱
        /// </summary>
        public string oldApi;

        /// <summary>
        /// 新 API 名稱 (歧義規則為顯示用的候選列表，實際套用值由匹配項的決議結果決定)
        /// </summary>
        public string newApi;

        /// <summary>
        /// 宣告型別的簡短名稱 (Qualified 與 InheritedMember 必須提供)
        /// </summary>
        public string declaringTypeName;

        /// <summary>
        /// 解析到的宣告型別 (供衍生關係驗證使用)
        /// </summary>
        public Type declaringType;

        /// <summary>
        /// 可作為靜態成員前綴的型別名稱 (宣告型別本身與其所有衍生型別)
        /// </summary>
        public string[] qualifierNames;

        /// <summary>
        /// 比對方式
        /// </summary>
        public MatchKind matchKind;

        /// <summary>
        /// 規則來源類型
        /// </summary>
        public SourceType sourceType;

        /// <summary>
        /// 規則說明 (顯示於預覽視窗)
        /// </summary>
        public string description;

        /// <summary>
        /// 變更起始的框架版本 (選填，僅供顯示與追溯)
        /// </summary>
        public string sinceVersion;

        /// <summary>
        /// 是否為歧義規則 (同一舊 API 因多載而對應多個新 API)
        /// <para>掃描時會依呼叫引數嘗試自動決議，無法決議者需由使用者於下拉選單指定新 API。</para>
        /// </summary>
        public bool isAmbiguous;

        /// <summary>
        /// 歧義規則的新 API 候選列表 (非歧義規則為 null)
        /// </summary>
        public List<APIUpdateCandidate> candidates;

        /// <summary>
        /// 是否為常見名稱 (Unity 魔術方法或通用命名，僅在具備型別上下文時才可比對)
        /// </summary>
        public bool isCommonName;

        /// <summary>
        /// 該規則的匹配是否一律需人工複核 (例如具名參數規則)
        /// </summary>
        public bool alwaysNeedsReview;

        /// <summary>
        /// 一律需人工複核的原因說明
        /// </summary>
        public string reviewNote;

        /// <summary>
        /// 緩存比對用的表達式
        /// </summary>
        private Regex _regex;

        /// <summary>
        /// 表達式的逾時保護 (避免異常內容導致回溯爆炸)
        /// </summary>
        private static readonly TimeSpan _REGEX_TIMEOUT = TimeSpan.FromSeconds(2.0);

        public APIUpdateRule(string oldApi, string newApi, MatchKind matchKind, SourceType sourceType, string declaringTypeName = null, string description = null, bool isAmbiguous = false, bool isCommonName = false)
        {
            this.oldApi = oldApi;
            this.newApi = newApi;
            this.matchKind = matchKind;
            this.sourceType = sourceType;
            this.declaringTypeName = declaringTypeName;
            this.description = description;
            this.isAmbiguous = isAmbiguous;
            this.isCommonName = isCommonName;
        }

        /// <summary>
        /// 取得比對用的表達式 (匹配結果一律僅涵蓋舊 API 名稱本身，前後綴以斷言處理)
        /// </summary>
        /// <returns></returns>
        public Regex GetRegex()
        {
            if (this._regex == null)
            {
                string escapedOldApi = Regex.Escape(this.oldApi);
                string pattern;
                switch (this.matchKind)
                {
                    case MatchKind.Qualified:
                        {
                            // 必須帶型別前綴, 例如 AssetLoaders.OldApi; 靜態成員亦可透過衍生型別名稱呼叫, 故一併納入
                            string[] qualifiers = (this.qualifierNames != null && this.qualifierNames.Length > 0) ?
                                this.qualifierNames :
                                new string[] { this.declaringTypeName };
                            string alternation = string.Join("|", qualifiers.Select(Regex.Escape));
                            // 容許泛型限定寫法, 例如 GSIManagerBase<MyManager>.OldApi
                            pattern = $@"(?<=\b(?:{alternation})(?:\s*<[^<>]*>)?\s*\.\s*){escapedOldApi}\b";
                        }
                        break;
                    case MatchKind.MemberAccess:
                        // 必須為成員存取形式, 例如 xxx.OldApi
                        pattern = $@"(?<=\.\s*){escapedOldApi}\b";
                        break;
                    case MatchKind.NamedArgument:
                        // 必須為具名參數形式, 例如 (oldApi: true) 或 (x, oldApi: true)
                        // 前方限定 ( 或 , 以排除三元運算子與 case 標籤等同為 [識別字:] 形式的語法
                        pattern = $@"(?<=[(,]\s*){escapedOldApi}(?=\s*:(?!:))";
                        break;
                    default:
                        // InheritedMember 與 TypeName 以識別字邊界比對 (前者另以型別上下文限制檔案範圍)
                        pattern = $@"\b{escapedOldApi}\b";
                        break;
                }
                this._regex = new Regex(pattern, RegexOptions.CultureInvariant, _REGEX_TIMEOUT);
            }
            return this._regex;
        }

        /// <summary>
        /// 取得規則的顯示名稱
        /// </summary>
        /// <returns></returns>
        public string GetDisplayName()
        {
            string prefix = string.IsNullOrEmpty(this.declaringTypeName) ? string.Empty : $"{this.declaringTypeName}.";
            return $"{prefix}{this.oldApi}  ->  {this.newApi}";
        }

        public override string ToString()
        {
            return this.GetDisplayName();
        }
    }

    /// <summary>
    /// API 更新匹配項
    /// </summary>
    public class APIUpdateMatch
    {
        /// <summary>
        /// 檔案絕對路徑
        /// </summary>
        public string filePath;

        /// <summary>
        /// 檔案的資源路徑 (用於在編輯器中開啟)
        /// </summary>
        public string assetPath;

        /// <summary>
        /// 所在行號 (1 起算)
        /// </summary>
        public int lineNumber;

        /// <summary>
        /// 匹配起始的字元索引
        /// </summary>
        public int startIndex;

        /// <summary>
        /// 匹配的字元長度
        /// </summary>
        public int length;

        /// <summary>
        /// 所在行的原文
        /// </summary>
        public string lineText;

        /// <summary>
        /// 是否位於註解或字串常值中
        /// </summary>
        public bool isInCommentOrString;

        /// <summary>
        /// 是否需要人工複核 (預設不勾選)
        /// </summary>
        public bool needsReview;

        /// <summary>
        /// 需要人工複核的原因
        /// </summary>
        public string reviewReason;

        /// <summary>
        /// 歧義規則依呼叫引數自動決議出的新 API (無法決議為 null)
        /// </summary>
        public string resolvedNewApi;

        /// <summary>
        /// 自動決議的說明 (顯示於提示)
        /// </summary>
        public string resolutionNote;

        /// <summary>
        /// 歧義規則由使用者於下拉選單指定的候選索引 (-1 表示尚未指定)
        /// </summary>
        public int chosenCandidateIndex = -1;

        /// <summary>
        /// 對應的規則
        /// </summary>
        public APIUpdateRule rule;

        /// <summary>
        /// 是否勾選套用
        /// </summary>
        public bool selected;

        /// <summary>
        /// 取得實際套用的新 API 名稱
        /// <para>非歧義規則直接取規則值；歧義規則依序取自動決議結果與使用者指定的候選，皆無則回傳 null (不可套用)。</para>
        /// </summary>
        /// <returns></returns>
        public string GetEffectiveNewApi()
        {
            if (this.rule == null)
                return null;

            if (!this.rule.isAmbiguous)
                return this.rule.newApi;

            if (!string.IsNullOrEmpty(this.resolvedNewApi))
                return this.resolvedNewApi;

            if (this.rule.candidates != null &&
                this.chosenCandidateIndex >= 0 &&
                this.chosenCandidateIndex < this.rule.candidates.Count)
                return this.rule.candidates[this.chosenCandidateIndex].newApi;

            return null;
        }

        /// <summary>
        /// 是否已具備可套用的新 API 名稱
        /// </summary>
        /// <returns></returns>
        public bool CanApply()
        {
            return !string.IsNullOrEmpty(this.GetEffectiveNewApi());
        }
    }

    /// <summary>
    /// API 更新掃描結果
    /// </summary>
    public class APIUpdateScanResult
    {
        /// <summary>
        /// 框架版本號
        /// </summary>
        public string frameworkVersion;

        /// <summary>
        /// 框架源碼的根目錄 (掃描時排除)
        /// </summary>
        public string frameworkRoot;

        /// <summary>
        /// 本次可自動比對的規則列表
        /// </summary>
        public List<APIUpdateRule> rules = new List<APIUpdateRule>();

        /// <summary>
        /// 無法由文字分析安全處理的規則列表 (需人工遷移)
        /// </summary>
        public List<APIUpdateRule> manualOnlyRules = new List<APIUpdateRule>();

        /// <summary>
        /// 掃描到的匹配項
        /// </summary>
        public List<APIUpdateMatch> matches = new List<APIUpdateMatch>();

        /// <summary>
        /// 掃描的腳本數量
        /// </summary>
        public int scannedFileCount;

        /// <summary>
        /// 取得可直接套用的匹配數量 (無需人工複核)
        /// </summary>
        /// <returns></returns>
        public int GetReadyMatchCount()
        {
            return this.matches.Count(match => !match.needsReview);
        }

        /// <summary>
        /// 取得需要人工複核的匹配數量
        /// </summary>
        /// <returns></returns>
        public int GetReviewMatchCount()
        {
            return this.matches.Count(match => match.needsReview);
        }

        /// <summary>
        /// 取得勾選且可套用的匹配數量
        /// </summary>
        /// <returns></returns>
        public int GetSelectedCount()
        {
            return this.matches.Count(match => match.selected && match.CanApply());
        }
    }
}
