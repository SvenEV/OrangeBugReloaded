using System.Threading.Tasks;
using UwpNetworkingEssentials.Rpc;

namespace OrangeBugReloaded.Core.ClientServer.Net
{
    public class ServerSideClientProxy : IGameClientStub
    {
        private readonly RpcConnection _connection;

        public string PlayerDisplayName { get; }

        public string PlayerId { get; }

        public ServerSideClientProxy(string playerId, string playerDisplayName, RpcConnection connection)
        {
            PlayerId = playerId;
            PlayerDisplayName = playerDisplayName;
            _connection = connection;
        }

        public async Task OnUpdate(ClientUpdate e)
        {
            await _connection.Proxy.OnUpdate(e);
        }
    }
}
