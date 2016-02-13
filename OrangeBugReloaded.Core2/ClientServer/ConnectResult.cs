namespace OrangeBugReloaded.Core.ClientServer
{
    public struct ConnectResult
    {
        /// <summary>
        /// True iff the connection is established and the player
        /// has successfully spawned on the map.
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        /// The connection ID that has been assigned to the client.
        /// </summary>
        public string ConnectionId { get; }

        /// <summary>
        /// The position on the map where the player has been spawned.
        /// </summary>
        public Point SpawnPosition { get; }

        /// <summary>
        /// A message describing the result.
        /// </summary>
        public string Message { get; }

        public ConnectResult(bool isSuccessful, string connectionId, Point spawnPosition, string message = "")
        {
            IsSuccessful = isSuccessful;
            ConnectionId = connectionId;
            SpawnPosition = spawnPosition;
            Message = message;
        }
    }
}
