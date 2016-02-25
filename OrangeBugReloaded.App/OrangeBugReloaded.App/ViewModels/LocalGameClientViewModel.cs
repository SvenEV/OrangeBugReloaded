using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.App.Common;
using OrangeBugReloaded.Core.ClientServer;
using OrangeBugReloaded.Core.ClientServer.Local;
using System;
using System.Diagnostics;

namespace OrangeBugReloaded.App.ViewModels
{
    public class LocalGameClientViewModel
    {
        private LocalGameServer _server;
        private LocalGameClient _client;
        private OrangeBugRenderer _renderer;

        public GameClientBase Client => _client;

        public OrangeBugRenderer Renderer => _renderer;

        public LocalGameClientViewModel(CanvasAnimatedControl canvas, LocalGameServer gameServer)
        {
            _renderer = new OrangeBugRenderer();
            _renderer.Attach(canvas);
            _server = gameServer;
            Init();
        }

        private async void Init()
        {
            try
            {
                var random = new Random();
                var number = random.Next(1, 100);
                var playerInfo = new GameClientInfo($"Player{number}", $"Player {number}");

                _client = new LocalGameClient(playerInfo);
                await _client.JoinAsync(_server);

                _renderer.Map = _client.Map;
                Renderer.CameraPosition = _client.PlayerPosition.ToVector2();
            }
            catch
            {
                Debugger.Break();
            }
        }
    }
}
