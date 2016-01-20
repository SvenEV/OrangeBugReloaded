using System;
using System.Collections.Generic;
using System.Linq;

namespace OrangeBugReloaded.Core.Foundation
{
    /// <summary>
    /// Locks and unlocks objects.
    /// </summary>
    public class LockContext
    {
        private List<LockToken> _locks = new List<LockToken>();
        private Func<object, Lock> _lockSelector;

        private LockContext(Func<object, Lock> lockSelector)
        {
            _lockSelector = lockSelector;
        }

        /// <summary>
        /// Initializes a new <see cref="LockContext"/> that supports locking
        /// and unlocking objects regarding the specified <see cref="Lock"/>.
        /// </summary>
        /// <param name="lockSelector">
        /// Selects the <see cref="Lock"/> for each object that needs to be locked
        /// </param>
        public static LockContext Create<T>(Func<T, Lock> lockSelector)
        {
            // No (co)variance for delegates in .NET 3.5 => We have to cast
            return new LockContext(o => lockSelector((T)o));
        }

        /// <summary>
        /// Locks the specified objects in addition to the ones that are
        /// already locked in this context.
        /// If any of object in <paramref name="targets"/> is already locked
        /// in the context of this <see cref="LockContext"/>, the resulting
        /// <see cref="LockToken"/> is still valid.
        /// Null-values in <paramref name="targets"/> are ignored.
        /// </summary>
        /// <param name="targets">Objects to be locked</param>
        /// <returns>
        /// A new <see cref="LockToken"/> that can be used to unlock
        /// the objects</returns>
        public LockToken Lock(params object[] targets)
        {
            var remainingTargets = targets
                .Where(o => o != null)
                .Select(o => _lockSelector(o))
                .Except(_locks.SelectMany(o => o.Targets))
                .ToArray();

            var newLock = new LockToken(remainingTargets, this);
            newLock.Disposed += OnLockDisposed;
            _locks.Add(newLock);

            return newLock;
        }

        /// <summary>
        /// Disposes all <see cref="LockToken"/>s that have been
        /// produced by this <see cref="LockContext"/> and not yet
        /// been disposed.
        /// </summary>
        public void DisposeAll()
        {
            while (_locks.Any())
                _locks[0].Dispose();
        }

        private void OnLockDisposed(LockToken token)
        {
            _locks.Remove(token);
        }
    }
}
