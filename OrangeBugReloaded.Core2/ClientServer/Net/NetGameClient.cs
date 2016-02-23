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

        public NetGameClient(string playerId, string playerDisplayName) : base(playerId, playerDisplayName)
        {
        }

        protected override async Task<IGameClientInfo> CreateClientInfoAsync(IGameServerInfo serverInfo)
        {
            var remoteServerInfo = serverInfo as NetGameServerInfo;

            if (remoteServerInfo == null)
                throw new ArgumentException();

            var serializer = new DefaultJsonSerializer(typeof(NetGameClient).GetTypeInfo().Assembly, Serialization.JsonSerializationSettings);
            _rpcClient = await RpcClient.ConnectAsync(remoteServerInfo.RemoteAddress, remoteServerInfo.RemotePort, this, serializer);

            return new NetGameClientInfo(_rpcClient.Connection.Id, PlayerId, PlayerDisplayName);
        }

        protected override async Task<IGameServerStub> CreateServerStubAsync(IGameServerInfo serverInfo)
        {
            await Task.CompletedTask;
            return new ClientSideServerProxy(_rpcClient.Connection);
        }
    }
}
