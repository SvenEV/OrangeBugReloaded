using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.Core.ClientServer;
using OrangeBugReloaded.Core.ClientServer.Net;
using System;
using System.Diagnostics;

namespace OrangeBugReloaded.App.Common
{
    public class GameClientViewModel
    {
        private NetGameServerInfo _serverInfo;
        private NetGameClient _client;
        private OrangeBugRenderer _renderer;

        public IGameClient Client => _client;

        public OrangeBugRenderer Renderer => _renderer;

        public GameClientViewModel(CanvasAnimatedControl canvas, NetGameServerInfo serverInfo)
        {
            _renderer = new OrangeBugRenderer();
            _renderer.Attach(canvas);
            _serverInfo = serverInfo;
            Init();
        }

        private async void Init()
        {
            try
            {
                var random = new Random();
                var number = random.Next(1, 100);
                _client = new NetGameClient($"Player{number}", $"Player {number}");
                await _client.ConnectAsync(_serverInfo);

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