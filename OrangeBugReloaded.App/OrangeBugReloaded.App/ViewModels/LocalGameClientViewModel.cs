using OrangeBugReloaded.Core.ClientServer;
using OrangeBugReloaded.Core.ClientServer.Local;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OrangeBugReloaded.App.ViewModels
{
    public class LocalGameClientViewModel
    {
        private LocalGameServer _server;
        private LocalGameClient _client;

        public GameClientBase Client => _client;

        public LocalGameClientViewModel(LocalGameServer gameServer)
        {
            _server = gameServer;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var random = new Random();
                var number = random.Next(1, 100);
                var playerInfo = new GameClientInfo($"Player{number}", $"Player {number}");

                _client = new LocalGameClient(playerInfo);
                await _client.JoinAsync(_server);
            }
            catch
            {
                Debugger.Break();
            }
        }
    }
}
