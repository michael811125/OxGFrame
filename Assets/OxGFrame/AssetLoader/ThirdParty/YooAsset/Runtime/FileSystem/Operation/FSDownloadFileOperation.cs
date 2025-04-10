
namespace YooAsset
{
    internal class DownloadFileOptions
    {
        /// <summary>
        /// 失败后重试次数
        /// </summary>
        public readonly int FailedTryAgain;

        /// <summary>
        /// 超时时间
        /// </summary>
        public readonly int Timeout;

        /// <summary>
        /// 主资源地址
        /// </summary>
        public string MainURL { set; get; }

        /// <summary>
        /// 备用资源地址
        /// </summary>
        public string FallbackURL { set; get; }

        /// <summary>
        /// 导入的本地文件路径
        /// </summary>
        public string ImportFilePath { set; get; }

        public DownloadFileOptions(int failedTryAgain, int timeout)
        {
            FailedTryAgain = failedTryAgain;
            Timeout = timeout;
        }
    }

    internal abstract class FSDownloadFileOperation : AsyncOperationBase
    {
        public PackageBundle Bundle { private set; get; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; }

        /// <summary>
        /// HTTP返回码
        /// </summary>
        public long HttpCode { protected set; get; }

        /// <summary>
        /// 当前下载的字节数
        /// </summary>
        public long DownloadedBytes { protected set; get; }

        /// <summary>
        /// 当前下载进度（0f - 1f）
        /// </summary>
        public float DownloadProgress { protected set; get; }


        public FSDownloadFileOperation(PackageBundle bundle)
        {
            Bundle = bundle;
            RefCount = 0;
            HttpCode = 0;
            DownloadedBytes = 0;
            DownloadProgress = 0;
        }

        internal override string InternalGetDesc()
        {
            return $"RefCount : {RefCount}";
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        public virtual void Release()
        {
            RefCount--;
        }

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public virtual void Reference()
        {
            RefCount++;
        }
    }
}