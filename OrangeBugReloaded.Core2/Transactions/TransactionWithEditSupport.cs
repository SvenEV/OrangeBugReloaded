using OrangeBugReloaded.Core.Events;
using System;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Transactions
{
    public class TransactionWithEditSupport : TransactionBase<TileMetadata>
    {
        public TransactionWithEditSupport(MoveInitiator initiator) : base(initiator)
        {
        }

        /// <summary>
        /// Applies the changes collected in the transaction to the specified
        /// map and flushes events to the specified observer.
        /// </summary>
        /// <param name="map">Map</param>
        /// <param name="eventSource">Observer</param>
        /// <returns>Task</returns>
        public async Task CommitAsync(IMap map, IObserver<IGameEvent> eventSource)
        {
            // Apply recorded changes to map
            foreach (var kvp in Changes)
                await map.SetMetadataAsync(kvp.Key, kvp.Value);

            // Flush recorded events
            foreach (var e in Events)
                eventSource?.OnNext(e);
        }
    }
}
