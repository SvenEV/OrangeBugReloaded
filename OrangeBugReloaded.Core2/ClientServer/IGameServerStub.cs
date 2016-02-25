using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer
{
    /// <summary>
    /// Provides methods a client uses to communicate with a server.
    /// </summary>
    public interface IGameServerStub
    {
        /// <summary>
        /// Disconnects a client from the server.
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <returns>Task</returns>
        Task LeaveAsync(string playerId);

        /// <summary>
        /// Gets the chunk with the specified index.
        /// This implies that after calling this method the
        /// client has loaded the chunk and is now interested
        /// in changes to that chunk.
        /// </summary>
        /// <param name="index">Chunk index</param>
        /// <param name="playerId">Player ID</param>
        /// <returns>The loaded chunk</returns>
        Task<IChunk> LoadChunkAsync(Point index, string playerId);

        /// <summary>
        /// Tells the server that the client has unloaded
        /// the chunk and is no longer interested in changes
        /// to that chunk.
        /// </summary>
        /// <param name="index">Chunk index</param>
        /// <param name="playerId">Player ID</param>
        /// <returns>Task</returns>
        Task UnloadChunkAsync(Point index, string playerId);

        /// <summary>
        /// Tries to execute a move on the server.
        /// </summary>
        /// <param name="move">Information about the move</param>
        /// <param name="playerId">Player ID</param>
        /// <returns>Resulting information</returns>
        Task<RemoteMoveResult> MoveAsync(RemoteMoveRequest move, string playerId);
    }
}
