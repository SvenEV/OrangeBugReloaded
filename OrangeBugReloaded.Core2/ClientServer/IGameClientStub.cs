using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer
{
    /// <summary>
    /// Provides methods a server uses to communicate with a client.
    /// </summary>
    public interface IGameClientStub
    {
        GameClientInfo PlayerInfo { get; }

        /// <summary>
        /// Sends updated tiles and events to the client.
        /// </summary>
        /// <param name="e">Update arguments</param>
        /// <returns></returns>
        Task OnUpdate(ClientUpdate e);
    }
}
