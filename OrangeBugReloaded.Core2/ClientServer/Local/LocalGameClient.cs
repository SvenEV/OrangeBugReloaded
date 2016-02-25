using System;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer.Local
{
    public class LocalGameClient : GameClientBase
    {
        public LocalGameClient(GameClientInfo playerInfo) : base(playerInfo)
        {
        }

        public async Task JoinAsync(LocalGameServer gameServer)
        {
            var joinResult = await gameServer.JoinAsync(this);

            if (joinResult.IsSuccessful)
                await InitializeAsync(joinResult.SpawnPosition, gameServer.UnderlyingGameServer);
            else
                throw new InvalidOperationException("Failed to join game server");
        }
    }
}
