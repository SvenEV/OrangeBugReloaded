using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Foundation;
using OrangeBugReloaded.Core.Tiles;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Represents the Orange Bug world.
    /// </summary>
    public interface IMap : IBindable
    {
        /// <summary>
        /// Contains metadata about the <see cref="IMap"/>.
        /// </summary>
        MapMetadata Metadata { get; }

        /// <summary>
        /// The <see cref="Core.ChunkLoader"/> used to load,
        /// unload, save and delete <see cref="Chunk"/> instances.
        /// </summary>
        ChunkLoader ChunkLoader { get; }
        
        /// <summary>
        /// Gets the <see cref="MapLocation"/> at the specified position.
        /// </summary>
        /// <param name="p">Position</param>
        /// <returns>Location (null is never returned)</returns>
        Task<MapLocation> TryGetAsync(Point p);

        /// <summary>
        /// Returns the <see cref="MapLocation"/> at the specified position
        /// if the respective <see cref="Chunk"/> is loaded.
        /// </summary>
        /// <param name="p">Position</param>
        /// <returns><see cref="MapLocation"/> or null if <see cref="Chunk"/> not loaded</returns>
        MapLocation GetIfLoaded(Point p);

        /// <summary>
        /// Creates an <see cref="Entity"/> of the specified type at the specified position.
        /// The method fails and returns null if the <see cref="Tile"/> at that position
        /// does not accept the Entity.
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="position">Position</param>
        /// <returns>The new entity or null if creation failed</returns>
        Task<T> CreateEntityAsync<T>(Point position) where T : Entity, new();

        /// <summary>
        /// Creates an <see cref="Entity"/> of the specified type at the specified position.
        /// The method fails and returns null if the <see cref="Tile"/> at that position
        /// does not accept the Entity.
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <param name="position">Position</param>
        /// <returns>The new entity or null if creation failed</returns>
        Task<Entity> CreateEntityAsync(Type entityType, Point position);

        /// <summary>
        /// Removes the <see cref="Entity"/> from the <see cref="Tile"/>
        /// at the specified position even if it can't be detached from the tile.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="e">Context</param>
        /// <returns>The entity that has been destroyed or null if there was no entity</returns>
        Task<Entity> DestroyEntityAsync(Point position, EntityMoveContext e);

        /// <summary>
        /// Subscribes to <see cref="INotifyPropertyChanged.PropertyChanged"/> events of
        /// the <see cref="Tile"/> at the specified position. However, subscriptions are not directly
        /// referencing a specific tile, but rather the position. That means that subscriptions
        /// are still valid even when the tile at that position is exchanged.
        /// </summary>
        /// <param name="position">Position on the Map</param>
        /// <param name="propertyName">Name of a property on the tile</param>
        /// <param name="handler">Event handler</param>
        void Subscribe(Point position, string propertyName, BindablePropertyChangedEventHandler handler);

        /// <summary>
        /// Unsubscribes from <see cref="INotifyPropertyChanged.PropertyChanged"/> events of
        /// the <see cref="Tile"/> at the specified position.
        /// </summary>
        /// <param name="position">Position on the Map</param>
        /// <param name="propertyName">Name of a property on the tile</param>
        /// <param name="handler">Event handler</param>
        void Unsubscribe(Point position, string propertyName, BindablePropertyChangedEventHandler handler);
    }
}