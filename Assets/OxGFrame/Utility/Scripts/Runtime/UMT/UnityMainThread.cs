using System;
using System.Collections.Generic;
using UnityEngine;

namespace OxGFrame.Utility.UMT
{
    internal class UnityMainThread : MonoBehaviour
    {
        internal static UnityMainThread worker;
        internal static readonly object threadLocker = new object();
        private Queue<Action> _jobs = new Queue<Action>();

        private void Awake()
        {
            this.gameObject.name = $"[{nameof(UnityMainThread)}]";
            worker = this;
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            while (this._jobs.Count > 0)
            {
                lock (threadLocker)
                {
                    this._jobs.Dequeue()?.Invoke();
                }
            }
        }

        internal void AddJob(Action newJob)
        {
            lock (threadLocker)
            {
                this._jobs.Enqueue(newJob);
            }
        }

        public void Clear()
        {
            this._jobs.Clear();
        }
    }
}

