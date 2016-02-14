using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer
{
    public interface IGameServer
    {
        IGameplayMap Map { get; }

        /// <summary>
        /// Establishes a connection from a client to the server.
        /// </summary>
        /// <param name="clientInfo">Client information</param>
        /// <returns>Result</returns>
        Task<ConnectResult> ConnectAsync(ClientConnectRequest clientInfo);

        /// <summary>
        /// Disconnects a client from the server.
        /// </summary>
        /// <param name="connectionId">Client ID</param>
        /// <returns>Task</returns>
        Task DisconnectAsync(string connectionId);

        /// <summary>
        /// Gets the chunk with the specified index.
        /// This implies that after calling this method the
        /// client has loaded the chunk and is now interested
        /// in changes to that chunk.
        /// </summary>
        /// <param name="index">Chunk index</param>
        /// <returns>The loaded chunk</returns>
        Task<IChunk> LoadChunkAsync(string connectionId, Point index);

        /// <summary>
        /// Tells the server that the client has unloaded
        /// the chunk and is no longer interested in changes
        /// to that chunk.
        /// </summary>
        /// <param name="index">Chunk index</param>
        /// <returns>Task</returns>
        Task UnloadChunkAsync(string connectionId, Point index);

        /// <summary>
        /// Tries to execute a move on the server.
        /// </summary>
        /// <param name="move">Information about the move</param>
        /// <returns>Resulting information</returns>
        Task<RemoteMoveResult> MoveAsync(string connectionId, RemoteMoveRequest move);
    }
}
