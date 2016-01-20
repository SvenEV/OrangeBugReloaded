using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using OrangeBugReloaded.Core.LocalSinglePlayer;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Test
{
    [TestClass]
    public class MapTests
    {
        [TestMethod]
        public async Task MapTryGetAsyncNotNull()
        {
            // Try to get a location on an entirely empty map
            var storage = new DummyStorage();
            var map = await Map.CreateAsync(storage);
            var location = await map.TryGetAsync(Point.Zero);
            Assert.IsNotNull(location, "Expected Location, got null");
        }

        [TestMethod]
        public async Task ChunkLoaderTryGetAsyncNotNull()
        {
            // Try to get a chunk from an entirely empty storage
            var storage = new DummyStorage();
            var chunkLoader = new ChunkLoader(storage);
            var chunk = await chunkLoader.TryGetAsync(Point.Zero);
            Assert.IsNotNull(chunk, "Expected Chunk, got null");
            Assert.IsTrue(chunk.IsEmpty, "Chunk is not empty");

            for (var y = 0; y < Chunk.Size; y++)
                for (var x = 0; x < Chunk.Size; x++)
                    Assert.IsNotNull(chunk[x, y], "Chunk indexer returned null, expected Location");
        }
    }
}
