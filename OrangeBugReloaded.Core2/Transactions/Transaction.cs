using OrangeBugReloaded.Core.Events;
using System.Collections.Generic;

namespace OrangeBugReloaded.Core.Transactions
{
    /// <summary>
    /// A basic transaction.
    /// </summary>
    public class Transaction : ITransaction
    {
        /// <inheritdoc/>
        public IDictionary<Point, Tile> ChangedTiles { get; } = new Dictionary<Point, Tile>();

        /// <inheritdoc/>
        public IList<IGameEvent> Events { get; } = new List<IGameEvent>();
        
        /// <inheritdoc/>
        public bool IsCancelled { get; private set; }
        
        /// <inheritdoc/>
        public void Cancel()
        {
            IsCancelled = true;
            ChangedTiles.Clear();
        }
    }
}
