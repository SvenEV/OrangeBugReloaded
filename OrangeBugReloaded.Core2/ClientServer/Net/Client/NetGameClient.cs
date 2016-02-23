using System;
using System.Reflection;
using System.Threading.Tasks;
using UwpNetworkingEssentials;
using UwpNetworkingEssentials.Rpc;

namespace OrangeBugReloaded.Core.ClientServer.Net.Client
{
    public class NetGameClient : IGameClient
    {
        private readonly GameClient _localGame;
        private RpcClient _rpcClient;
        private ClientSideServerProxy _serverProxy;

        public Point PlayerPosition => _localGame.PlayerPosition;

        public string PlayerId => _localGame.PlayerId;

        public string PlayerDisplayName => _localGame.PlayerDisplayName;

        public IGameplayMap Map => _localGame.Map;

        public NetGameClient(GameClient localGame)
        {
            _localGame = localGame;
        }

        public Task OnUpdate(ClientUpdate e)
        {
            return _localGame.OnUpdate(e);
        }

        public async Task ConnectAsync(IGameServerStub server)
        {
            var serverInfo = server as ServerConnectInfo;

            if (serverInfo == null)
                throw new ArgumentException($"{nameof(ServerConnectInfo)} expected", nameof(server));

            var serializer = new DefaultJsonSerializer(typeof(NetGameClient).GetTypeInfo().Assembly);
            _rpcClient = await RpcClient.ConnectAsync(serverInfo.RemoteAddress, serverInfo.RemotePort, this, serializer);
            _serverProxy = new ClientSideServerProxy(_rpcClient.Connection);
            await _localGame.ConnectAsync(_serverProxy);
        }

        public async Task DisconnectAsync()
        {
            await _localGame.DisconnectAsync();
            // TODO: _rpcClient.DisconnectAsync(...)
        }

        public Task<bool> MoveAsync(Point sourcePosition, Point targetPosition)
        {
            return _localGame.MoveAsync(sourcePosition, targetPosition);
        }

        public Task<bool> MovePlayerAsync(Point direction)
        {
            return _localGame.MovePlayerAsync(direction);
        }
    }
}
