using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer.Local
{
    public class LocalGameServer
    {
        private readonly GameServer _gameServer;

        public GameServer UnderlyingGameServer => _gameServer;

        public LocalGameServer(GameServer gameServer)
        {
            _gameServer = gameServer;
        }

        public Task<JoinResult> JoinAsync(LocalGameClient gameClient)
        {
            return _gameServer.JoinAsync(gameClient.PlayerInfo, gameClient);
        }
    }
}
