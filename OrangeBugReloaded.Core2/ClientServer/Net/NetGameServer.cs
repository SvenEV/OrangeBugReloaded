using System;
using System.Reflection;
using System.Threading.Tasks;
using UwpNetworkingEssentials;
using UwpNetworkingEssentials.Rpc;

namespace OrangeBugReloaded.Core.ClientServer.Net
{
    public class NetGameServer : GameServerBase
    {
        private RpcServer _rpcServer;

        public NetGameServer(IGameplayMap map) : base(map)
        {
        }

        public async Task StartAsync(string port)
        {
            var serializer = new DefaultJsonSerializer(typeof(NetGameServer).GetTypeInfo().Assembly, Serialization.JsonSerializationSettings);
            _rpcServer = await RpcServer.StartAsync(port, this, serializer);
        }

        protected override async Task<IGameClientStub> CreateClientStubAsync(IGameClientInfo clientInfo)
        {
            await Task.CompletedTask;

            var remoteClientInfo = clientInfo as NetGameClientInfo;

            if (remoteClientInfo == null)
                throw new ArgumentException();

            var connection = _rpcServer.Connections[remoteClientInfo.ConnectionId];

            // Create a proxy that forwards server-calls to the client via RPC
            return new ServerSideClientProxy(remoteClientInfo.PlayerId, remoteClientInfo.PlayerDisplayName, connection);
        }
    }
}
