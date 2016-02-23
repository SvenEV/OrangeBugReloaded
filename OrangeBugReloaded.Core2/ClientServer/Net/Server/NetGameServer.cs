using OrangeBugReloaded.Core.ClientServer.Net.Client;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UwpNetworkingEssentials;
using UwpNetworkingEssentials.Rpc;

namespace OrangeBugReloaded.Core.ClientServer.Net.Server
{
    public class NetGameServer : IGameServerStub
    {
        private RpcServer _rpcServer;
        private IGameServer _localGame;
        private readonly Dictionary<string, ServerSideClientProxy> _connections = new Dictionary<string, ServerSideClientProxy>();

        private NetGameServer()
        {
        }

        public static async Task<NetGameServer> StartAsync(string port, IGameServer localGame)
        {
            var server = new NetGameServer();
            server._localGame = localGame;

            var serializer = new DefaultJsonSerializer(typeof(NetGameServer).GetTypeInfo().Assembly);
            server._rpcServer = await RpcServer.StartAsync(port, server, serializer);

            return server;
        }

        public async Task<ConnectResult> ConnectAsync(IGameClientStub client, [RpcCaller]object caller)
        {
            var clientInfo = client as ClientConnectInfo;

            if (clientInfo == null)
                throw new ArgumentException($"{nameof(ClientConnectInfo)} expected", nameof(client));

            // Create a proxy that forwards calls server-calls to the client via RPC
            var clientProxy = new ServerSideClientProxy(clientInfo.PlayerId, clientInfo.PlayerDisplayName, (RpcConnection)caller);

            _connections.Add(((RpcConnection)caller).Id, clientProxy);

            var result = await _localGame.ConnectAsync(clientProxy, clientProxy.PlayerId);
            return result;
        }

        public async Task DisconnectAsync([RpcCaller]object caller)
        {
            await _localGame.DisconnectAsync(GetProxy(caller).PlayerId);
        }

        public Task<IChunk> LoadChunkAsync(Point index, [RpcCaller]object caller)
        {
            return _localGame.LoadChunkAsync(index, GetProxy(caller).PlayerId);
        }

        public Task UnloadChunkAsync(Point index, [RpcCaller]object caller)
        {
            return _localGame.UnloadChunkAsync(index, GetProxy(caller).PlayerId);
        }

        public Task<RemoteMoveResult> MoveAsync(RemoteMoveRequest move, [RpcCaller]object caller)
        {
            return _localGame.MoveAsync(move, GetProxy(caller).PlayerId);
        }

        private ServerSideClientProxy GetProxy(object caller)
        {
            var connectionId = (caller as RpcConnection)?.Id;
            ServerSideClientProxy proxy;

            if (string.IsNullOrEmpty(connectionId) || !_connections.TryGetValue(connectionId, out proxy))
                throw new ArgumentException();

            return proxy;
        }
    }
}
