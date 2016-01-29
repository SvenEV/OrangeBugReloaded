using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using F = Windows.Foundation;

namespace OrangeBugReloaded
{
    public class OrangeBugRenderer
    {
        private const float _zoomLevelDamping = 10;
        private const float _cameraPositionDamping = 2;

        private Dictionary<string, CanvasBitmap> _sprites = new Dictionary<string, CanvasBitmap>();
        private CanvasAnimatedControl _canvas;
        private Map _map;
        private IDisposable _mapEventSubscription;
        private float _currentZoomLevel = 50;
        private Vector2 _currentCameraPosition = Vector2.Zero;

        public Map Map
        {
            get { return _map; }
            set
            {
                if (!Equals(_map, value))
                {
                    _mapEventSubscription?.Dispose();
                    _map = value;
                    _mapEventSubscription = _map?.Events.Subscribe(OnEvent);
                    Plugins.OnMapChanged(value);
                }
            }
        }

        public PluginCollection Plugins { get; }

        public float ZoomLevel { get; set; }

        public Vector2 CameraPosition { get; set; }

        public OrangeBugRenderer()
        {
            Plugins = new PluginCollection(this);
            ZoomLevel = _currentZoomLevel;
            CameraPosition = _currentCameraPosition;
        }

        public void Attach(CanvasAnimatedControl canvas)
        {
            _canvas = canvas;
            _canvas.Draw += OnDraw;
            _canvas.CreateResources += OnCreateResources;
        }

        public void Detach()
        {
            _canvas.Draw -= OnDraw;
            _canvas.CreateResources -= OnCreateResources;
            _canvas = null;
        }

        private void OnEvent(IGameEvent e)
        {
            System.Diagnostics.Debug.WriteLine(e.ToString());
        }

        private void OnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            if (Map == null)
                return;

            var g = args.DrawingSession;

            // Interpolate towards target values for ZoomLevel and CameraPosition
            var deltaTime = (float)args.Timing.ElapsedTime.TotalSeconds;
            _currentZoomLevel = Mathf.Lerp(_currentZoomLevel, Mathf.Clamp(ZoomLevel, 1, 1000), _zoomLevelDamping * deltaTime);
            _currentCameraPosition = Vector2.Lerp(_currentCameraPosition, CameraPosition, _cameraPositionDamping * deltaTime);

            foreach (var chunk in Map.ChunkLoader.Chunks.Values)
            {
                for (var y = 0; y < Chunk.Size; y++)
                {
                    for (var x = 0; x < Chunk.Size; x++)
                    {
                        var tile = chunk[x, y, MapLayer.Gameplay];

                        var tilePosition = chunk.Index * Chunk.Size + new Point(x, y);

                        var rect = new F.Rect(
                            (tilePosition.X - _currentCameraPosition.X - .5) * _currentZoomLevel + sender.Size.Width / 2,
                            -(tilePosition.Y - _currentCameraPosition.Y + .5) * _currentZoomLevel + sender.Size.Height / 2,
                            _currentZoomLevel,
                            _currentZoomLevel);

                        var sprite = _sprites.TryGetValue(VisualHintAttribute.GetVisualName(tile), _sprites["NoSprite"]);
                        g.DrawImage(sprite, rect);

                        if (tile.Entity != Entity.None)
                        {
                            var entitySprite = _sprites.TryGetValue(VisualHintAttribute.GetVisualName(tile.Entity), _sprites["NoSprite"]);
                            g.DrawImage(entitySprite, rect);
                        }
                    }
                }
            }
        }

        private void OnCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            if (args.Reason != CanvasCreateResourcesReason.DpiChanged)
                args.TrackAsyncAction(LoadSpritesAsync(sender).AsAsyncAction());
        }

        private async Task LoadSpritesAsync(ICanvasResourceCreatorWithDpi resourceCreator)
        {
            var loadSpriteAsync = new Func<string, F.IAsyncOperation<CanvasBitmap>>(s => CanvasBitmap.LoadAsync(resourceCreator, new Uri($"ms-appx:///Assets/Sprites/{s}.png")));

            _sprites["NoSprite"] = await loadSpriteAsync("NoSprite");

            // Tile sprites
            _sprites["PathTile"] = await loadSpriteAsync("Path");
            _sprites["WallTile"] = await loadSpriteAsync("Wall");
            _sprites["TeleporterTile"] = await loadSpriteAsync("Teleport");
            _sprites["GateTile-Open"] = await loadSpriteAsync("DummyWallRemoved");
            _sprites["GateTile-Closed"] = await loadSpriteAsync("DummyWall");
            _sprites["ButtonTile-Entities-On"] = await loadSpriteAsync("SensitiveButton");
            _sprites["ButtonTile-Entities-Off"] = await loadSpriteAsync("SensitiveButton");
            _sprites["ButtonTile-EntitiesExceptPlayer-On"] = await loadSpriteAsync("Button");
            _sprites["ButtonTile-EntitiesExceptPlayer-Off"] = await loadSpriteAsync("Button");
            _sprites["ButtonTile-Player-On"] = await loadSpriteAsync("SensitiveButton");
            _sprites["ButtonTile-Player-Off"] = await loadSpriteAsync("SensitiveButton");
            _sprites["PinTile-Red"] = await loadSpriteAsync("RedPool");
            _sprites["PinTile-Green"] = await loadSpriteAsync("GreenPool");
            _sprites["PinTile-Blue"] = await loadSpriteAsync("BluePool");
            _sprites["InkTile-Red"] = await loadSpriteAsync("RedInk");
            _sprites["InkTile-Green"] = await loadSpriteAsync("GreenInk");
            _sprites["InkTile-Blue"] = await loadSpriteAsync("BlueInk");

            // Entity sprites
            _sprites["PlayerEntity"] = await loadSpriteAsync("PlayerRight");
            _sprites["BoxEntity"] = await loadSpriteAsync("Box");
            _sprites["BalloonEntity-Red"] = await loadSpriteAsync("RedBall");
            _sprites["BalloonEntity-Green"] = await loadSpriteAsync("GreenBall");
            _sprites["BalloonEntity-Blue"] = await loadSpriteAsync("BlueBall");
        }
    }
}
