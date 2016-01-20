namespace OrangeBugReloaded.Core.Foundation
{
    /// <summary>
    /// Supports locking and unlocking of an object for a specific purpose.
    /// The same object can declare multiple locks to secure different
    /// properties.
    /// </summary>
    public class Lock : BindableBase
    {
        private object _lockObject = new object();

        private bool _isLocked;
        private object _owner;

        /// <summary>
        /// Determines whether the object
        /// is locked.
        /// </summary>
        public bool IsLocked
        {
            get { return _isLocked; }
            set { Set(ref _isLocked, value); }
        }

        /// <summary>
        /// Determines the lock owner.
        /// Null if not locked.
        /// </summary>
        public object Owner
        {
            get { return _owner; }
            set { Set(ref _owner, value); }
        }

        /// <summary>
        /// Tries to obtain the lock.
        /// </summary>
        /// <param name="owner">Lock owner</param>
        /// <returns>True if the locking was successful, false otherwise</returns>
        public bool TryLock(object owner)
        {
            lock (_lockObject)
            {
                if (IsLocked)
                {
                    if (Owner == owner)
                        return true;
                    else
                        return false;
                }
                else
                {
                    IsLocked = true;
                    Owner = owner;
                    return true;
                }
            }
        }

        /// <summary>
        /// Unlocks the object.
        /// </summary>
        public void Release()
        {
            lock (_lockObject)
            {
                IsLocked = false;
                Owner = null;
            }
        }
    }
}
