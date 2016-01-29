using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// A simple chunk storage that holds all chunks in memory.
    /// </summary>
    public class InMemoryChunkStorage : IChunkStorage
    {
        private readonly IMapMetadata _meta = null;
        private readonly Dictionary<Point, IChunk> _chunks;

        public InMemoryChunkStorage(IEnumerable<IChunk> chunks = null)
        {
            _chunks = chunks?.ToDictionary(o => o.Index) ?? new Dictionary<Point, IChunk>();
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

        // Experimental
        public string MapXML
        {
            get
            {
                XElement xChunks;

                var doc = new XDocument(
                    new XElement("Map",
                        new XElement("Metadata"),
                        new XElement("Players"),
                        new XElement("Regions"),
                        xChunks = new XElement("Chunks")));

                foreach (var chunk in _chunks.Values)
                {
                    XElement xTiles, xDesignedTiles;

                    var xChunk = new XElement("Chunk",
                        new XAttribute("Index", chunk.Index),
                        xTiles = new XElement("Tiles"),
                        xDesignedTiles = new XElement("DesignedTiles"));

                    for (var y = 0; y < Chunk.Size; y++)
                        for (var x = 0; x < Chunk.Size; x++)
                        {
                            var tile = chunk[x, y, MapLayer.Gameplay];
                            var props = tile.GetType().GetProperties(BindingFlags.Instance).Where(prop => prop.Name != "Entity").Select(prop => new XAttribute(prop.Name, prop.GetValue(tile).ToString()));
                            var xTile = new XElement(tile.GetType().Name, props);

                            if (tile.Entity != Entity.None)
                            {
                                var props2 = tile.Entity.GetType().GetProperties(BindingFlags.Instance).Select(prop => new XAttribute(prop.Name, prop.GetValue(tile.Entity).ToString()));
                                var xEntity = new XElement(tile.Entity.GetType().Name, props2);
                                xTile.Add(xEntity);
                            }

                            xTiles.Add(xTile);
                        }

                    xChunks.Add(xChunk);
                }

                return doc.ToString(SaveOptions.None);
            }
        }
    }
}
