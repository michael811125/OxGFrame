using OxGKit.LoggingSystem;
using System;
using System.Collections.Generic;

namespace OxGFrame.GSIFrame
{
    /// <summary>
    /// Generic singleton manager that registers, switches, and drives game stages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GSIManagerBase<T> where T : GSIManagerBase<T>, new()
    {
        /// <summary>
        /// Cache of registered game stages, keyed by game stage id
        /// </summary>
        protected Dictionary<int, GSIBase> _dictGameStage = null;

        /// <summary>
        /// Record the incoming game stage id (the next game stage id to switch to)
        /// </summary>
        protected int _incomingId = 0;

        /// <summary>
        /// Record the current game stage id
        /// </summary>
        protected int _currentId { get; private set; }

        /// <summary>
        /// Current game stage instance
        /// </summary>
        protected GSIBase _currentGameStage { get; private set; }

        private static readonly object _locker = new object();
        private static T _instance = null;
        /// <summary>
        /// Get the singleton instance (created lazily with thread-safe double-checked locking)
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Get the current game stage id
        /// </summary>
        /// <returns></returns>
        public static int GetCurrentId()
        {
            return GetInstance()._currentId;
        }

        /// <summary>
        /// Get a registered game stage by its type (uses the type hash code as id)
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public static U GetStage<U>() where U : GSIBase
        {
            return GetInstance().GetGameStage<U>();
        }

        /// <summary>
        /// Get a registered game stage by its id
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static U GetStage<U>(int id) where U : GSIBase
        {
            return GetInstance().GetGameStage<U>(id);
        }

        /// <summary>
        /// Register a new game stage of type U (uses the type hash code as id)
        /// </summary>
        /// <typeparam name="U"></typeparam>
        public static void AddStage<U>() where U : GSIBase, new()
        {
            GetInstance().AddGameStage<U>();
        }

        /// <summary>
        /// Register a new game stage of type U with a specified id
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="id"></param>
        public static void AddStage<U>(int id) where U : GSIBase, new()
        {
            GetInstance().AddGameStage<U>(id);
        }

        /// <summary>
        /// Register a game stage instance with a specified id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="gameStage"></param>
        public static void AddStage(int id, GSIBase gameStage)
        {
            GetInstance().AddGameStage(id, gameStage);
        }

        /// <summary>
        /// Remove a registered game stage by its type (uses the type hash code as id)
        /// </summary>
        /// <typeparam name="U"></typeparam>
        public static void DeleteStage<U>() where U : GSIBase
        {
            GetInstance().DeleteGameStage<U>();
        }

        /// <summary>
        /// Remove a registered game stage by its id
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteStage(int id)
        {
            GetInstance().DeleteGameStage(id);
        }

        /// <summary>
        /// Switch to the game stage of type U on the next update
        /// When force is true, switches immediately (allows re-entering the same stage)
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="force"></param>
        public static void ChangeStage<U>(bool force = false) where U : GSIBase
        {
            if (force)
                GetInstance().ChangeGameStageForce<U>();
            else
                GetInstance().ChangeGameStage<U>();
        }

        /// <summary>
        /// Switch to the game stage with the specified id on the next update
        /// When force is true, switches immediately (allows re-entering the same stage)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="force"></param>
        public static void ChangeStage(int id, bool force = false)
        {
            if (force)
                GetInstance().ChangeGameStageForce(id);
            else
                GetInstance().ChangeGameStage(id);
        }

        /// <summary>
        /// Drive the manager start flow (call from the main MonoBehaviour Start)
        /// </summary>
        public static void DriveStart()
        {
            GetInstance().OnStart();
        }

        /// <summary>
        /// Drive the manager update flow (call from the main MonoBehaviour Update)
        /// </summary>
        /// <param name="dt"></param>
        public static void DriveUpdate(float dt = 0.0f)
        {
            GetInstance().OnUpdate(dt);
        }

        /// <summary>
        /// Obsolete. Use DriveStart instead.
        /// </summary>
        [Obsolete("Use DriveStart instead.")]
        public static void Start()
        {
            GetInstance().OnStart();
        }

        /// <summary>
        /// Obsolete. Use DriveUpdate instead.
        /// </summary>
        /// <param name="dt"></param>
        [Obsolete("Use DriveUpdate instead.")]
        public static void Update(float dt = 0.0f)
        {
            GetInstance().OnUpdate(dt);
        }
        #endregion

        /// <summary>
        /// Initialize the game stage cache and reset the current stage state
        /// </summary>
        public GSIManagerBase()
        {
            this._dictGameStage = new Dictionary<int, GSIBase>();
            this._currentId = 0;
            this._currentGameStage = null;
        }

        /// <summary>
        /// Get the current game stage id
        /// </summary>
        /// <returns></returns>
        public int GetCurrentGameStageId()
        {
            return this._currentId;
        }

        /// <summary>
        /// Get a registered game stage by its type (uses the type hash code as id)
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
        /// Get a registered game stage by its id (returns null if not found)
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
        /// Register a new game stage of type U (uses the type hash code as id)
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
        /// Register a new game stage of type U with a specified id
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="id"></param>
        public void AddGameStage<U>(int id) where U : GSIBase, new()
        {
            U gameStage = new U();

            this.AddGameStage(id, gameStage);
        }

        /// <summary>
        /// Register a game stage instance with a specified id (null instances and duplicate ids are rejected)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="gameStage"></param>
        public void AddGameStage(int id, GSIBase gameStage)
        {
            // 空實例防護
            if (gameStage == null)
            {
                Logging.PrintError<Logger>($"Failed to add GameStage with Id '{id}': the GameStage instance is null.");
                return;
            }

            if (this._dictGameStage.ContainsKey(id))
            {
                Logging.PrintWarning<Logger>($"Failed to add GameStage '{gameStage.GetType().Name}': a GameStage with the same ID already exists in the cache.");
                return;
            }

            gameStage.SetId(id);
            this._dictGameStage.Add(id, gameStage);
        }

        /// <summary>
        /// Remove a registered game stage by its type (uses the type hash code as id)
        /// </summary>
        /// <typeparam name="U"></typeparam>
        public void DeleteGameStage<U>() where U : GSIBase
        {
            System.Type gameStageType = typeof(U);
            int hashCode = gameStageType.GetHashCode();

            this.DeleteGameStage(hashCode);
        }

        /// <summary>
        /// Remove a registered game stage by its id (the current running game stage cannot be deleted)
        /// </summary>
        /// <param name="id"></param>
        public void DeleteGameStage(int id)
        {
            // 防止刪除運行中的 GameStage，避免 OnExit 未被調用且當前參考懸空
            if (id == this._currentId &&
                this._currentGameStage != null)
            {
                Logging.PrintWarning<Logger>($"Failed to delete GameStage with Id '{id}': cannot delete the current running GameStage.");
                return;
            }

            if (this._dictGameStage.ContainsKey(id))
                this._dictGameStage.Remove(id);
        }

        /// <summary>
        /// Switch to the game stage of type U (takes effect on the next update; switching to the same stage is not allowed)
        /// </summary>
        /// <typeparam name="U"></typeparam>
        public void ChangeGameStage<U>() where U : GSIBase
        {
            System.Type gameStageType = typeof(U);
            int hashCode = gameStageType.GetHashCode();

            this.ChangeGameStage(hashCode);
        }

        /// <summary>
        /// Switch to the game stage with the specified id (takes effect on the next update; switching to the same stage is not allowed)
        /// </summary>
        /// <param name="id"></param>
        public void ChangeGameStage(int id)
        {
            // 先驗證 GameStage 是否存在，再設置 _incomingId，避免無效 Id 進入切換流程
            var gameStage = this.GetGameStage<GSIBase>(id);
            if (gameStage == null)
            {
                Logging.PrintError<Logger>($" 【>>>>>> Change GameStage Failed <<<<<<】Cannot find GameStage with Id: {id}");
                return;
            }

            this._incomingId = id;

            if (this._incomingId == this._currentId)
                Logging.PrintWarning<Logger>($" 【>>>>>> Same GameStage (Change Failed - Try Force) <<<<<<】Id: {this._incomingId}, Stage: {gameStage.GetType().Name}");
            else
                Logging.PrintInfo<Logger>($" 【>>>>>> Change GameStage <<<<<<】Id: {this._incomingId}, Stage: {gameStage.GetType().Name}");
        }

        /// <summary>
        /// Force an immediate switch to the game stage of type U (allows re-entering the same stage)
        /// </summary>
        /// <typeparam name="U"></typeparam>
        public void ChangeGameStageForce<U>() where U : GSIBase
        {
            System.Type gameStageType = typeof(U);
            int hashCode = gameStageType.GetHashCode();

            this.ChangeGameStageForce(hashCode);
        }

        /// <summary>
        /// Force an immediate switch to the game stage with the specified id (allows re-entering the same stage)
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
        /// Invoked by OnUpdate to perform any pending stage switch and update the current game stage
        /// </summary>
        /// <param name="dt"></param>
        protected void UpdateGameStage(float dt = 0.0f)
        {
            // 非立即的切換 GameStage 的情況下會去判斷，如果 currentId 與 incomingId 不同的話，就會進入切換
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
        /// Initialize the current game stage (takes the stage by the current id and begins its init flow)
        /// </summary>
        protected void InitGameStage()
        {
            // 透過當前的 GameStageId 取出 GameStage，並且指定至當前 GameStage 中
            this._currentGameStage = this.GetGameStage<GSIBase>(this._currentId);
            // 開始進行 GameStage 初始流程
            if (this._currentGameStage != null)
                this._currentGameStage.BeginInit().Forget();
            else
                Logging.PrintError<Logger>($"Failed to change stage: GameStage with Id {this._currentId} not found.");
        }

        /// <summary>
        /// Release the current game stage by invoking its OnExit
        /// </summary>
        protected void ReleaseGameStage()
        {
            if (this._currentGameStage == null)
                return;
            this._currentGameStage.OnExit();
        }

        /// <summary>
        /// Invoked by DriveStart when the main MonoBehaviour starts
        /// </summary>
        public virtual void OnStart() { }

        /// <summary>
        /// Invoked by DriveUpdate every frame to process the game stage update flow
        /// </summary>
        /// <param name="dt"></param>
        public virtual void OnUpdate(float dt)
        {
            this.UpdateGameStage(dt);
        }
    }
}