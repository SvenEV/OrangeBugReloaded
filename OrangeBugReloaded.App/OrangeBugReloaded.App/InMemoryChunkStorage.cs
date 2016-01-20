using OrangeBugReloaded.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using OrangeBugReloaded.Core.Tiles;
using OrangeBugReloaded.Core.Entities;

namespace OrangeBugReloaded.App
{
    class InMemoryChunkStorage : IChunkStorage
    {
        private readonly MapMetadata _meta = new MapMetadata();
        private readonly Dictionary<Point, Chunk> _chunks;

        private InMemoryChunkStorage(IEnumerable<Chunk> chunks = null)
        {
            _chunks = chunks.ToDictionary(o => o.Index);
        }

        public static InMemoryChunkStorage CreateEmpty() => new InMemoryChunkStorage();

        public static InMemoryChunkStorage CreateSampleWorld()
        {
            var c = new Chunk(Point.Zero);

            foreach (var location in c)
                location.Tile = new PathTile();

            return new InMemoryChunkStorage(new[] { c });
        }

        public Task DeleteAsync(Point index)
        {
            _chunks.Remove(index);
            return Task.CompletedTask;
        }

        public Task<Chunk> LoadAsync(Point index, CancellationToken cancellation)
        {
            if (!_chunks.ContainsKey(index))
                _chunks[index] = new Chunk(index);

            return Task.FromResult(_chunks[index]);
        }

        public Task<MapMetadata> LoadMetadataAsync() => Task.FromResult(_meta);

        public Task SaveAsync(Chunk chunk)
        {
            if (!_chunks.ContainsKey(chunk.Index))
                _chunks[chunk.Index] = chunk;

            return Task.CompletedTask;
        }

        public Task SaveMetadataAsync(MapMetadata metadata) => Task.CompletedTask;
    }
}
