using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.App.Presentation;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.ClientServer;
using OrangeBugReloaded.Core.ClientServer.Net;
using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Events;
using System;
using System.Diagnostics;
using System.Reactive.Linq;

namespace OrangeBugReloaded.App.ViewModels
{
    public class NetGameClientViewModel
    {
        private NetGameServerInfo _serverInfo;
        private NetGameClient _client;
        private OrangeBugRenderer _renderer;

        public GameClientBase Client => _client;

        public OrangeBugRenderer Renderer => _renderer;

        public NetGameClientViewModel(CanvasAnimatedControl canvas, NetGameServerInfo serverInfo)
        {
            _renderer = new OrangeBugRenderer { Canvas = canvas };
            _serverInfo = serverInfo;
            Init();
        }

        private async void Init()
        {
            try
            {
                var random = new Random();
                var number = random.Next(1, 100);
                var playerInfo = new GameClientInfo($"Player{number}", $"Player {number}");

                _client = new NetGameClient(playerInfo);

                _client.Events.OfType<EntityMoveEvent>()
                    .Where(e => (e.Source.Entity as PlayerEntity)?.PlayerId == Client.PlayerInfo.PlayerId)
                    .Select(e => e.TargetPosition)
                    .Subscribe(OnPlayerPositionChanged);

                await _client.JoinAsync(_serverInfo);

                _renderer.Map = _client.Map;
                Renderer.CameraPosition = _client.PlayerPosition.ToVector2();
            }
            catch
            {
                Debugger.Break();
            }
        }

        private void OnPlayerPositionChanged(Point newPosition)
        {
            Renderer.CameraPosition = newPosition.ToVector2();
        }
    }
}