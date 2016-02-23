using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.ClientServer;
using OrangeBugReloaded.Core.ClientServer.Net.Server;
using System;
using System.Diagnostics;

namespace OrangeBugReloaded.App.Common
{
    public class GameServerViewModel
    {
        private NetGameServer _server;
        private OrangeBugRenderer _renderer;

        public OrangeBugRenderer Renderer => _renderer;

        public GameServerViewModel(CanvasAnimatedControl canvas, string port)
        {
            _renderer = new OrangeBugRenderer();
            _renderer.Attach(canvas);
            Init(port);
        }

        private async void Init(string port)
        {
            try
            {
                var storage = new InMemoryChunkStorage();
                storage.SaveAsync(new Point(0, 0), SampleChunks.Chunk1.Clone()).Wait();
                storage.SaveAsync(new Point(-1, 0), SampleChunks.Chunk2.Clone()).Wait();
                storage.SaveAsync(new Point(-1, -1), SampleChunks.Chunk3.Clone()).Wait();
                var map = new Map(storage);

                await map.GetAsync(Point.Zero);
                _renderer.Map = map;

                var game = new GameServer(map);
                _server = await NetGameServer.StartAsync(port, game);
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
        }
    }
}
