using MyBox;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.AssetLoader.Bundle
{
    public static class PlayModeParametersDefine
    {
        /// <summary>
        /// [Boolean] 初始預設包裹
        /// </summary>
        public const string INITIALIZE_PRESET_PACKAGES = "INITIALIZE_PRESET_PACKAGES";

        /// <summary>
        /// [Boolean] 獲取遠端 App 版號文件
        /// </summary>
        public const string FETCH_APP_CONFIG_FROM_SERVER = "FETCH_APP_CONFIG_FROM_SERVER";

        /// <summary>
        /// [Boolean] 版號 PATCH 檢查規則
        /// </summary>
        public const string SEMANTIC_RULE_PATCH = "SEMANTIC_RULE_PATCH";

        /// <summary>
        /// [Boolean] 是否自動檢查與設置請求端點 (Host Server, Fallback Host Server)
        /// </summary>
        public const string AUTO_CONFIGURE_SERVER_ENDPOINTS = "AUTO_CONFIGURE_SERVER_ENDPOINTS";

        /// <summary>
        /// [Boolean] 是否創建 PresetPackages 下載器
        /// </summary>
        public const string CREATE_PRESET_PACKAGES_DOWNLOADER = "CREATE_PRESET_PACKAGES_DOWNLOADER";

        /// <summary>
        /// [Boolean] 是否檢查硬盤空間 (當創建 PresetPackages 下載器時, 會進行檢查)
        /// </summary>
        public const string ENABLE_DISK_SPACE_CHECK_FOR_PRESET_PACKAGES_DOWNLOADER = "ENABLE_DISK_SPACE_CHECK_FOR_PRESET_PACKAGES_DOWNLOADER";

        /// <summary>
        /// [Boolean] 是否檢查本地最後版本 (用於弱聯網環境)
        /// </summary>
        public const string ENABLE_LAST_LOCAL_VERSIONS_CHECK_IN_WEAK_NETWORK = "ENABLE_LAST_LOCAL_VERSIONS_CHECK_IN_WEAK_NETWORK";
    }

    public abstract class PlayModeParameters
    {
        /// <summary>
        /// 版號控管
        /// </summary>
        [Serializable]
        public class SemanticRule
        {
            [ReadOnly]
            [SerializeField]
            private bool _major = true;
            public bool major => this._major;

            [ReadOnly]
            [SerializeField]
            private bool _minor = true;
            public bool minor => this._minor;

            [SerializeField]
            [Tooltip("Compare MAJOR.MINOR.PATCH when enabled; otherwise MAJOR.MINOR.")]
            private bool _patch = false;
            public bool patch => this._patch;

            public void SetPatch(bool active)
            {
                this._patch = active;
            }
        }

        public bool initializePresetPackages;

        [Tooltip("Fetch the app config from the server to compare the app version; otherwise, fetch it from StreamingAssets.")]
        public bool fetchAppConfigFromServer;

        [Tooltip("If checked, the patch field will compare whole version."), ConditionalField(nameof(fetchAppConfigFromServer))]
        public SemanticRule semanticRule = new SemanticRule();

        [Tooltip("Auto-check & set Primary/Fallback Server URLs during package initialization.")]
        public bool autoConfigureServerEndpoints;

        [Tooltip("If unchecked, skips creating the preset-packages downloader; assets will be downloaded on demand while playing.")]
        public bool createPresetPackagesDownloader;

        [Tooltip("If checked, will check disk space is it enough while preset-packages downloader checking. (Not supported on WebGL)")]
        public bool enableDiskSpaceCheckForPresetPackagesDownloader;

        [Tooltip("If checked, will check the last locally stored versions. (Applies to WeakHostMode only)")]
        public bool enableLastLocalVersionsCheckInWeakNetwork;

        #region YooAsset
        [Header("YooAsset Settings")]
        /// <summary>
        /// YooAsset Parameter 參數列表
        /// </summary>
        [Tooltip("[YooAsset] Runtime parameters for YooAsset. (Custom Mode is not supported!)")]
        public List<ParameterEntry> parameterEntries = new List<ParameterEntry>();
        #endregion

        public PlayModeParameters()
        {
            this.SetDefaultParameters();
        }

        /// <summary>
        /// 添加自定義參數
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetParameter(string name, object value)
        {
            switch (name)
            {
                case PlayModeParametersDefine.INITIALIZE_PRESET_PACKAGES:
                    this.initializePresetPackages = Convert.ToBoolean(value);
                    break;

                case PlayModeParametersDefine.FETCH_APP_CONFIG_FROM_SERVER:
                    this.fetchAppConfigFromServer = Convert.ToBoolean(value);
                    break;

                case PlayModeParametersDefine.SEMANTIC_RULE_PATCH:
                    this.semanticRule.SetPatch(Convert.ToBoolean(value));
                    break;

                case PlayModeParametersDefine.AUTO_CONFIGURE_SERVER_ENDPOINTS:
                    this.autoConfigureServerEndpoints = Convert.ToBoolean(value);
                    break;

                case PlayModeParametersDefine.CREATE_PRESET_PACKAGES_DOWNLOADER:
                    this.createPresetPackagesDownloader = Convert.ToBoolean(value);
                    break;

                case PlayModeParametersDefine.ENABLE_DISK_SPACE_CHECK_FOR_PRESET_PACKAGES_DOWNLOADER:
                    this.enableDiskSpaceCheckForPresetPackagesDownloader = Convert.ToBoolean(value);
                    break;

                case PlayModeParametersDefine.ENABLE_LAST_LOCAL_VERSIONS_CHECK_IN_WEAK_NETWORK:
                    this.enableLastLocalVersionsCheckInWeakNetwork = Convert.ToBoolean(value);
                    break;
            }
        }

        /// <summary>
        /// 預設參數
        /// </summary>
        public abstract void SetDefaultParameters();
    }

    /// <summary>
    /// 開發模擬模式
    /// </summary>
    [Serializable]
    public class EditorSimulateModeParameters : PlayModeParameters
    {
        public EditorSimulateModeParameters() : base() { }

        public override void SetDefaultParameters()
        {
            this.SetParameter(PlayModeParametersDefine.INITIALIZE_PRESET_PACKAGES, true);
            this.SetParameter(PlayModeParametersDefine.FETCH_APP_CONFIG_FROM_SERVER, false);
            this.SetParameter(PlayModeParametersDefine.SEMANTIC_RULE_PATCH, false);
            this.SetParameter(PlayModeParametersDefine.AUTO_CONFIGURE_SERVER_ENDPOINTS, false);
            this.SetParameter(PlayModeParametersDefine.CREATE_PRESET_PACKAGES_DOWNLOADER, false);
            this.SetParameter(PlayModeParametersDefine.ENABLE_DISK_SPACE_CHECK_FOR_PRESET_PACKAGES_DOWNLOADER, false);
            this.SetParameter(PlayModeParametersDefine.ENABLE_LAST_LOCAL_VERSIONS_CHECK_IN_WEAK_NETWORK, false);
        }
    }

    /// <summary>
    /// 離線模式
    /// </summary>
    [Serializable]
    public class OfflineModeParameters : PlayModeParameters
    {
        public OfflineModeParameters() : base() { }

        public override void SetDefaultParameters()
        {
            this.SetParameter(PlayModeParametersDefine.INITIALIZE_PRESET_PACKAGES, true);
            this.SetParameter(PlayModeParametersDefine.FETCH_APP_CONFIG_FROM_SERVER, false);
            this.SetParameter(PlayModeParametersDefine.SEMANTIC_RULE_PATCH, false);
            this.SetParameter(PlayModeParametersDefine.AUTO_CONFIGURE_SERVER_ENDPOINTS, false);
            this.SetParameter(PlayModeParametersDefine.CREATE_PRESET_PACKAGES_DOWNLOADER, false);
            this.SetParameter(PlayModeParametersDefine.ENABLE_DISK_SPACE_CHECK_FOR_PRESET_PACKAGES_DOWNLOADER, false);
            this.SetParameter(PlayModeParametersDefine.ENABLE_LAST_LOCAL_VERSIONS_CHECK_IN_WEAK_NETWORK, false);
        }
    }

    /// <summary>
    /// 聯網模式
    /// </summary>
    [Serializable]
    public class HostModeParameters : PlayModeParameters
    {
        public HostModeParameters() : base() { }

        public override void SetDefaultParameters()
        {
            this.SetParameter(PlayModeParametersDefine.INITIALIZE_PRESET_PACKAGES, true);
            this.SetParameter(PlayModeParametersDefine.FETCH_APP_CONFIG_FROM_SERVER, true);
            this.SetParameter(PlayModeParametersDefine.SEMANTIC_RULE_PATCH, false);
            this.SetParameter(PlayModeParametersDefine.AUTO_CONFIGURE_SERVER_ENDPOINTS, true);
            this.SetParameter(PlayModeParametersDefine.CREATE_PRESET_PACKAGES_DOWNLOADER, true);
            this.SetParameter(PlayModeParametersDefine.ENABLE_DISK_SPACE_CHECK_FOR_PRESET_PACKAGES_DOWNLOADER, true);
            this.SetParameter(PlayModeParametersDefine.ENABLE_LAST_LOCAL_VERSIONS_CHECK_IN_WEAK_NETWORK, false);
        }
    }

    /// <summary>
    /// 弱聯網模式
    /// </summary>
    [Serializable]
    public class WeakHostModeParameters : PlayModeParameters
    {
        public WeakHostModeParameters() : base() { }

        public override void SetDefaultParameters()
        {
            this.SetParameter(PlayModeParametersDefine.INITIALIZE_PRESET_PACKAGES, true);
            this.SetParameter(PlayModeParametersDefine.FETCH_APP_CONFIG_FROM_SERVER, true);
            this.SetParameter(PlayModeParametersDefine.SEMANTIC_RULE_PATCH, false);
            this.SetParameter(PlayModeParametersDefine.AUTO_CONFIGURE_SERVER_ENDPOINTS, true);
            this.SetParameter(PlayModeParametersDefine.CREATE_PRESET_PACKAGES_DOWNLOADER, true);
            this.SetParameter(PlayModeParametersDefine.ENABLE_DISK_SPACE_CHECK_FOR_PRESET_PACKAGES_DOWNLOADER, true);
            this.SetParameter(PlayModeParametersDefine.ENABLE_LAST_LOCAL_VERSIONS_CHECK_IN_WEAK_NETWORK, true);
        }
    }

    /// <summary>
    /// WebGL 模式
    /// </summary>
    [Serializable]
    public class WebGLModeParameters : PlayModeParameters
    {
        public WebGLModeParameters() : base() { }

        public override void SetDefaultParameters()
        {
            this.SetParameter(PlayModeParametersDefine.INITIALIZE_PRESET_PACKAGES, true);
            this.SetParameter(PlayModeParametersDefine.FETCH_APP_CONFIG_FROM_SERVER, false);
            this.SetParameter(PlayModeParametersDefine.SEMANTIC_RULE_PATCH, false);
            this.SetParameter(PlayModeParametersDefine.AUTO_CONFIGURE_SERVER_ENDPOINTS, false);
            this.SetParameter(PlayModeParametersDefine.CREATE_PRESET_PACKAGES_DOWNLOADER, false);
            this.SetParameter(PlayModeParametersDefine.ENABLE_DISK_SPACE_CHECK_FOR_PRESET_PACKAGES_DOWNLOADER, false);
            this.SetParameter(PlayModeParametersDefine.ENABLE_LAST_LOCAL_VERSIONS_CHECK_IN_WEAK_NETWORK, false);
        }
    }

    /// <summary>
    /// WebGL 遠端模式
    /// </summary>
    [Serializable]
    public class WebGLRemoteModeParameters : PlayModeParameters
    {
        public WebGLRemoteModeParameters() : base() { }

        public override void SetDefaultParameters()
        {
            this.SetParameter(PlayModeParametersDefine.INITIALIZE_PRESET_PACKAGES, true);
            this.SetParameter(PlayModeParametersDefine.FETCH_APP_CONFIG_FROM_SERVER, true);
            this.SetParameter(PlayModeParametersDefine.SEMANTIC_RULE_PATCH, false);
            this.SetParameter(PlayModeParametersDefine.AUTO_CONFIGURE_SERVER_ENDPOINTS, true);
            this.SetParameter(PlayModeParametersDefine.CREATE_PRESET_PACKAGES_DOWNLOADER, true);
            this.SetParameter(PlayModeParametersDefine.ENABLE_DISK_SPACE_CHECK_FOR_PRESET_PACKAGES_DOWNLOADER, false);
            this.SetParameter(PlayModeParametersDefine.ENABLE_LAST_LOCAL_VERSIONS_CHECK_IN_WEAK_NETWORK, false);
        }
    }

    /// <summary>
    /// 自定義模式
    /// </summary>
    [Serializable]
    public class CustomModeParameters : PlayModeParameters
    {
        public CustomModeParameters() : base() { }

        public override void SetDefaultParameters()
        {
            this.SetParameter(PlayModeParametersDefine.INITIALIZE_PRESET_PACKAGES, false);
            this.SetParameter(PlayModeParametersDefine.FETCH_APP_CONFIG_FROM_SERVER, true);
            this.SetParameter(PlayModeParametersDefine.SEMANTIC_RULE_PATCH, false);
            this.SetParameter(PlayModeParametersDefine.AUTO_CONFIGURE_SERVER_ENDPOINTS, true);
            this.SetParameter(PlayModeParametersDefine.CREATE_PRESET_PACKAGES_DOWNLOADER, true);
            this.SetParameter(PlayModeParametersDefine.ENABLE_DISK_SPACE_CHECK_FOR_PRESET_PACKAGES_DOWNLOADER, true);
            this.SetParameter(PlayModeParametersDefine.ENABLE_LAST_LOCAL_VERSIONS_CHECK_IN_WEAK_NETWORK, false);
        }
    }
}
