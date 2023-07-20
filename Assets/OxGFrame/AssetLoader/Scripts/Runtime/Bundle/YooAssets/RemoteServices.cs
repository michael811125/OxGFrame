using System.IO;
using YooAsset;

namespace OxGFrame.AssetLoader.Bundle
{
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
}
