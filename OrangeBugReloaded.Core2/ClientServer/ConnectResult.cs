using Newtonsoft.Json;

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
        /// The position on the map where the player has been spawned.
        /// </summary>
        public Point SpawnPosition { get; }

        /// <summary>
        /// A message describing the result.
        /// </summary>
        public string Message { get; }

        [JsonConstructor]
        public ConnectResult(bool isSuccessful, Point spawnPosition, string message = "")
        {
            IsSuccessful = isSuccessful;
            SpawnPosition = spawnPosition;
            Message = message;
        }
    }
}
