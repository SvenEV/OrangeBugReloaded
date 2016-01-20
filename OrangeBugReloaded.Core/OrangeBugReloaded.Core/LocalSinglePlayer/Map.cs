using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Foundation;
using OrangeBugReloaded.Core.Tiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.LocalSinglePlayer
{
    /// <summary>
    /// Represents the Orange Bug world.
    /// </summary>
    public class Map : BindableBase, IMap, ILocationsProvider
    {
        private readonly ChunkLoader _chunkLoader;

        private Dictionary<TilePropertyChangedKey, List<BindablePropertyChangedEventHandler>> _handlers =
            new Dictionary<TilePropertyChangedKey, List<BindablePropertyChangedEventHandler>>();

        /// <inheritdoc/>
        public MapMetadata Metadata { get; private set; }

        /// <inheritdoc/>
        public ChunkLoader ChunkLoader => _chunkLoader;

        /// <inheritdoc/>
        public ObservableDictionary<Point, ILocation> Locations { get; } = new ObservableDictionary<Point, ILocation>();

        private Map(IChunkStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            _chunkLoader = new ChunkLoader(storage);
            _chunkLoader.Chunks.ItemAdded += OnChunkAdded;
            _chunkLoader.Chunks.ItemRemoved += OnChunkRemoved;
        }

        /// <summary>
        /// Initializes a new Map with the specified storage.
        /// </summary>
        public static async Task<Map> CreateAsync(IChunkStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            var map = new Map(storage);
            map.Metadata = await storage.LoadMetadataAsync();
            return map;
        }

        private void OnChunkRemoved(KeyValuePair<Point, Chunk> item)
        {
            for (var y = 0; y < Chunk.Size; y++)
            {
                for (var x = 0; x < Chunk.Size; x++)
                {
                    var location = item.Value[x, y];
                    location.Map = null;
                    location.Unsubscribe(nameof(MapLocation.Tile), OnLocationTileChanged);

                    Locations.Remove(location.Position);

                    if (location.Tile != null)
                    {
                        OnLocationTileChanged(null, new BindablePropertyChangedEventArgs("", location.Tile, null));
                    }
                }
            }
        }

        // TODO: Async void ò.ó
        private async void OnChunkAdded(KeyValuePair<Point, Chunk> item)
        {
            for (var y = 0; y < Chunk.Size; y++)
            {
                for (var x = 0; x < Chunk.Size; x++)
                {
                    var location = item.Value[x, y];
                    location.Map = this;
                    location.Subscribe(nameof(MapLocation.Tile), OnLocationTileChanged);

                    Locations.Add(location.Position, location);

                    if (location.Tile != null)
                    {
                        OnLocationTileChanged(null, new BindablePropertyChangedEventArgs("", null, location.Tile));
                        await location.Tile.ActivateAsync();
                    }
                }
            }
        }

        private void OnLocationTileChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            // Is called, when the tile of a location has changed.
            // Subscribes to and unsubscribes from Tile.PropertyChanged-event.

            var oldTile = e.OldValue as Tile;
            var newTile = e.NewValue as Tile;

            if (oldTile != null)
                oldTile.PropertyChanged -= OnAnyTilePropertyChanged;

            if (newTile != null)
                newTile.PropertyChanged += OnAnyTilePropertyChanged;
        }

        private void OnAnyTilePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Any property on any tile has changed
            var tile = (Tile)sender;
            var key = new TilePropertyChangedKey(tile.Location.Position, e.PropertyName);

            // Call all handlers that have subscribed to events at that position with that property name
            List<BindablePropertyChangedEventHandler> list;

            if (_handlers.TryGetValue(key, out list))
                foreach (var handler in list)
                    handler(sender, (BindablePropertyChangedEventArgs)e);
        }

        /// <summary>
        /// Saves all <see cref="Chunk"/> instances that have been changed to storage.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _chunkLoader.Storage.SaveMetadataAsync(Metadata);
            await _chunkLoader.SaveChangesAsync();
        }

        /// <inheritDoc/>
        public Task<Entity> CreateEntityAsync(Type entityType, Point position) =>
            CreateEntityCoreAsync((Entity)Activator.CreateInstance(entityType), position);

        /// <inheritdoc/>
        public async Task<T> CreateEntityAsync<T>(Point position) where T : Entity, new() =>
            (T)await CreateEntityCoreAsync(new T(), position);

        private async Task<Entity> CreateEntityCoreAsync(Entity entity, Point position)
        {
            if (!await entity.TryMoveAsync(position, new EntityMoveContext(this, entity)))
            {
                Logger.LogError($"Failed to create entity at {position}");
                return null;
            }

            return entity;
        }

        /// <inheritdoc/>
        public async Task<Entity> DestroyEntityAsync(Point position, EntityMoveContext e)
        {
            var tile = (await TryGetAsync(position))?.Tile;
            var entity = tile?.Entity;

            if (entity == null)
            {
                Logger.LogWarning($"There is no entity at {position}");
                return null;
            }

            // Move to null - this is by design
            await entity.TryMoveAsync(null, e);

            tile.Entity = null;
            entity.Owner = null;
            entity.UnsubscribeAll();
            return entity;
        }

        /// <inheritdoc/>
        public void Subscribe(Point position, string propertyName, BindablePropertyChangedEventHandler handler)
        {
            List<BindablePropertyChangedEventHandler> list;
            var key = new TilePropertyChangedKey(position, propertyName);

            if (!_handlers.TryGetValue(key, out list))
                list = _handlers[key] = new List<BindablePropertyChangedEventHandler>();

            list.Add(handler);
        }

        /// <inheritdoc/>
        public void Unsubscribe(Point position, string propertyName, BindablePropertyChangedEventHandler handler)
        {
            List<BindablePropertyChangedEventHandler> list;
            var key = new TilePropertyChangedKey(position, propertyName);

            if (!_handlers.TryGetValue(key, out list))
                return;

            list.Remove(handler);

            if (list.Count == 0)
                _handlers.Remove(key);
        }

        /// <inheritdoc/>
        public async Task<MapLocation> TryGetAsync(Point p)
        {
            var chunk = await _chunkLoader.TryGetAsync(p / Chunk.Size);
            return chunk[p % Chunk.Size];
        }

        /// <inheritdoc/>
        public MapLocation GetIfLoaded(Point p)
        {
            Chunk chunk;
            return ChunkLoader.Chunks.TryGetValue(p / Chunk.Size, out chunk) ? chunk[p % Chunk.Size] : null;
        }

        struct TilePropertyChangedKey
        {
            public Point Position { get; }
            public string PropertyName { get; }

            public TilePropertyChangedKey(Point position, string propertyName) : this()
            {
                Position = position;
                PropertyName = propertyName;
            }
        }
    }
}
