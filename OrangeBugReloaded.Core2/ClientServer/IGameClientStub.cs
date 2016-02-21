using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer
{
    /// <summary>
    /// Provides methods a server uses to communicate with a client.
    /// </summary>
    public interface IGameClientStub
    {
        /// <summary>
        /// The unique player ID.
        /// TODO: How does this correspond to PlayerEntity.Id?
        /// </summary>
        string PlayerId { get; }

        /// <summary>
        /// The name that is displayed for the player.
        /// </summary>
        string PlayerDisplayName { get; }

        /// <summary>
        /// Sends updated tiles and events to the client.
        /// </summary>
        /// <param name="e">Update arguments</param>
        /// <returns></returns>
        Task OnUpdate(ClientUpdate e);
    }
}
