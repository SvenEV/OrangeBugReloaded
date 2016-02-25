using System;
using System.Reflection;
using System.Threading.Tasks;
using UwpNetworkingEssentials;
using UwpNetworkingEssentials.Rpc;

namespace OrangeBugReloaded.Core.ClientServer.Net
{
    public class NetGameClient : GameClientBase
    {
        private RpcClient _rpcClient;

        public NetGameClient(GameClientInfo playerInfo) : base(playerInfo)
        {
        }

        public async Task JoinAsync(NetGameServerInfo serverInfo)
        {
            var serializer = new DefaultJsonSerializer(typeof(NetGameClient).GetTypeInfo().Assembly, Serialization.JsonSerializationSettings);
            _rpcClient = await RpcClient.ConnectAsync(serverInfo.RemoteAddress, serverInfo.RemotePort, this, serializer);

            JoinResult joinResult = await _rpcClient.Server.JoinAsync(PlayerInfo);
            var serverStub = new ClientSideServerProxy(_rpcClient.Connection);

            if (joinResult.IsSuccessful)
                await InitializeAsync(joinResult.SpawnPosition, serverStub);
            else
                throw new InvalidOperationException("Failed to join game server");
        }
    }
}
