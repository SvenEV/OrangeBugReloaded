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
        private readonly Dictionary<Point, IChunk> _chunks = new Dictionary<Point, IChunk>();

        public InMemoryChunkStorage()
        {
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
        public Task SaveAsync(Point index, IChunk chunk)
        {
            if (!_chunks.ContainsKey(index))
                _chunks[index] = chunk;

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<IMapMetadata> LoadMetadataAsync() => Task.FromResult(_meta);

        /// <inheritdoc/>
        public Task SaveMetadataAsync(IMapMetadata metadata) => Task.CompletedTask;
    }
}
