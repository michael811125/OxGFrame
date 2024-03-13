using OxGFrame.MediaFrame.AudioFrame;
using OxGFrame.MediaFrame.VideoFrame;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OxGFrame.MediaFrame.Cacher
{
    internal class MediaLRUCache
    {
        private readonly int _capacity;
        private readonly ConcurrentDictionary<string, LinkedListNode<CacheItem>> _cache;
        private readonly LinkedList<CacheItem> _lruList;

        public int Count => this._cache.Count;

        public MediaLRUCache(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than zero.");

            this._capacity = capacity;
            this._cache = new ConcurrentDictionary<string, LinkedListNode<CacheItem>>();
            this._lruList = new LinkedList<CacheItem>();
        }

        public string[] GetKeys()
        {
            return this._cache.Keys.ToArray();
        }

        public bool Contains(string key)
        {
            return this._cache.ContainsKey(key);
        }

        public string Get(string key)
        {
            if (this._cache.TryGetValue(key, out var node))
            {
                // Move the accessed item to the front of the LRU list
                this._lruList.Remove(node);
                this._lruList.AddFirst(node);

                return node.Value.Value;
            }

            return default;
        }

        public void Add(string key, string value)
        {
            if (this._cache.Count >= this._capacity)
                this.RemoveLRUItem();

            var cacheItem = new CacheItem(key, value);
            var newNode = new LinkedListNode<CacheItem>(cacheItem);
            this._lruList.AddFirst(newNode);
            this._cache.TryAdd(key, newNode);
        }

        public bool Remove(string key)
        {
            if (this._cache.TryGetValue(key, out var node))
            {
                node.Value.Value = default;
                this._lruList.Remove(node);
                this._cache.Remove(key, out _);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            var keys = this.GetKeys();
            foreach (var key in keys)
            {
                this.Remove(key);
            }
            this._lruList.Clear();
            this._cache.Clear();
        }

        protected void RemoveLRUItem()
        {
            var lastNode = this._lruList.Last;
            var key = lastNode.Value.Key;
            var value = lastNode.Value.Value;
            if (key.IndexOf(nameof(AudioBase)) != -1)
                AudioManager.GetInstance().ForceUnload(value);
            else if (key.IndexOf(nameof(VideoBase)) != -1)
                VideoManager.GetInstance().ForceUnload(value);
            lastNode.Value.Value = default;
            this._cache.TryRemove(lastNode.Value.Key, out _);
            this._lruList.RemoveLast();
        }

        private class CacheItem
        {
            public string Key { get; }
            public string Value { get; set; }

            public CacheItem(string key, string value)
            {
                this.Key = key;
                this.Value = value;
            }
        }
    }
}