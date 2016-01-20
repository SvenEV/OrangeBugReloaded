using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OrangeBugReloaded.Core.Entities;
using System;

namespace OrangeBugReloaded.Core.LocalSinglePlayer
{
    /// <summary>
    /// A simple and stupid JSON storage that always
    /// loads and saves all chunks at once.
    /// </summary>
    public class BasicJsonStorage : IChunkStorage
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.Indented
        };

        private Dictionary<Point, Chunk> _chunks;
        private MapMetadata _metadata;

        /// <summary>
        /// Path to the json file where the chunks are saved.
        /// </summary>
        public string Filename { get; }

        /// <summary>
        /// Initializes an new <see cref="BasicJsonStorage"/> using
        /// the specified file.
        /// </summary>
        /// <param name="filename">Path to the json file where the chunks are saved</param>
        public BasicJsonStorage(string filename)
        {
            Filename = filename;
        }

        /// <inheritdoc/>
        public async Task<Chunk> LoadAsync(Point index, CancellationToken cancellation)
        {
            if (_chunks == null)
                await LoadEverythingAsync();

            Chunk chunk;
            _chunks.TryGetValue(index, out chunk);
            return chunk;
        }

        /// <inheritdoc/>
        public async Task SaveAsync(Chunk chunk)
        {
            Logger.LogInfo($"Saving chunk {chunk.Index}. Player still there? {chunk.Any(o => o.Tile?.Entity is PlayerEntity)}");

            _chunks.Remove(chunk.Index);
            _chunks.Add(chunk.Index, chunk);

            await SaveEverythingAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Point index)
        {
            _chunks.Remove(index);
            await SaveEverythingAsync();
        }

        /// <inheritdoc/>
        public async Task<MapMetadata> LoadMetadataAsync()
        {
            if (_chunks == null)
                await LoadEverythingAsync();

            return _metadata;
        }

        /// <inheritdoc/>
        public async Task SaveMetadataAsync(MapMetadata metadata)
        {
            _metadata = metadata;
            await SaveEverythingAsync();
        }

        private Task LoadEverythingAsync()
        {
            try
            {
                if (File.Exists(Filename))
                {
                    var json = File.ReadAllText(Filename);
                    var file = JsonConvert.DeserializeObject<WorldFile>(json);
                    _metadata = file.Metadata;
                    _chunks = new Dictionary<Point, Chunk>(file.Chunks.ToDictionary(o => o.Index));
                }
                else
                {
                    _metadata = new MapMetadata { Creator = "LocalPlayer", CreationDate = DateTime.Now };
                    _chunks = new Dictionary<Point, Chunk>();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }

            return TaskEx.FromResult(0);
        }

        private Task SaveEverythingAsync()
        {
            var file = new WorldFile
            {
                Metadata = _metadata,
                Chunks = _chunks.Values.ToArray()
            };

            var json = JsonConvert.SerializeObject(file, _settings);
            File.WriteAllText(Filename, json);
            return TaskEx.FromResult(0);
        }



        class WorldFile
        {
            public MapMetadata Metadata { get; set; }
            public Chunk[] Chunks { get; set; }
        }
    }
}
