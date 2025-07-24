
namespace YooAsset
{
    internal abstract class UnityDownloadFileOperation : UnityWebRequestOperation
    {
        protected enum ESteps
        {
            None,
            CreateRequest,
            Download,
            CopyLocalFile,
            VerifyFile,
            Done,
        }

        protected readonly DefaultCacheFileSystem _fileSystem;
        protected readonly PackageBundle _bundle;
        protected readonly string _tempFilePath;

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; }

        internal UnityDownloadFileOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle, string url) : base(url)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
            _tempFilePath = _fileSystem.GetTempFilePath(bundle);
        }
        internal override string InternalGetDesc()
        {
            return $"RefCount : {RefCount}";
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        public void Release()
        {
            RefCount--;
        }

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void Reference()
        {
            RefCount++;
        }
    }
}