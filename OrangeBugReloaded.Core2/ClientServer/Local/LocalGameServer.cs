using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer.Local
{
    public class LocalGameServer : GameServerBase
    {
        public LocalGameServer(IGameplayMap map) : base(map)
        {
        }

        protected override async Task<IGameClientStub> CreateClientStubAsync(IGameClientInfo clientInfo)
        {
            await Task.CompletedTask;
            return (clientInfo as LocalGameClientInfo)?.Client;
        }
    }
}
