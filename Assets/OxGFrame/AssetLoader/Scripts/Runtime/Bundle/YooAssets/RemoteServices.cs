#if YOOASSET_3
using System.Collections.Generic;
#endif
using System.IO;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
#if YOOASSET_2
    public class HostServers : IRemoteServices
    {
        private readonly string _hostServer;
        private readonly string _fallbackHostServer;

        public HostServers(string hostServer, string fallbackHostServer)
        {
            this._hostServer = hostServer;
            this._fallbackHostServer = fallbackHostServer;
        }

        public string GetRemoteMainURL(string fileName)
        {
            return Path.Combine(this._hostServer, fileName);
        }

        public string GetRemoteFallbackURL(string fileName)
        {
            return Path.Combine(this._fallbackHostServer, fileName);
        }
    }
#elif YOOASSET_3
    public class HostServers : IRemoteService
    {
        private readonly string _hostServer;
        private readonly string _fallbackHostServer;

        public HostServers(string hostServer, string fallbackHostServer)
        {
            this._hostServer = hostServer;
            this._fallbackHostServer = fallbackHostServer;
        }

        /// <summary>
        /// [YooAsset v3] 依優先序回傳遠端候選位址 (主位址 -> 備援位址)
        /// </summary>
        public IReadOnlyList<string> GetRemoteUrls(string fileName)
        {
            return new List<string>()
            {
                Path.Combine(this._hostServer, fileName),
                Path.Combine(this._fallbackHostServer, fileName)
            };
        }
    }
#endif
}
