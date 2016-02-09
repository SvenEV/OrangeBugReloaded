using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// A simple chunk storage that holds all chunks in memory.
    /// </summary>
    public class InMemoryChunkStorage : IChunkStorage
    {
        private readonly IMapMetadata _meta;
        private readonly Dictionary<Point, IChunk> _chunks;

        public InMemoryChunkStorage(IEnumerable<IChunk> chunks = null)
        {
            _chunks = chunks?.ToDictionary(o => o.Index) ?? new Dictionary<Point, IChunk>();
            _meta = null;
        }

        /// <inheritdoc/>
        public Task DeleteAsync(Point index)
        {
            _chunks.Remove(index);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<IChunk> LoadAsync(Point index, CancellationToken cancellation)
        {
            if (!_chunks.ContainsKey(index))
                _chunks[index] = new Chunk(index);

            return Task.FromResult(_chunks[index]);
        }

        /// <inheritdoc/>
        public Task SaveAsync(IChunk chunk)
        {
            if (!_chunks.ContainsKey(chunk.Index))
                _chunks[chunk.Index] = chunk;

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<IMapMetadata> LoadMetadataAsync() => Task.FromResult(_meta);

        /// <inheritdoc/>
        public Task SaveMetadataAsync(IMapMetadata metadata) => Task.CompletedTask;
    }
}
