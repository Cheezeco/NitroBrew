﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroBrew.Attributes;
using NitroBrew.Extensions;

namespace NitroBrew
{
    public class Cache
    {
        public TimeSpan ItemLifeSpan
        {
            get
            {
                lock (_lock) return _itemLifeSpan;
            }
            set
            {
                lock (_lock)
                {
                    _itemLifeSpan = value;
                }
            }
        }

        private readonly Dictionary<string, CacheItem> _cacheItems;
        private object _lock;
        private TimeSpan _itemLifeSpan = TimeSpan.FromSeconds(60);

        public Cache()
        {
            _cacheItems = new Dictionary<string, CacheItem>();
            _lock = new object();
        }

        public void Add(object key, object value)
        {
            lock (_lock)
            {
                var copy = value.CreateCopy();

                if (copy is null) return;

                //var isEnumerable = value.GetType().IsEnumerableType();
                var valueType = value.GetType();

                if (!TryGet(key, valueType, out CacheItem item))
                {
                    //var props = SeparateProperties(GetKey(key, isEnumerable), value);
                    //foreach (var prop in props)
                    //{
                    //    _cacheItems.Add(
                    //        GetKey(prop.Key, prop.Value.GetType().IsEnumerableType()), new CacheItem(prop.Value));
                    //}

                    _cacheItems.Add(GetKey(key, valueType), new CacheItem(copy));
                }

                if (item.IsNotNull())
                {
                    item.Value = copy;
                }

                RemoveStaleItems();
            }
        }

        public void Remove(object key, Type type)
        {
            lock (_lock)
            {
                _cacheItems.Remove(GetKey(key, type));
            }
        }

        public void Remove<T>(object key)
        {
            Remove(key, typeof(T));
        }

        public object Get(object key, Type type)
        {
            TryGet(key, type, out object value);

            return value;
        }

        public T Get<T>(object key) where T : class
        {
            var value = Get(key, typeof(T));

            return value as T;
        }

        public bool TryGet<T>(object key, out T value) where T : class
        {
            var success = TryGet(key, typeof(T), out object item);

            value = item as T;

            return success;
        }

        public bool TryGet(object key, Type type, out object value)
        {
            lock (_lock)
            {
                TryGet(key, type, out CacheItem item);
                value = item?.Value;

                RemoveStaleItems();
            }

            return value.IsNotNull();
        }

        private bool TryGet(object key, Type type, out CacheItem item)
        {
            return _cacheItems.TryGetValue(GetKey(key, type), out item);
        }

        public void ClearCache()
        {
            lock (_lock)
            {
                _cacheItems.Clear();
            }
        }

        private void RemoveStaleItems()
        {
            var staleItems = _cacheItems.Where(pair => IsStale(pair.Value)).ToDictionary(pair => pair.Key, pair => pair.Value);
            foreach (var staleItem in staleItems)
            {
                _cacheItems.Remove(staleItem.Key);
            }
        }

        private bool IsStale(CacheItem item)
        {
            return item.GetTimeSinceUpdate() > ItemLifeSpan;
        }

        private static string GetKey(object key, Type type)
        {
            return $"{key}:{type}";
        }
    }

    internal class CacheItem
    {
        public object Value
        {
            get => _value;
            set
            {
                LastUpdate = DateTime.UtcNow;
                _value = value;
            }
        }

        public DateTime LastUpdate { get; set; }

        private object _value;

        public CacheItem(object value)
        {
            Value = value;
        }

        public TimeSpan GetTimeSinceUpdate()
        {
            return DateTime.UtcNow - LastUpdate;
        }
    }
}