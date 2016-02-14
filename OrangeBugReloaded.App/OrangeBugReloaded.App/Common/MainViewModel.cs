using System.Numerics;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.Tiles;
using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.Core.Entities;
using System;
using OrangeBugReloaded.Core.ClientServer;

namespace OrangeBugReloaded.App.Common
{
    public class MainViewModel : ViewModelBase
    {
        private object _selectedTileTemplate;

        public object[] TileTemplates { get; } = new object[]
        {
            PathTile.Default,
            WallTile.Default,
            new ButtonTile(false, EntityFilterMode.Entities),
            new InkTile(InkColor.Red),
            new InkTile(InkColor.Green),
            new InkTile(InkColor.Blue),
            new PinTile(InkColor.Red),
            new PinTile(InkColor.Green),
            new PinTile(InkColor.Blue),
            BoxEntity.Default,
            new BalloonEntity(InkColor.Red),
            new BalloonEntity(InkColor.Green),
            new BalloonEntity(InkColor.Blue),
        };

        public object SelectedTileTemplate
        {
            get { return _selectedTileTemplate; }
            set { Set(ref _selectedTileTemplate, value); }
        }

        public IGameClient Client1 { get; }
        public IGameClient Client2 { get; }
        public IGameServer Server { get; }

        public OrangeBugRenderer RendererClient1 { get; }
        public OrangeBugRenderer RendererClient2 { get; }
        public OrangeBugRenderer RendererServer { get; }

        public MainViewModel(CanvasAnimatedControl canvasClient1, CanvasAnimatedControl canvasClient2, CanvasAnimatedControl canvasServer)
        {
            var storage = new InMemoryChunkStorage();
            storage.SaveAsync(new Point(0, 0), Chunk.SampleChunk.Clone()).Wait();
            storage.SaveAsync(new Point(-1, 0), Chunk.SampleChunk2.Clone()).Wait();
            var map = new Map(storage);

            Server = new DelayedServerFacade(new GameServer(map));
            Client1 = new GameClient("player1", "Player A");
            Client2 = new GameClient("player2", "Player B");

            RendererClient1 = new OrangeBugRenderer();
            RendererClient1.Attach(canvasClient1);
            RendererClient1.Plugins.Add<OrangeBugAudioPlayer>();
            RendererClient1.Map = Client1.Map;

            RendererClient2 = new OrangeBugRenderer();
            RendererClient2.Attach(canvasClient2);
            RendererClient2.Plugins.Add<OrangeBugAudioPlayer>();
            RendererClient2.Map = Client2.Map;

            RendererServer = new OrangeBugRenderer();
            RendererServer.Attach(canvasServer);
            RendererServer.Map = Server.Map;

            Client1.ConnectAsync(Server);
            Client2.ConnectAsync(Server);
        }

        public async Task EditMapAsync(Vector2 canvasPosition)
        {
            /*var t = TransactionChainWithEditSupport.Create<TransactionWithEditSupport>(Map);
            var gamePosition = RendererClient.TransformCanvasPosition(canvasPosition);

            var meta = (SelectedTileTemplate is Tile) ?
                new TileMetadata((Tile)SelectedTileTemplate, 0) :
                new TileMetadata(Tile.Compose((await Map.GetMetadataAsync(gamePosition)).TileTemplate, (Entity)SelectedTileTemplate), 0);

            await t.SetMetadataAsync(gamePosition, meta);

            await t.CommitAsync(null);*/

            // Manual reset
            // TODO: Handle versioning correctly
            throw new NotImplementedException();
            //foreach (var kvp in t.CurrentTransaction.Changes)
            //{
            //    await Map.SetAsync(kvp.Key, kvp.Value.TileTemplate);
            //}
        }
    }
}
