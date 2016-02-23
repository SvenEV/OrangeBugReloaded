using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace OrangeBugReloaded.Core.ClientServer
{
    public class RemoteMoveResult
    {
        /// <summary>
        /// Indicates whether the move request has been acknowledged by the server.
        /// If true, the client commits its pending local transaction and increases
        /// the version of affected tiles to match the new version number assigned
        /// by the server.
        /// If false, the client cancels its pending transaction and uses the
        /// provided <see cref="ChunkUpdates"/> (if any) to become up-to-date.
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        /// The version number that the client assigns to the affected
        /// tiles of the transaction if the move is successful.
        /// The value is -1 if <see cref="IsSuccessful"/> is false
        /// or the move yielded no tile changes.
        /// </summary>
        public int NewVersion { get; }

        /// <summary>
        /// If the move is faulted, this contains up-to-date versions
        /// of chunks that have been detected as outdated on the client
        /// This is empty if the move is successful.
        /// </summary>
        public IReadOnlyCollection<KeyValuePair<Point, IChunk>> ChunkUpdates { get; }

        [JsonConstructor]
        private RemoteMoveResult(bool isSuccessful, int newVersion, IEnumerable<KeyValuePair<Point, IChunk>> chunkUpdates)
        {
            IsSuccessful = isSuccessful;
            NewVersion = newVersion;
            ChunkUpdates = chunkUpdates.ToArray();
        }

        public static RemoteMoveResult CreateSuccessful(int newVersion)
            => new RemoteMoveResult(true, newVersion, new KeyValuePair<Point, IChunk>[0]);

        public static RemoteMoveResult CreateFaulted(IEnumerable<KeyValuePair<Point, IChunk>> chunkUpdates)
            => new RemoteMoveResult(false, -1, chunkUpdates.ToArray());
    }
}
