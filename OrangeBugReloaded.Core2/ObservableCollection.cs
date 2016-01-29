using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Represents a dynamic data collection that provides notifications when items get added or removed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableCollection<T> : IList<T>, INotifyCollectionChanged<T>
    {
        private readonly List<T> _items;
        private readonly object _lock = new object();

        /// <inheritdoc/>
        public int Count { get { lock (_lock) return _items.Count; } }

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public event Action<T> ItemAdded;

        /// <inheritdoc/>
        public event Action<T> ItemRemoved;

        /// <summary>
        /// Initializes a new <see cref="ObservableCollection{T}"/>.
        /// </summary>
        public ObservableCollection()
        {
            _items = new List<T>();
        }

        /// <summary>
        /// Initializes a new <see cref="ObservableCollection{T}"/>
        /// with the specified items.
        /// </summary>
        public ObservableCollection(IEnumerable<T> items)
        {
            _items = items.ToList();
        }

        /// <inheritdoc/>
        public T this[int index]
        {
            get { lock (_lock) return _items[index]; }
            set
            {
                lock (_lock)
                {
                    var existing = _items[index];

                    if (Equals(value, existing))
                        return;

                    ItemRemoved?.Invoke(existing);
                    _items[index] = value;
                    ItemAdded?.Invoke(value);
                }
            }
        }

        /// <inheritdoc/>
        public void Add(T item)
        {
            lock (_lock)
            {
                _items.Add(item);
                ItemAdded?.Invoke(item);
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            lock (_lock)
            {
                var oldItems = _items.ToList();
                _items.Clear();

                foreach (var item in oldItems)
                    ItemRemoved?.Invoke(item);
            }
        }

        /// <inheritdoc/>
        public void Insert(int index, T item)
        {
            lock (_lock)
            {
                _items.Insert(index, item);
                ItemAdded?.Invoke(item);
            }
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            lock (_lock)
            {
                if (_items.Remove(item))
                {
                    ItemRemoved?.Invoke(item);
                    return true;
                }
                return false;
            }
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            lock (_lock)
            {
                var item = _items[index];
                _items.RemoveAt(index);
                ItemRemoved?.Invoke(item);
            }
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            lock (_lock)
                return _items.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_lock)
                _items.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            lock (_lock)
                return _items.IndexOf(item);
        }
    }
}
