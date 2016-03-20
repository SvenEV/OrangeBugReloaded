using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.ClientServer;
using OrangeBugReloaded.Core.ClientServer.Local;
using OrangeBugReloaded.Core.ClientServer.Net;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace OrangeBugReloaded.App.ViewModels
{
    public class GameServerViewModel
    {
        private GameServer _gameServer;

        public LocalGameServer LocalServer { get; private set; }
        public NetGameServer NetServer { get; private set; }

        public GameServerViewModel()
        {
        }

        public async Task InitializeAsync()
        {
            var storage = new InMemoryChunkStorage(SampleChunks.MapMetadata);
            await storage.SaveAsync(new Point(0, 0), SampleChunks.Chunk1.Clone());
            await storage.SaveAsync(new Point(-1, 0), SampleChunks.Chunk2.Clone());
            await storage.SaveAsync(new Point(-1, -1), SampleChunks.Chunk3.Clone());

            var map = new Map(storage);
            await map.GetAsync(Point.Zero);

            _gameServer = new GameServer(map);
            LocalServer = new LocalGameServer(_gameServer);
        }

        public async Task OpenForNetworkAsync(string port)
        {
            if (NetServer != null)
                return;

            try
            {
                NetServer = new NetGameServer(_gameServer);
                await NetServer.StartAsync(port);
            }
            catch
            {
                Debugger.Break();
            }
        }

        public async Task CloseForNetworkAsync()
        {
            if (NetServer == null)
                return;

            await NetServer.DisposeAsync();
            NetServer = null;
        }
    }
}
