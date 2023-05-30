namespace OxGFrame.Utility.Singleton
{
    public class NewSingleton<T> where T : class, new()
    {
        private static readonly object _locker = new object();
        private static T _instance = null;
        public static T GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = new T();
                }
            }
            return _instance;
        }
    }
}
