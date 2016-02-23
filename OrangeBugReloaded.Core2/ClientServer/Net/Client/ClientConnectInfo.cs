using System;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer.Net.Client
{
    public class ClientConnectInfo : IGameClientStub
    {
        public string PlayerDisplayName { get; }

        public string PlayerId { get; }

        public ClientConnectInfo(string playerId, string playerDisplayName)
        {
            PlayerId = playerId;
            PlayerDisplayName = playerDisplayName;
        }

        Task IGameClientStub.OnUpdate(ClientUpdate e)
        {
            throw new NotSupportedException();
        }
    }
}
