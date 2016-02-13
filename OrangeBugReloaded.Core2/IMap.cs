using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Transactions;
using System;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// A map that supports get-operations.
    /// </summary>
    public interface IReadOnlyMap
    {
        /// <summary>
        /// Gets the tile at the specified position.
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>The <see cref="Tile"/></returns>
        Task<TileInfo> GetAsync(Point position);

        /// <summary>
        /// Gets the tile metadata at the specified position.
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>The <see cref="TileMetadata"/></returns>
        Task<TileMetadata> GetMetadataAsync(Point position);
    }

    /// <summary>
    /// A map that supports get- and set-operations.
    /// </summary>
    public interface IMap : IReadOnlyMap
    {
        /// <summary>
        /// Sets the tile at the specified position.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="tile">Tile</param>
        /// <returns>True if the tile has changed during the call</returns>
        Task<bool> SetAsync(Point position, TileInfo tile);

        /// <summary>
        /// Sets the tile metadata at the specified position.
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>True if the metadata has changed during this call</returns>
        Task<bool> SetMetadataAsync(Point position, TileMetadata value);
    }

    public interface ISupportsMove
    {
        /// <summary>
        /// Tries to move the <see cref="Entity"/> at the specified
        /// source position to the specified target position.
        /// </summary>
        /// <param name="sourcePosition">The position from where the entity is moved</param>
        /// <param name="targetPosition">The position to which the entity is moved</param>
        /// <returns>Move result</returns>
        Task<MoveResult> MoveAsync(Point sourcePosition, Point targetPosition);
    }

    /// <summary>
    /// A map that supports get-, set- and move-operations and
    /// provides a stream of gameplay events.
    /// </summary>
    public interface IGameplayMap : IMap, ISupportsMove
    {
        /// <summary>
        /// Provides metadata about the map.
        /// </summary>
        IMapMetadata Metadata { get; }

        /// <summary>
        /// Provides a stream of events related to the game.
        /// These events provide information about chunks being loaded
        /// or unloaded, entity moves that are executed etc.
        /// </summary>
        IObservable<IGameEvent> Events { get; }

        /// <summary>
        /// Stores the dependencies between tiles on the map.
        /// </summary>
        MapDependencyTable Dependencies { get; }

        /// <summary>
        /// The object responsible for loading and unloading chunks.
        /// </summary>
        ChunkLoader ChunkLoader { get; }

        /// <summary>
        /// Tries to move the <see cref="Entity"/> at the specified
        /// source position to the specified target position
        /// using an existing transaction.
        /// </summary>
        /// <param name="sourcePosition">The position from where the entity is moved</param>
        /// <param name="targetPosition">The position to which the entity is moved</param>
        /// <param name="transaction">The transaction that records the changes made during the move</param>
        /// <returns>Move result</returns>
        Task<MoveResult> MoveAsync(Point sourcePosition, Point targetPosition, ITransactionWithMoveSupport transaction);

        /// <summary>
        /// Tries to spawn the specified entity at the specified position.
        /// </summary>
        /// <remarks>
        /// Spawning entities without executing a move is useful for e.g. spawning players.
        /// </remarks>
        /// <param name="entity">The entity that is spawned</param>
        /// <param name="position">The position of the tile where the entity is attached</param>
        /// <returns>Spawn result</returns>
        Task<MoveResult> SpawnAsync(Entity entity, Point position);
    }
}
