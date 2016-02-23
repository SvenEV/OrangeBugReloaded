using System;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer.Net.Client
{
    public class ServerConnectInfo : IGameServerStub
    {
        public string RemoteAddress { get; }
        public string RemotePort { get; }

        public ServerConnectInfo(string remoteAddress, string remotePort)
        {
            RemoteAddress = remoteAddress;
            RemotePort = remotePort;
        }

        Task<ConnectResult> IGameServerStub.ConnectAsync(IGameClientStub client, object playerId)
        {
            throw new NotSupportedException();
        }

        Task IGameServerStub.DisconnectAsync(object playerId)
        {
            throw new NotSupportedException();
        }

        Task<IChunk> IGameServerStub.LoadChunkAsync(Point index, object playerId)
        {
            throw new NotSupportedException();
        }

        Task<RemoteMoveResult> IGameServerStub.MoveAsync(RemoteMoveRequest move, object playerId)
        {
            throw new NotSupportedException();
        }

        Task IGameServerStub.UnloadChunkAsync(Point index, object playerId)
        {
            throw new NotSupportedException();
        }
    }
}
