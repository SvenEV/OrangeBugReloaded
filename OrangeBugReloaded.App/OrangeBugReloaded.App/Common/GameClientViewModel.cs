using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.Core.ClientServer;
using OrangeBugReloaded.Core.ClientServer.Net.Client;
using System;
using System.Diagnostics;

namespace OrangeBugReloaded.App.Common
{
    public class GameClientViewModel
    {
        private ServerConnectInfo _serverInfo;
        private NetGameClient _client;
        private OrangeBugRenderer _renderer;

        public IGameClient Client => _client;

        public OrangeBugRenderer Renderer => _renderer;

        public GameClientViewModel(CanvasAnimatedControl canvas, ServerConnectInfo serverInfo)
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
                var game = new GameClient("Player1", "Player 1");
                _client = new NetGameClient(game);
                await _client.ConnectAsync(_serverInfo);
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }
    }
}