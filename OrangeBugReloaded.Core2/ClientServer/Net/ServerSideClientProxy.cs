using System.Diagnostics;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Rpc;

namespace OrangeBugReloaded.Core.ClientServer.Net
{
    public class ServerSideClientProxy : IGameClientStub
    {
        private readonly RpcConnection _connection;

        public GameClientInfo PlayerInfo { get; }

        public ServerSideClientProxy(GameClientInfo clientInfo, RpcConnection connection)
        {
            PlayerInfo = clientInfo;
            _connection = connection;
        }

        public async Task OnUpdate(ClientUpdate e)
        {
            try
            {
                await _connection.Proxy.OnUpdate(e);
            }
            catch
            {
                Debugger.Break();
            }
        }
    }
}
