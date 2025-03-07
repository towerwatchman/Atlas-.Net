using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Atlas.Core.Utilities
{
    public class LruCache<TKey, TValue>
    {
        private readonly int _capacity;
        private readonly ConcurrentDictionary<TKey, TValue> _cache;
        private readonly LinkedList<TKey> _lruList;
        private readonly object _lock = new object();

        public LruCache(int capacity)
        {
            _capacity = capacity;
            _cache = new ConcurrentDictionary<TKey, TValue>();
            _lruList = new LinkedList<TKey>();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out value))
            {
                lock (_lock)
                {
                    _lruList.Remove(key);
                    _lruList.AddFirst(key); // Move to front as most recently used
                }
                return true;
            }
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (_cache.Count >= _capacity)
                {
                    var last = _lruList.Last; // Least recently used item
                    _lruList.RemoveLast();
                    _cache.TryRemove(last.Value, out _); // Remove from cache
                }
                _cache[key] = value;
                _lruList.AddFirst(key); // Add as most recently used
            }
        }
    }
}
