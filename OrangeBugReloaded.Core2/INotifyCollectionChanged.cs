using System;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Notifies listeners of dynamic changes, such as when items get added and removed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INotifyCollectionChanged<T>
    {
        /// <summary>
        /// Occurs when an item is added.
        /// </summary>
        event Action<T> ItemAdded;

        /// <summary>
        /// Occurs when an item is removed.
        /// </summary>
        event Action<T> ItemRemoved;
    }
}
