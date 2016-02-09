using System.Numerics;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.Tiles;
using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.Core.Transactions;
using OrangeBugReloaded.Core.Entities;

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

        public Map Map { get; }

        public OrangeBugRenderer Renderer { get; }

        public MainViewModel(CanvasAnimatedControl canvas)
        {
            Map = new Map(new InMemoryChunkStorage(new[] { Chunk.SampleChunk, Chunk.SampleChunk2 }));

            Renderer = new OrangeBugRenderer();
            Renderer.Attach(canvas);
            Renderer.Plugins.Add<OrangeBugAudioPlayer>();
            Renderer.Map = Map;

            Map.ChunkLoader.GetAsync(Point.West);
        }

        public async Task EditMapAsync(Vector2 canvasPosition)
        {
            var t = TransactionChainWithEditSupport.Create<TransactionWithEditSupport>(Map);
            var gamePosition = Renderer.TransformCanvasPosition(canvasPosition);

            var meta = (SelectedTileTemplate is Tile) ?
                new TileMetadata((Tile)SelectedTileTemplate, 0) :
                new TileMetadata(Tile.Compose((await Map.GetMetadataAsync(gamePosition)).TileTemplate, (Entity)SelectedTileTemplate), 0);

            await t.SetMetadataAsync(gamePosition, meta);

            await t.CommitAsync(null);

            // Manual reset
            foreach (var kvp in t.CurrentTransaction.Changes)
            {
                await Map.SetAsync(kvp.Key, kvp.Value.TileTemplate);
            }
        }
    }
}
