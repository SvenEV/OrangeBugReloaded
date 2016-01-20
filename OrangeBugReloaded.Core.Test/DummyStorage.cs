using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Test
{
    class DummyStorage : IChunkStorage
    {
        public MapMetadata StoredMetadata { get; set; }

        public List<Chunk> StoredChunks { get; } = new List<Chunk>();

        public Task DeleteAsync(Point index)
        {
            var chunk = StoredChunks.FirstOrDefault(o => o.Index == index);
            Assert.IsNotNull(chunk, "Tried to delete a chunk that was not stored");
            StoredChunks.Remove(chunk);
            return TaskEx.FromResult(0);
        }

        public Task<Chunk> LoadAsync(Point index, CancellationToken cancellation)
        {
            var chunk = StoredChunks.FirstOrDefault(o => o.Index == index);
            return TaskEx.FromResult(chunk);
        }

        public Task<MapMetadata> LoadMetadataAsync()
        {
            return TaskEx.FromResult(StoredMetadata);
        }

        public Task SaveAsync(Chunk chunk)
        {
            if (!StoredChunks.Contains(chunk))
                StoredChunks.Add(chunk);

            return TaskEx.FromResult(0);
        }

        public Task SaveMetadataAsync(MapMetadata metadata)
        {
            StoredMetadata = metadata;
            return TaskEx.FromResult(0);
        }
    }
}
