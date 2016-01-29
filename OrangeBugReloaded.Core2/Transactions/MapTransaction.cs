using OrangeBugReloaded.Core.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Transactions
{
    public class MapTransaction : IMapTransaction, IGameEventEmitter
    {
        protected readonly IReadOnlyMap _map;
        private readonly Dictionary<Point, Tile> _changedTiles = new Dictionary<Point, Tile>();
        private readonly List<IGameEvent> _events = new List<IGameEvent>();
        private readonly MapTransaction _previousTransaction;
        private object _initiator; // Only used in the first transaction (shared across all transactions in a chain)


        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<Point, Tile>> ChangedTiles => _changedTiles;

        /// <inheritdoc/>
        public IReadOnlyMapTransaction Previous => _previousTransaction;

        /// <inheritdoc/>
        public IReadOnlyMapTransaction Next { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyMapTransaction First => Previous?.First ?? this;

        /// <inheritdoc/>
        public IReadOnlyMapTransaction Last => Next?.Last ?? this;

        public bool IsFirst => Previous == null;

        public bool IsLast => Next == null;

        /// <inheritdoc/>
        public bool IsCancelled { get; private set; }

        /// <inheritdoc/>
        public object Initiator
        {
            get { return ((MapTransaction)First)._initiator; }
            set { ((MapTransaction)First)._initiator = value; }
        }

        /// <summary>
        /// Initializes a new <see cref="MapTransaction"/> and adds
        /// it to the chain of transactions that currently ends with
        /// the specified transaction.
        /// </summary>
        /// <param name="previous">The last transaction of a chain of transactions</param>
        /// <exception cref="ArgumentNullException">If <paramref name="previous"/> is null</exception>
        /// <exception cref="ArgumentException">If the specified transaction is not the tail of the chain</exception>
        /// <exception cref="InvalidOperationException">If the specified transaction is cancelled</exception>
        public MapTransaction(MapTransaction previous)
        {
            if (previous == null)
                throw new ArgumentNullException(nameof(previous));

            if (!previous.IsLast)
                throw new ArgumentException("The transaction cannot be used as a previous transaction because it is already part of a transaction chain", nameof(previous));

            if (previous.IsCancelled)
                throw new InvalidOperationException("Follow-up transactions cannot be created for cancelled transactions");

            _previousTransaction = previous;
            _map = previous._map;
            previous.Next = this;
        }

        /// <summary>
        /// Initializes a new <see cref="MapTransaction"/> using
        /// the specified map as the underlying tile source.
        /// </summary>
        /// <param name="map">A map</param>
        public MapTransaction(IReadOnlyMap map)
        {
            // Suppose that 'map' is not a transaction
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            _previousTransaction = null;
            _map = map;
        }

        /// <summary>
        /// Gets the tile at the specified position.
        /// 
        /// Note that there may be next transactions which override
        /// the tile at the desired position.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="layer">Layer</param>
        /// <returns></returns>
        public virtual async Task<Tile> GetAsync(Point position, MapLayer layer = MapLayer.Gameplay)
        {
            Tile tile;

            // TODO: In the future we would like to use MapTransactions for designing purposes as well
            if (layer != MapLayer.Gameplay)
                throw new ArgumentException("Not supported: layer must be Gameplay", nameof(layer));

            if (_changedTiles.TryGetValue(position, out tile))
            {
                // If tile at specified position has changed in this transaction
                // return the changed tile
                return tile;
            }
            else
            {
                // Otherwise, load tile from parent transaction or,
                // if this is already the first transaction, directly from the map.
                var map = _previousTransaction ?? _map;
                return await map.GetAsync(position, layer);
            }
        }

        /// <summary>
        /// Sets the tile at the specified position.
        /// This is only allowed on the last transaction of a chain.
        /// Attempts to call this method on any other transaction in
        /// the chain will result in an exception.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="tile">Tile</param>
        /// <param name="layer">Layer</param>
        /// <returns>
        /// True if the operation was successful.
        /// False if the transaction is cancelled or the tile did not change.
        /// </returns>
        public virtual async Task<bool> SetAsync(Point position, Tile tile, MapLayer layer = MapLayer.Gameplay)
        {
            EnsureLastTransaction();

            if (IsCancelled)
                return false;

            var currentTile = await GetAsync(position, layer);

            if (!Equals(currentTile, tile))
            {
                // If tile actually differs from the one in the previous transaction
                // or on the map, add it to the list of changed tiles.
                _changedTiles[position] = tile;
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            EnsureLastTransaction();
            IsCancelled = true;
            _changedTiles.Clear();
        }

        /// <inheritdoc/>
        public void Emit(IGameEvent e) => _events.Add(e);

        internal void FlushEvents(IObserver<IGameEvent> observer)
        {
            foreach (var e in _events)
                observer.OnNext(e);
        }

        private void EnsureLastTransaction()
        {
            if (!IsLast)
                throw new InvalidOperationException("The operation can only be called on the last transaction in the chain");
        }

    }
}
