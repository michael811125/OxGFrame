using OxGKit.LoggingSystem;
using System;
using System.Collections.Generic;

namespace OxGFrame.GSIFrame
{
    public abstract class GSIManagerBase<T> where T : GSIManagerBase<T>, new()
    {
        /// <summary>
        /// GameStage 緩存
        /// </summary>
        protected Dictionary<int, GSIBase> _dictGameStage = null;

        /// <summary>
        /// 用於記錄 Incoming GameStageId (新的 GameStageId)
        /// </summary>
        protected int _incomingId = 0;

        /// <summary>
        /// 記錄當前的 GameStageId
        /// </summary>
        protected int _currentId { get; private set; }

        /// <summary>
        /// 當前 GameStage
        /// </summary>
        protected GSIBase _currentGameStage { get; private set; }

        private static readonly object _locker = new object();
        private static T _instance = null;
        protected static T GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                        _instance = new T();
                }
            }
            return _instance;
        }

        #region Default API
        public static int GetCurrentId()
        {
            return GetInstance()._currentId;
        }

        public static U GetStage<U>() where U : GSIBase
        {
            return GetInstance().GetGameStage<U>();
        }

        public static U GetStage<U>(int id) where U : GSIBase
        {
            return GetInstance().GetGameStage<U>(id);
        }

        public static void AddStage<U>() where U : GSIBase, new()
        {
            GetInstance().AddGameStage<U>();
        }

        public static void AddStage<U>(int id) where U : GSIBase, new()
        {
            GetInstance().AddGameStage<U>(id);
        }

        public static void AddStage(int id, GSIBase gameStage)
        {
            GetInstance().AddGameStage(id, gameStage);
        }

        public static void DeleteStage<U>() where U : GSIBase
        {
            GetInstance().DeleteGameStage<U>();
        }

        public static void DeleteStage(int id)
        {
            GetInstance().DeleteGameStage(id);
        }

        public static void ChangeStage<U>(bool force = false) where U : GSIBase
        {
            if (force)
                GetInstance().ChangeGameStageForce<U>();
            else
                GetInstance().ChangeGameStage<U>();
        }

        public static void ChangeStage(int id, bool force = false)
        {
            if (force)
                GetInstance().ChangeGameStageForce(id);
            else
                GetInstance().ChangeGameStage(id);
        }

        /// <summary>
        /// Call by main MonoBehaviour (Start)
        /// </summary>
        public static void DriveStart()
        {
            GetInstance().OnStart();
        }

        /// <summary>
        /// Call by main MonoBehaviour (Update)
        /// </summary>
        /// <param name="dt"></param>
        public static void DriveUpdate(float dt = 0.0f)
        {
            GetInstance().OnUpdate(dt);
        }

        [Obsolete("Use DriveStart instead.")]
        public static void Start()
        {
            GetInstance().OnStart();
        }

        [Obsolete("Use DriveUpdate instead.")]
        public static void Update(float dt = 0.0f)
        {
            GetInstance().OnUpdate(dt);
        }
        #endregion

        public GSIManagerBase()
        {
            this._dictGameStage = new Dictionary<int, GSIBase>();
            this._currentId = 0;
            this._currentGameStage = null;
        }

        /// <summary>
        /// 取得當前 GameStage Id
        /// </summary>
        /// <returns></returns>
        public int GetCurrentGameStageId()
        {
            return this._currentId;
        }

        /// <summary>
        /// 取得 GameStage
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public U GetGameStage<U>() where U : GSIBase
        {
            System.Type gameStageType = typeof(U);
            int hashCode = gameStageType.GetHashCode();

            return this.GetGameStage<U>(hashCode);
        }

        /// <summary>
        /// 取得 GameStage
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public U GetGameStage<U>(int id) where U : GSIBase
        {
            if (!this._dictGameStage.ContainsKey(id))
                return null;
            return (U)this._dictGameStage[id];
        }

        /// <summary>
        /// 註冊 GameStage
        /// </summary>
        /// <typeparam name="U"></typeparam>
        public void AddGameStage<U>() where U : GSIBase, new()
        {
            System.Type gameStageType = typeof(U);
            int hashCode = gameStageType.GetHashCode();

            U gameStage = new U();

            this.AddGameStage(hashCode, gameStage);
        }

        /// <summary>
        /// 註冊 GameStage
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="id"></param>
        public void AddGameStage<U>(int id) where U : GSIBase, new()
        {
            U gameStage = new U();

            this.AddGameStage(id, gameStage);
        }

        /// <summary>
        /// 註冊 GameStage
        /// </summary>
        /// <param name="id"></param>
        /// <param name="gameStage"></param>
        public void AddGameStage(int id, GSIBase gameStage)
        {
            if (this._dictGameStage.ContainsKey(id))
            {
                Logging.PrintWarning<Logger>($"Failed to add GameStage '{gameStage?.GetType().Name}': a GameStage with the same ID already exists in the cache.");
                return;
            }

            gameStage.SetId(id);
            this._dictGameStage.Add(id, gameStage);
        }

        /// <summary>
        /// 刪除 GameStage
        /// </summary>
        /// <typeparam name="U"></typeparam>
        public void DeleteGameStage<U>() where U : GSIBase
        {
            System.Type gameStageType = typeof(U);
            int hashCode = gameStageType.GetHashCode();

            this.DeleteGameStage(hashCode);
        }

        /// <summary>
        /// 刪除 GameStage
        /// </summary>
        /// <param name="id"></param>
        public void DeleteGameStage(int id)
        {
            if (this._dictGameStage.ContainsKey(id))
                this._dictGameStage.Remove(id);
        }

        /// <summary>
        /// 切換 GameStage (下一幀數才切換, 不允許切換至同階段)
        /// </summary>
        /// <typeparam name="U"></typeparam>
        public void ChangeGameStage<U>() where U : GSIBase
        {
            System.Type gameStageType = typeof(U);
            int hashCode = gameStageType.GetHashCode();

            this.ChangeGameStage(hashCode);
        }

        /// <summary>
        /// 切換 GameStage (下一幀數才切換, 不允許切換至同階段)
        /// </summary>
        /// <param name="id"></param>
        public void ChangeGameStage(int id)
        {
            this._incomingId = id;

            var gameStage = this.GetGameStage<GSIBase>(id);
            if (this._incomingId == this._currentId)
                Logging.PrintWarning<Logger>($" 【>>>>>> Same GameStage (Change Failed - Try Force) <<<<<<】Id: {this._incomingId}, Stage: {gameStage?.GetType().Name}");
            else
                Logging.PrintInfo<Logger>($" 【>>>>>> Change GameStage <<<<<<】Id: {this._incomingId}, Stage: {gameStage?.GetType().Name}");
        }

        /// <summary>
        /// 立即強制切換 GameStage
        /// </summary>
        /// <typeparam name="U"></typeparam>
        public void ChangeGameStageForce<U>() where U : GSIBase
        {
            System.Type gameStageType = typeof(U);
            int hashCode = gameStageType.GetHashCode();

            this.ChangeGameStageForce(hashCode);
        }

        /// <summary>
        /// 立即強制切換 GameStage
        /// </summary>
        /// <param name="id"></param>
        public void ChangeGameStageForce(int id)
        {
            var gameStage = this.GetGameStage<GSIBase>(id);
            Logging.PrintInfo<Logger>($" 【>>>>>> Change GameStage Force <<<<<<】Id: {id}, Stage: {gameStage?.GetType().Name}");

            this.ReleaseGameStage();                 // 立即釋放原本的 GameStage
            this._currentId = this._incomingId = id; // 立即指定 currentId & incomingId = id
            this.InitGameStage();                    // 立即開始進行 GameStage 初始的處理過程
        }

        /// <summary>
        /// 【由 OnUpdate 調用】處理 GameStage 要 Update 的過程
        /// </summary>
        /// <param name="dt"></param>
        protected void UpdateGameStage(float dt = 0.0f)
        {
            // 非立即的切換 GameStage 的情況下會去判斷, 如果 currentId 與 incomingId 不同的話, 就會進入切換
            if (this._currentId != this._incomingId)
            {
                this.ReleaseGameStage();            // 釋放原本的 GameStage
                this._currentId = this._incomingId; // 將 incomingId 記錄至 currentId
                this.InitGameStage();               // 開始進行 GameStage 初始的處理過程
            }

            if (this._currentGameStage == null)
                return;

            if (this._currentGameStage.runUpdate)
                this._currentGameStage.OnUpdate(dt); // 開始刷新當前的 GameStage
        }

        /// <summary>
        /// 初始 GameStage
        /// </summary>
        protected void InitGameStage()
        {
            // 透過當前的 GameStageId 取出 GameStage, 並且指定至當前 GameStage 中
            this._currentGameStage = this.GetGameStage<GSIBase>(this._currentId);
            // 開始進行 GameStage 初始流程
            if (this._currentGameStage != null)
                this._currentGameStage.BeginInit().Forget();
            else
                Logging.PrintError<Logger>($"Failed to change stage: GameStage with Id {this._currentId} not found.");
        }

        /// <summary>
        /// 釋放 GameStage
        /// </summary>
        protected void ReleaseGameStage()
        {
            if (this._currentGameStage == null)
                return;
            this._currentGameStage.OnExit();
        }

        /// <summary>
        /// Call by Main MonoBehaviour (Start)
        /// </summary>
        public virtual void OnStart() { }

        /// <summary>
        /// Call by Main MonoBehaviour (Update)
        /// </summary>
        /// <param name="dt"></param>
        public virtual void OnUpdate(float dt)
        {
            this.UpdateGameStage(dt);
        }
    }
}