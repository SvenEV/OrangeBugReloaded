using System;

namespace OrangeBugReloaded.Core.ClientServer
{
    public class ClientConnectRequest
    {
        /// <summary>
        /// The unique player ID.
        /// TODO: How does this correspond to PlayerEntity.Id?
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// The name that is displayed for the player.
        /// </summary>
        public string PlayerDisplayName { get; }

        public ClientConnectRequest(string playerId, string playerDisplayName)
        {
            PlayerId = playerId;
            PlayerDisplayName = playerDisplayName;
        }
    }
}
