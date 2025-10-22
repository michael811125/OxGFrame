namespace OxGFrame.AssetLoader.Cacher
{
    /// <summary>
    /// 嘗試計數器
    /// </summary>
    public class RetryCounter
    {
        public int retryCount;
        public int maxRetryCount;

        public RetryCounter(int maxRetryCount)
        {
            this.retryCount = this.maxRetryCount = maxRetryCount;
        }

        public bool IsOutOfRetries()
        {
            return this.retryCount < 0;
        }

        public void DelRetryCount()
        {
            this.retryCount--;
        }
    }
}