using System.Collections.Generic;
using UnityEngine;
using MyBox;

namespace OxGFrame.CoreFrame.Utility
{
    [AddComponentMenu("OxGFrame/Utility/Pool/NodePool")]
    public class NodePool : MonoBehaviour
    {
        [SerializeField, Tooltip("物件池要生成的物件")]
        public GameObject go = null;
        [SerializeField, Tooltip("物件池初始大小")]
        public int initSize = 0;
        [SerializeField, Tooltip("物件池不夠時, 是否自動新增")]
        public bool autoPut = false;
        [SerializeField, ConditionalField(nameof(autoPut)), Tooltip("每次自動新增的數量")]
        public int autoPutSize = 0;
        private Queue<GameObject> _pool;

        private void Awake()
        {
            this._pool = new Queue<GameObject>();
        }

        private void Start()
        {
            this._Init();
        }

        private void _Init()
        {
            for (int i = 0; i < this.initSize; i++)
            {
                GameObject instGo = Instantiate(this.go, Vector3.zero, Quaternion.identity, this.transform);
                this.Put(instGo);
            }
        }

        private void _AutoPut()
        {
            for (int i = 0; i < this.autoPutSize; i++)
            {
                GameObject instGo = Instantiate(this.go, Vector3.zero, Quaternion.identity, this.transform);
                this.Put(instGo);
            }
        }

        /// <summary>
        /// 物件池大小
        /// </summary>
        /// <returns></returns>
        public int Size()
        {
            return this._pool.Count;
        }

        /// <summary>
        /// 清空物件池
        /// </summary>
        public void Clear()
        {
            this._pool.ForEach(go =>
            {
                Destroy(go);
            });
            this._pool.Clear();
        }

        /// <summary>
        /// 回收物件
        /// </summary>
        /// <param name="go"></param>
        public void Put(GameObject go)
        {
            if (go)
            {
                go.transform.SetParent(this.transform);
                go.SetActive(false);
                this._pool.Enqueue(go);
            }
        }

        /// <summary>
        /// 取出物件
        /// </summary>
        /// <returns></returns>
        public GameObject Get()
        {
            if (this._pool.Count == 0)
            {
                if (this.autoPut && this.autoPutSize > 0) this._AutoPut();
                else return null;
            }

            GameObject go = this._pool.Dequeue();
            go.transform.SetParent(null);
            go.SetActive(true);

            return go;
        }

        /// <summary>
        /// 取出物件
        /// </summary>
        /// <returns></returns>
        public GameObject Get(Transform parent)
        {
            if (this._pool.Count == 0)
            {
                if (this.autoPut && this.autoPutSize > 0) this._AutoPut();
                else return null;
            }

            GameObject go = this._pool.Dequeue();
            go.transform.SetParent(parent);
            go.SetActive(true);

            return go;
        }

        /// <summary>
        /// 取出物件
        /// </summary>
        /// <returns></returns>
        public GameObject Get(Transform parent, Vector3 position)
        {
            if (this._pool.Count == 0)
            {
                if (this.autoPut && this.autoPutSize > 0) this._AutoPut();
                else return null;
            }

            GameObject go = this._pool.Dequeue();
            go.transform.SetParent(parent);
            go.transform.localPosition = position;
            go.SetActive(true);

            return go;
        }

        /// <summary>
        /// 取出物件
        /// </summary>
        /// <returns></returns>
        public GameObject Get(Transform parent, Vector3 position, Quaternion rotation)
        {
            if (this._pool.Count == 0)
            {
                if (this.autoPut && this.autoPutSize > 0) this._AutoPut();
                else return null;
            }

            GameObject go = this._pool.Dequeue();
            go.transform.SetParent(parent);
            go.transform.localPosition = position;
            go.transform.rotation = rotation;
            go.SetActive(true);

            return go;
        }
    }
}