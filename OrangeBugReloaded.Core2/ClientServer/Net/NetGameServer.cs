﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UwpNetworkingEssentials;
using UwpNetworkingEssentials.Rpc;

namespace OrangeBugReloaded.Core.ClientServer.Net
{
    public class NetGameServer : IRpcTarget
    {
        // Maps StreamSocketConnection IDs to player information
        private readonly Dictionary<string, GameClientInfo> _players = new Dictionary<string, GameClientInfo>();

        private readonly GameServer _gameServer;
        private RpcServer _rpcServer;
        private bool _isDisposed = false;

        public GameServer UnderlyingGameServer => _gameServer;

        public NetGameServer(GameServer gameServer)
        {
            _gameServer = gameServer;
        }

        public async Task StartAsync(string port)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(NetGameServer));

            if (_rpcServer != null)
                throw new InvalidOperationException();

            var serializer = new DefaultJsonSerializer(typeof(NetGameServer).GetTypeInfo().Assembly, Serialization.JsonSerializationSettings);
            _rpcServer = await RpcServer.StartAsync(port, this, serializer);
        }

        public async Task DisposeAsync()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                if (_rpcServer != null)
                {
                    // Closing the server will disconnect all clients
                    // => OnDisconnected will correctly remove players from the game
                    await _rpcServer.DisposeAsync();
                    _rpcServer = null;
                }
            }
        }


        // RPC methods
        // These are similar to those in IGameServerStub but tailored to RPC connections.

        public async Task<JoinResult> JoinAsync(GameClientInfo clientInfo, [RpcCaller]RpcConnection caller)
        {
            // Create a proxy that forwards server-calls to the client via RPC
            var clientStub = new ServerSideClientProxy(clientInfo, caller);
            var joinResult = await _gameServer.JoinAsync(clientInfo, clientStub);

            if (joinResult.IsSuccessful)
                _players.Add(caller.Id, clientInfo);

            return joinResult;
        }

        public async Task LeaveAsync([RpcCaller]RpcConnection caller)
        {
            var playerInfo = _players[caller.Id];
            _players.Remove(caller.Id);
            await _gameServer.LeaveAsync(playerInfo.PlayerId);
        }

        public Task<IChunk> LoadChunkAsync(Point index, [RpcCaller]RpcConnection caller)
            => _gameServer.LoadChunkAsync(index, _players[caller.Id].PlayerId);

        public Task UnloadChunkAsync(Point index, [RpcCaller]RpcConnection caller)
            => _gameServer.UnloadChunkAsync(index, _players[caller.Id].PlayerId);

        public Task<RemoteMoveResult> MoveAsync(RemoteMoveRequest move, [RpcCaller]RpcConnection caller)
            => _gameServer.MoveAsync(move, _players[caller.Id].PlayerId);

        public Task<bool> ResetRegionAsync([RpcCaller]RpcConnection caller)
            => _gameServer.ResetRegionAsync(_players[caller.Id].PlayerId);


        // RPC events

        public void OnConnected(RpcConnection connection)
        {
        }

        public void OnConnectionAttemptFailed(RpcConnectionAttemptFailedException exception)
        {
        }

        public async void OnDisconnected(RpcConnection connection)
        {
            GameClientInfo playerInfo;

            if (_players.TryGetValue(connection.Id, out playerInfo))
            {
                // Connection unexpectedly closed without correctly leaving the game
                await _gameServer.LeaveAsync(playerInfo.PlayerId);
                _players.Remove(connection.Id);
            }
        }
    }
}
