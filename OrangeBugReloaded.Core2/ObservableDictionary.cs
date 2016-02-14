using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Represents a dictionary that provides notifications when items get added or removed.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> _dict;
        private readonly object _lock = new object();

        /// <inheritdoc/>
        public TValue this[TKey key]
        {
            get { return _dict[key]; }
            set
            {
                lock (_lock)
                {
                    TValue existing;

                    if (_dict.TryGetValue(key, out existing))
                    {
                        if (Equals(value, existing))
                            return;

                        ItemRemoved?.Invoke(new KeyValuePair<TKey, TValue>(key, existing));
                    }

                    _dict[key] = value;
                    ItemAdded?.Invoke(new KeyValuePair<TKey, TValue>(key, value));
                }
            }
        }

        /// <inheritdoc/>
        public int Count { get { lock (_lock) return _dict.Count; } }

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public ICollection<TKey> Keys { get { lock (_lock) return _dict.Keys.ToArray(); } }

        /// <inheritdoc/>
        public ICollection<TValue> Values { get { lock (_lock) return _dict.Values.ToArray(); } }

        /// <inheritdoc/>
        public event Action<KeyValuePair<TKey, TValue>> ItemAdded;

        /// <inheritdoc/>
        public event Action<KeyValuePair<TKey, TValue>> ItemRemoved;



        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/>
        /// class that is empty.
        /// </summary>
        public ObservableDictionary()
        {
            _dict = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/>
        /// class that contains elements copied from the specified <see cref="IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="entries">
        /// The <see cref="IDictionary{TKey, TValue}"/> whose elements are copied to the new
        /// <see cref="ObservableDictionary{TKey, TValue}"/></param>
        public ObservableDictionary(IDictionary<TKey, TValue> entries)
        {
            _dict = new Dictionary<TKey, TValue>(entries);
        }



        /// <inheritdoc/>
        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                _dict.Add(key, value);
                ItemAdded?.Invoke(new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            lock (_lock)
            {
                foreach (var item in _dict)
                    ItemRemoved?.Invoke(item);

                _dict.Clear();
            }
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            lock (_lock)
                return _dict.ContainsKey(key);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();

        /// <inheritdoc/>
        public bool Remove(TKey key)
        {
            lock (_lock)
            {
                TValue value;

                if (_dict.TryGetValue(key, out value))
                {
                    _dict.Remove(key);
                    ItemRemoved?.Invoke(new KeyValuePair<TKey, TValue>(key, value));
                    return true;
                }

                return false;
            }
        }

        /// <inheritdoc/>
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_lock)
                return _dict.TryGetValue(key, out value);
        }
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Unsupported and hidden
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dict).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}
