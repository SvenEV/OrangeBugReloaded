using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.LocalSinglePlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using F = Windows.Foundation;

namespace OrangeBugReloaded.App
{
    public sealed partial class MainPage : Page
    {
        private const double _tileRenderSize = 50;
        
        private Map _map;
        private F.Point _cameraPosition = new F.Point(.5, .5);
        private PlayerEntity _player;

        private Dictionary<string, CanvasBitmap> _sprites = new Dictionary<string, CanvasBitmap>();

        public MainPage()
        {
            InitializeComponent();
            Window.Current.CoreWindow.KeyDown += OnKeyDown;
            Init();
        }

        private async void OnKeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case VirtualKey.Left:
                    await _player.TryMoveTowardsAsync(Point.West);
                    break;

                case VirtualKey.Right:
                    await _player.TryMoveTowardsAsync(Point.East);
                    break;

                case VirtualKey.Up:
                    await _player.TryMoveTowardsAsync(Point.North);
                    break;

                case VirtualKey.Down:
                    await _player.TryMoveTowardsAsync(Point.South);
                    break;
            }

            _cameraPosition = new F.Point(
                _player.Owner.Location.Position.X + .5f,
                _player.Owner.Location.Position.Y + .5f);
        }

        private async void Init()
        {
            _map = await Map.CreateAsync(InMemoryChunkStorage.CreateSampleWorld());

            _player = new PlayerEntity();
            var context = new EntityMoveContext(_map, _player);
            await _player.TryMoveAsync(Point.Zero, context);

            var box = new BoxEntity();
            context = new EntityMoveContext(_map, box);
            await box.TryMoveAsync(new Point(2, 2), context);
        }

        private void OnCanvasDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            var center = new Vector2((float)canvas.Size.Width / 2, (float)canvas.Size.Height / 2);

            foreach (var loc in _map.Locations.Select(o => o.Value))
            {
                var drawPosition = new F.Point(
                    center.X + _tileRenderSize * (loc.Position.X - _cameraPosition.X),
                    center.Y + _tileRenderSize * -(loc.Position.Y - _cameraPosition.Y + 1));

                if (loc.Tile != null)
                {
                    args.DrawingSession.DrawImage(
                        _sprites[loc.Tile.GetType().Name],
                        new F.Rect(drawPosition, new F.Size(_tileRenderSize, _tileRenderSize)));

                    if (loc.Tile.Entity != null)
                    {
                        args.DrawingSession.DrawImage(
                            _sprites[loc.Tile.Entity.GetType().Name],
                            new F.Rect(drawPosition, new F.Size(_tileRenderSize, _tileRenderSize)));
                    }
                }
            }

            args.DrawingSession.DrawCircle(center, 4, Colors.Black);
        }

        private void OnCanvasCreateResources(CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
            => args.TrackAsyncAction(LoadSpritesAsync().AsAsyncAction());

        private async Task LoadSpritesAsync()
        {
            _sprites.Clear();
            _sprites["PathTile"] = await CanvasBitmap.LoadAsync(canvas, new Uri("ms-appx:///Assets/Sprites/Path.png"));
            _sprites["WallTile"] = await CanvasBitmap.LoadAsync(canvas, new Uri("ms-appx:///Assets/Sprites/Wall.png"));
            _sprites["PlayerEntity"] = await CanvasBitmap.LoadAsync(canvas, new Uri("ms-appx:///Assets/Sprites/PlayerRight.png"));
            _sprites["BoxEntity"] = await CanvasBitmap.LoadAsync(canvas, new Uri("ms-appx:///Assets/Sprites/Box.png"));
        }
    }
}
