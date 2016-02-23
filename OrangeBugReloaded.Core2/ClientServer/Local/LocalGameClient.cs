using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer.Local
{
    public class LocalGameClient : GameClientBase
    {
        public LocalGameClient(string playerId, string playerDisplayName) : base(playerId, playerDisplayName)
        {
        }

        protected override async Task<IGameClientInfo> CreateClientInfoAsync(IGameServerInfo serverInfo)
        {
            await Task.CompletedTask;
            return new LocalGameClientInfo(this, PlayerId, PlayerDisplayName);
        }

        protected override async Task<IGameServerStub> CreateServerStubAsync(IGameServerInfo serverInfo)
        {
            await Task.CompletedTask;
            return (serverInfo as LocalGameServerInfo).Server;
        }
    }
}
