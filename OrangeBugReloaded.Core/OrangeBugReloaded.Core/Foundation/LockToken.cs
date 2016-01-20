using System;

namespace OrangeBugReloaded.Core.Foundation
{
    /// <summary>
    /// Supports locking and unlocking of objects.
    /// </summary>
    public class LockToken : IDisposable
    {
        /// <summary>
        /// The <see cref="Lock"/> objects which are locked.
        /// </summary>
        public Lock[] Targets { get; }

        /// <summary>
        /// True only if all target objects could be locked
        /// successfully. False indicates that at least one
        /// object could not be locked and that the critical
        /// code section should not be executed.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Is called when the <see cref="LockToken"/> is disposed.
        /// </summary>
        public event Action<LockToken> Disposed;

        /// <summary>
        /// Initializes a new <see cref="LockToken"/> and
        /// tries to lock the specified <see cref="Lock"/>s.
        /// </summary>
        /// <param name="targets">Objects to be locked</param>
        /// <param name="owner">Lock owner</param>
        public LockToken(Lock[] targets, object owner)
        {
            Targets = targets;
            IsValid = true;

            for (var i = 0; i < Targets.Length; i++)
            {
                IsValid &= Targets[i].TryLock(owner);

                if (!IsValid)
                {
                    // Unlock the objects we have already locked
                    for (var j = 0; j < i; j++)
                        Targets[i].Release();
                    break;
                }
            }
        }

        /// <summary>
        /// Unlocks the objects if they have been locked.
        /// </summary>
        public void Dispose()
        {
            if (IsValid)
            {
                foreach (var target in Targets)
                    target.Release();
            }

            Disposed?.Invoke(this);
        }
    }
}
