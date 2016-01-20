using System.Threading;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Represents a data access object that provides methods
    /// to load and save <see cref="Chunk"/> instances.
    /// </summary>
    public interface IChunkStorage
    {
        /// <summary>
        /// Loads the chunk with the specified index.
        /// Returns null if that fails.
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="cancellation">Cancellation Token</param>
        /// <returns>Chunk</returns>
        Task<Chunk> LoadAsync(Point index, CancellationToken cancellation);

        /// <summary>
        /// Saves the specified chunk.
        /// If the chunk is already stored, it is replaced.
        /// Otherwise, it is added to the storage.
        /// </summary>
        /// <param name="chunk">Chunk</param>
        Task SaveAsync(Chunk chunk);

        /// <summary>
        /// Loads metadata.
        /// </summary>
        /// <returns><see cref="MapMetadata"/></returns>
        Task<MapMetadata> LoadMetadataAsync();

        /// <summary>
        /// Saves the specified metadata.
        /// </summary>
        /// <param name="metadata"><see cref="MapMetadata"/></param>
        /// <returns>Task</returns>
        Task SaveMetadataAsync(MapMetadata metadata);

        /// <summary>
        /// Deletes the chunk with the specified index.
        /// If no such chunk is stored, nothing happens.
        /// </summary>
        /// <param name="index">Index</param>
        Task DeleteAsync(Point index);
    }
}
