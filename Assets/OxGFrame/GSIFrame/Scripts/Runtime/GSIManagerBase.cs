using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.GSIFrame
{
    public class GSIManagerBase<T> where T : GSIManagerBase<T>, new()
    {
        protected Dictionary<int, GSIBase> _dictGameStage = null; // GameStage 快取
        protected int _incomingId = 0;                            // 用於記錄 Incoming GameStageId (新的 GameStageId)
        protected int _currentId { get; private set; }            // 記錄當前的 GameStageId
        protected GSIBase _currentGameStage { get; private set; } // 當前 GameStage

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

        public GSIManagerBase()
        {
            this._dictGameStage = new Dictionary<int, GSIBase>();

            this._currentId = 0;
            this._currentGameStage = null;
        }

        ~GSIManagerBase()
        {
            this._dictGameStage.Clear();
            this._dictGameStage = null;

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
            if (!this._dictGameStage.ContainsKey(id)) return null;
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
#if UNITY_EDITOR
                Debug.Log($"Failed to add GameStage ({gameStage.GetType().Name}). Already has same GameStageId in cache");
#endif
                return;
            }

            this._dictGameStage.Add(id, gameStage);
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

#if UNITY_EDITOR
            var gameStage = this.GetGameStage<GSIBase>(id);
            if (this._incomingId == this._currentId)
                Debug.Log(string.Format("<color=#ff54ac> 【>>>>>> Same GameStage (Change Failed - <color=#ffb12a>Try Force</color>) <<<<<<】Id: {0}, Stage: {1}</color>", this._incomingId, gameStage?.GetType().Name));
            else
                Debug.Log(string.Format("<color=#00B8FF> 【>>>>>> Change GameStage <<<<<<】Id: {0}, Stage: {1}</color>", this._incomingId, gameStage?.GetType().Name));
#endif
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
#if UNITY_EDITOR
            var gameStage = this.GetGameStage<GSIBase>(id);
            Debug.Log(string.Format("<color=#00B8FF> 【>>>>>> Change GameStage <color=#ffb12a>Force</color> <<<<<<】Id: {0}, Stage: {1}</color>", id, gameStage?.GetType().Name));
#endif

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

            if (this._currentGameStage == null) return;

            if (this._currentGameStage.runUpdate) this._currentGameStage.OnUpdate(dt); // 開始刷新當前的 GameStage
        }

        /// <summary>
        /// 初始 GameStage
        /// </summary>
        protected void InitGameStage()
        {
            // 透過當前的 GameStageId 取出 GameStage, 並且指定至當前 GameStage 中
            this._currentGameStage = this.GetGameStage<GSIBase>(this._currentId);
            // 開始進行 GameStage 初始流程
            if (this._currentGameStage != null) this._currentGameStage.BeginInit().Forget();
#if UNITY_EDITOR
            else Debug.Log(string.Format("Cannot found GameStage. Id: {0}", this._currentId));
#endif
        }

        /// <summary>
        /// 釋放 GameStage
        /// </summary>
        protected void ReleaseGameStage()
        {
            if (this._currentGameStage == null) return;
            this._currentGameStage.OnExit();
        }

        /// <summary>
        /// 子類實作, 並且透過主要的 MonoBehaviour Start 調用
        /// </summary>
        public virtual void OnStart() { }

        /// <summary>
        /// 子類實作, 並且透過主要的 MonoBehaviour Update 調用
        /// </summary>
        /// <param name="dt"></param>
        public virtual void OnUpdate(float dt = 0.0f)
        {
            this.UpdateGameStage(dt);
        }
    }
}