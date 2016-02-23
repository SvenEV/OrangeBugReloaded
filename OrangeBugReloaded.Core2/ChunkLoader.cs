using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Provides methods to load, cache and save chunks.
    /// </summary> 
    public class ChunkLoader
    {
        /// <summary>
        /// A collection of all the chunks currently loaded.
        /// </summary>
        public ObservableDictionary<Point, IChunk> Chunks { get; } = new ObservableDictionary<Point, IChunk>();

        /// <summary>
        /// A collection of all indices of chunks that are currently being loaded from <see cref="Storage"/>.
        /// </summary>
        public ObservableDictionary<Point, LoadingChunkToken> ChunksLoading { get; } = new ObservableDictionary<Point, LoadingChunkToken>();

        /// <summary>
        /// The storage used to load and save chunks.
        /// </summary>
        public IChunkStorage Storage { get; }

        /// <summary>
        /// Initializes a new ChunkLoader utilizing the specified
        /// IChunkStorage to load and save chunks.
        /// </summary>
        /// <param name="storage">Chunk Storage</param>
        /// 
        public ChunkLoader(IChunkStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Saves all <see cref="Chunk"/> instances that have been changed to <see cref="Storage"/>.
        /// Empty <see cref="Chunk"/> instances are deleted from <see cref="Storage"/>.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            var tasks = Chunks.Select(kvp => SaveChangesOrDeleteIfEmptyAsync(kvp.Key)).ToArray();
            await TaskEx.WhenAll(tasks);
        }

        private async Task<bool> SaveChangesOrDeleteIfEmptyAsync(Point index)
        {
            var chunk = Chunks[index];

            if (chunk.IsEmpty)
            {
                await Storage.DeleteAsync(index);
                Chunks.Remove(index);
                return true;
            }
            else if (chunk.HasChanged)
            {
                // Save a clone of the chunk so that any further changes to
                // the chunk do not propagate to the cache of the storage.
                await Storage.SaveAsync(index, chunk.Clone());
                chunk.OnSaved();
            }
            return false;
        }

        /// <summary>
        /// Unloads the chunk with the specified index
        /// if it is loaded.
        /// </summary>
        /// <param name="index">Chunk index</param>
        /// <param name="saveChanges">Indicates whether changes should be saved or discarded</param>
        /// <returns>Task</returns>
        public async Task UnloadAsync(Point index, bool saveChanges)
        {
            IChunk chunk;

            if (ChunksLoading.ContainsKey(index))
            {
                // Chunk is still being loaded
                // -> Cancel loading operation
                ChunksLoading[index].Cancellation.Cancel();
            }
            else if (Chunks.TryGetValue(index, out chunk))
            {
                // Chunk is already loaded. Remove it!
                if (saveChanges)
                    if (await SaveChangesOrDeleteIfEmptyAsync(index))
                        return; // Chunk was empty and has already been removed

                Chunks.Remove(index);
            }
        }

        /// <summary>
        /// Obtains the <see cref="Chunk"/> with the specified index.
        /// If the chunk is already loaded, it is returned.
        /// Otherwise, it is loaded from <see cref="Storage"/>.
        /// </summary>
        /// <param name="index">Chunk index</param>
        /// <returns>The desired chunk. If the chunk could not be loaded, a new empty chunk is returned</returns>
        public async Task<IChunk> GetAsync(Point index)
        {
            // If chunk already loaded, return it immediately
            IChunk alreadyLoadedChunk;
            if (Chunks.TryGetValue(index, out alreadyLoadedChunk))
                return alreadyLoadedChunk;

            // If chunk is already being loaded, await its loader task (do not start a second task)
            LoadingChunkToken loadingChunkToken;
            if (ChunksLoading.TryGetValue(index, out loadingChunkToken))
                return await loadingChunkToken.LoaderTask;

            // Otherwise, begin loading the chunk
            var token = new LoadingChunkToken { Cancellation = new CancellationTokenSource() };
            ChunksLoading.Add(index, token);

            IChunk chunk = null;

            try
            {
                token.LoaderTask = Storage.LoadAsync(index, token.Cancellation.Token);
                chunk = await token.LoaderTask;
                token.Cancellation.Token.ThrowIfCancellationRequested(); // Handle cancellation in case storage doesn't
            }
            catch (OperationCanceledException)
            {
                // Chunk loading was cancelled by an Unload(...) call, do nothing
            }
            catch
            {
                // Something else happened, throw
                throw;
            }

            if (chunk == null)
            {
                // If chunk loading fails, create an empty chunk
                // (empty chunks won't be saved)
                chunk = new Chunk();
            }
            else
            {
                // The storage might have its own caching strategies and
                // reuse chunks it has once loaded, so we need to work with
                // a copy of the loaded chunk.
                chunk = chunk.Clone();
            }

            ChunksLoading.Remove(index);
            Chunks.Add(index, chunk);
            return chunk;
        }

        /// <summary>
        /// Checks whether the chunk with the specified index
        /// is already loaded or is currently being loaded.
        /// </summary>
        /// <param name="index">Chunk index</param>
        /// <returns>True if available or being loaded; otherwise, false.</returns>
        public bool IsLoadedOrLoading(Point index) => Chunks.ContainsKey(index) || ChunksLoading.ContainsKey(index);


        /// <summary>
        /// Represents the loading operation of an <see cref="IChunk"/>.
        /// </summary>
        public class LoadingChunkToken
        {
            /// <summary>
            /// The <see cref="Task"/> that loads the chunk.
            /// </summary>
            public Task<IChunk> LoaderTask { get; set; }

            /// <summary>
            /// The <see cref="CancellationTokenSource"/> used to
            /// cancel the operation.
            /// </summary>
            public CancellationTokenSource Cancellation { get; set; }
        }
    }
}
