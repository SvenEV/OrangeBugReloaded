using OrangeBugReloaded.Core.ClientServer;
using OrangeBugReloaded.Core.ClientServer.Net;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OrangeBugReloaded.App.ViewModels
{
    public class NetGameClientViewModel
    {
        private NetGameServerInfo _serverInfo;
        private NetGameClient _client;

        public GameClientBase Client => _client;

        public NetGameClientViewModel(NetGameServerInfo serverInfo)
        {
            _serverInfo = serverInfo;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var random = new Random();
                var number = random.Next(1, 100);
                var playerInfo = new GameClientInfo($"Player{number}", $"Player {number}");

                _client = new NetGameClient(playerInfo);
                await _client.JoinAsync(_serverInfo);
            }
            catch
            {
                Debugger.Break();
            }
        }
    }
}