using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OxGFrame.Utility.Cacher
{
    public class LRUCache<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly ConcurrentDictionary<TKey, LinkedListNode<CacheItem>> _cache;
        private readonly LinkedList<CacheItem> _lruList;

        public int Count => this._cache.Count;

        public LRUCache(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than zero.");

            this._capacity = capacity;
            this._cache = new ConcurrentDictionary<TKey, LinkedListNode<CacheItem>>();
            this._lruList = new LinkedList<CacheItem>();
        }

        public bool Contains(TKey key)
        {
            return this._cache.ContainsKey(key);
        }

        public TValue Get(TKey key)
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

        public void Add(TKey key, TValue value)
        {
            if (this._cache.Count >= this._capacity)
                RemoveLRUItem();

            var cacheItem = new CacheItem(key, value);
            var newNode = new LinkedListNode<CacheItem>(cacheItem);
            this._lruList.AddFirst(newNode);
            this._cache.TryAdd(key, newNode);
        }

        protected void RemoveLRUItem()
        {
            var lastNode = this._lruList.Last;
            this._cache.TryRemove(lastNode.Value.Key, out _);
            this._lruList.RemoveLast();
        }

        private class CacheItem
        {
            public TKey Key { get; }
            public TValue Value { get; }

            public CacheItem(TKey key, TValue value)
            {
                this.Key = key;
                this.Value = value;
            }
        }
    }
}