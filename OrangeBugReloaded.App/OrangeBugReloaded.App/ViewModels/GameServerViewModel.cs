using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.App.Common;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.ClientServer;
using OrangeBugReloaded.Core.ClientServer.Local;
using OrangeBugReloaded.Core.ClientServer.Net;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OrangeBugReloaded.App.ViewModels
{
    public class GameServerViewModel
    {
        private GameServer _gameServer;
        private LocalGameServer _localServer;
        private NetGameServer _netServer;
        private OrangeBugRenderer _renderer;

        public OrangeBugRenderer Renderer => _renderer;

        public LocalGameServer LocalServer => _localServer;

        public GameServerViewModel(CanvasAnimatedControl canvas)
        {
            _renderer = new OrangeBugRenderer();
            _renderer.Attach(canvas);
            Init();
        }

        private async void Init()
        {
            var storage = new InMemoryChunkStorage();
            storage.SaveAsync(new Point(0, 0), SampleChunks.Chunk1.Clone()).Wait();
            storage.SaveAsync(new Point(-1, 0), SampleChunks.Chunk2.Clone()).Wait();
            storage.SaveAsync(new Point(-1, -1), SampleChunks.Chunk3.Clone()).Wait();
            var map = new Map(storage);

            await map.GetAsync(Point.Zero);
            _renderer.Map = map;

            _gameServer = new GameServer(map);
            _localServer = new LocalGameServer(_gameServer);
        }

        public async Task OpenForNetworkAsync(string port)
        {
            if (_netServer != null)
                return;

            try
            {
                _netServer = new NetGameServer(_gameServer);
                await _netServer.StartAsync(port);
            }
            catch
            {
                Debugger.Break();
            }
        }

        public async Task CloseForNetworkAsync()
        {
            if (_netServer == null)
                return;

            await _netServer.DisposeAsync();
            _netServer = null;
        }
    }
}
