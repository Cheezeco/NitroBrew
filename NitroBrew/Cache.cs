using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private TimeSpan _itemLifeSpan = TimeSpan.FromSeconds(30);

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

                var isEnumerable = value.GetType().IsEnumerableType();

                if (!TryGet(key, isEnumerable, out CacheItem item))
                {
                    var props = SeparateProperties(GetKey(key, isEnumerable), value);
                    foreach (var prop in props)
                    {
                        _cacheItems.Add(
                            GetKey(prop.Key, prop.Value.GetType().IsEnumerableType()), new CacheItem(prop.Value));
                    }

                    _cacheItems.Add(GetKey(key, isEnumerable), new CacheItem(copy));
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
                _cacheItems.Remove(GetKey(key, type.IsEnumerableType()));
            }
        }

        public void Remove<T>(object key)
        {
            Remove(key, typeof(T));
        }

        public object Get(object key, Type type)
        {
            TryGet(key, type.IsEnumerableType(), out object value);

            return value;
        }

        public T Get<T>(object key) where T : class
        {
            var value = Get(key, typeof(T));

            return value as T;
        }

        public bool TryGet<T>(object key, out T value) where T : class
        {
            var success = TryGet(key, typeof(T).IsEnumerableType(), out object item);

            value = item as T;

            return success;
        }

        public bool TryGet(object key, bool isEnumerable, out object value)
        {
            lock (_lock)
            {
                TryGet(key, isEnumerable, out CacheItem item);
                value = item?.Value;

                RemoveStaleItems();
            }

            return value.IsNotNull();
        }

        private bool TryGet(object key, bool isEnumerable, out CacheItem item)
        {
            return _cacheItems.TryGetValue(GetKey(key, isEnumerable), out item);
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
            foreach (var staleItem in _cacheItems.Where(pair => IsStale(pair.Value)))
            {
                _cacheItems.Remove(staleItem.Key);
            }
        }

        private bool IsStale(CacheItem item)
        {
            return item.GetTimeSinceUpdate() > ItemLifeSpan;
        }

        private static IEnumerable<(object Key, object Value)> SeparateProperties(object key, object value)
        {
            if (value is null) return new List<(object Key, object Value)>();

            var type = value.GetType();
            var properties = type.GetProperties();

            var separatedProperties = new List<(object Key, object Value)>(properties.Length);

            foreach (var property in properties)
            {
                if (property.PropertyType.IsPrimitiveType()) continue;

                var propertyValue = property.GetValue(value);

                if (propertyValue is null) continue;

                separatedProperties.AddRange(SeparateProperties(GetKey(key, value.GetType().IsEnumerableType()),
                    property.PropertyType.IsEnumerableType() ? propertyValue : property.GetValue(value)));
            }

            return separatedProperties;
        }

        private static string GetKey(object key, bool isEnumerable)
        {
            return $"{key}:{isEnumerable}";
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