using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.App.Common;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.UI;
using F = Windows.Foundation;

namespace OrangeBugReloaded.App.Presentation
{
    public class OrangeBugRenderer
    {
        private const float _zoomLevelDamping = 10;
        private const float _cameraPositionDamping = 2;

        private static readonly Dictionary<Point, float> _radiansForDirection = new Dictionary<Point, float>
        {
            { Point.North, 0 },
            { Point.West, Mathf.PI * 3 / 2 },
            { Point.South, Mathf.PI },
            { Point.East, Mathf.PI / 2 },
        };

        private static CanvasTextFormat _textFormat = new CanvasTextFormat
        {
            HorizontalAlignment = CanvasHorizontalAlignment.Center,
            VerticalAlignment = CanvasVerticalAlignment.Center,
            FontSize = 8
        };

        private readonly Dictionary<string, CanvasBitmap> _sprites = new Dictionary<string, CanvasBitmap>();
        private readonly List<EntityInfo> _entities = new List<EntityInfo>();
        private readonly object _entitiesLock = new object();
        private CanvasAnimatedControl _canvas;
        private IGameplayMap _map;
        private float _currentZoomLevel = 40;
        private Vector2 _currentCameraPosition = Vector2.Zero;
        private IDisposable _spawnSubscription;
        private IDisposable _moveSubscription;
        private string _followedPlayerId;

        public IGameplayMap Map
        {
            get { return _map; }
            set
            {
                if (!Equals(_map, value))
                {
                    DetachMap(_map);
                    _map = value;
                    AttachMap(value);
                    Plugins.OnMapChanged(value);
                }
            }
        }

        public CanvasAnimatedControl Canvas
        {
            get { return _canvas; }
            set
            {
                if (!Equals(_canvas, value))
                {
                    DetachCanvas(_canvas);
                    _canvas = value;
                    AttachCanvas(value);
                }
            }
        }

        public PluginCollection Plugins { get; }

        public IReadOnlyDictionary<string, CanvasBitmap> Sprites => _sprites;

        public bool DisplayDebugInfo { get; set; } = false;

        public float ZoomLevel { get; set; }

        public Vector2 CameraPosition { get; set; }

        public string FollowedPlayerId
        {
            get { return _followedPlayerId; }
            set
            {
                _followedPlayerId = value;
                MoveToPlayer(value);
            }
        }

        public OrangeBugRenderer()
        {
            Plugins = new PluginCollection(this);
            ZoomLevel = _currentZoomLevel;
            CameraPosition = _currentCameraPosition;
        }

        private void AttachCanvas(CanvasAnimatedControl canvas)
        {
            if (canvas == null)
                return;

            _canvas = canvas;
            _canvas.Draw += OnDraw;
            _canvas.CreateResources += OnCreateResources;
        }

        private void DetachCanvas(CanvasAnimatedControl canvas)
        {
            if (canvas == null)
                return;

            canvas.Draw -= OnDraw;
            canvas.CreateResources -= OnCreateResources;
        }

        private void AttachMap(IGameplayMap map)
        {
            if (map == null)
                return;

            _spawnSubscription = map.Events.OfType<EntitySpawnEvent>().Subscribe(OnEntitySpawned);
            _moveSubscription = map.Events.OfType<EntityMoveEvent>()
                .Where(e => (e.Source.Entity as PlayerEntity)?.PlayerId == FollowedPlayerId && FollowedPlayerId != null)
                .Subscribe(OnFollowedPlayerMoved);

            var chunkBounds = new Rectangle(0, 0, Chunk.Size - 1, Chunk.Size - 1);

            // Add the entities of the already loaded chunks
            // (if after that a chunk is loaded or unloaded the map will
            // emit appropiate spawn/despawn events)
            var entitySpawnEvents = map.ChunkLoader.Chunks
                .SelectMany(chunk => chunkBounds.Select(p => new { Position = chunk.Key * Chunk.Size + p, Entity = chunk.Value[p].Tile.Entity }))
                .Where(o => o.Entity != Entity.None)
                .Select(o => new EntitySpawnEvent(o.Position, o.Entity));

            foreach (var e in entitySpawnEvents)
                OnEntitySpawned(e);
        }

        private void DetachMap(IGameplayMap map)
        {
            if (map == null)
                return;

            _spawnSubscription?.Dispose();
            _spawnSubscription = null;

            _moveSubscription?.Dispose();
            _moveSubscription = null;

            lock (_entitiesLock)
            {
                foreach (var e in _entities)
                    e.Dispose();

                _entities.Clear();
            }
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

            // Draw tiles
            foreach (var kvp in Map.ChunkLoader.Chunks.ToArray())
            {
                for (var y = 0; y < Chunk.Size; y++)
                {
                    for (var x = 0; x < Chunk.Size; x++)
                    {
                        var tileInfo = kvp.Value[x, y];

                        var position = kvp.Key * Chunk.Size + new Point(x, y);
                        DrawSprite(g, tileInfo.Tile, position.ToVector2());

                        if (DisplayDebugInfo)
                        {
                            // Draw tile version for testing purposes
                            var textPosition = TransformGamePosition(position.ToVector2());
                            var textRect = new F.Rect(textPosition.X, textPosition.Y, _currentZoomLevel, _currentZoomLevel);

                            g.DrawText(tileInfo.Version.ToString(), textRect, Colors.Yellow, _textFormat);
                        }
                    }
                }
            }

            // Draw and animate entities
            lock (_entitiesLock)
            {
                foreach (var entityInfo in _entities)
                {
                    entityInfo.Advance();
                    DrawSprite(g, entityInfo.Entity, entityInfo.CurrentPosition);
                }
            }

            var pluginDrawArgs = new PluginDrawEventArgs(args, this);
            Plugins.RaiseOnDraw(pluginDrawArgs);
        }

        public void DrawSprite(CanvasDrawingSession g, object o, Vector2 position)
        {
            var sprite = GetSprite(o);
            var canvasPosition = TransformGamePosition(position);
            var rect = new F.Rect(canvasPosition.X, canvasPosition.Y, _currentZoomLevel, _currentZoomLevel);

            var orientation = VisualHintAttribute.GetOrientation(o);

            if (orientation.IsDirection)
            {
                var rotation = _radiansForDirection[orientation];
                var m = Matrix4x4.CreateRotationZ(rotation, new Vector3(canvasPosition.X + _currentZoomLevel / 2, canvasPosition.Y + _currentZoomLevel / 2, 0));
                g.DrawImage(sprite, rect, sprite.Bounds, 1, CanvasImageInterpolation.Linear, m);
            }
            else
            {
                g.DrawImage(sprite, rect);
            }
        }

        /// <summary>
        /// Transforms a position in virtual game coordinates to canvas coordinates.
        /// </summary>
        /// <param name="gamePosition">Position in game coordinates</param>
        /// <returns>Position in canvas coordinates (DIPs)</returns>
        public Vector2 TransformGamePosition(Vector2 gamePosition)
        {
            return new Vector2(
                (gamePosition.X - _currentCameraPosition.X - .5f) * _currentZoomLevel + (float)_canvas.Size.Width / 2,
                -(gamePosition.Y - _currentCameraPosition.Y + .5f) * _currentZoomLevel + (float)_canvas.Size.Height / 2);
        }

        /// <summary>
        /// Transforms a position in canvas coordinates to virtual game coordinates.
        /// </summary>
        /// <param name="canvasPosition">Position in canvas coordinates (DIPs)</param>
        /// <returns>Position in game coordinates</returns>
        public Point TransformCanvasPosition(Vector2 canvasPosition)
        {
            return new Point(
                (int)Math.Floor(((canvasPosition.X - (float)_canvas.Size.Width / 2) / _currentZoomLevel) + .5f + _currentCameraPosition.X),
                (int)Math.Ceiling(((canvasPosition.Y - (float)_canvas.Size.Height / 2) / -_currentZoomLevel) - .5f + _currentCameraPosition.Y));
        }

        public CanvasBitmap GetSprite(object o)
        {
            return _sprites.TryGetValue(VisualHintAttribute.GetVisualName(o), _sprites["NoSprite"]);
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
            _sprites["PistonTile"] = await loadSpriteAsync("Piston");
            _sprites["CornerTile"] = await loadSpriteAsync("Corner");

            // Entity sprites
            _sprites["PlayerEntity"] = await loadSpriteAsync("PlayerRight");
            _sprites["BoxEntity"] = await loadSpriteAsync("Box");
            _sprites["BalloonEntity-Red"] = await loadSpriteAsync("RedBall");
            _sprites["BalloonEntity-Green"] = await loadSpriteAsync("GreenBall");
            _sprites["BalloonEntity-Blue"] = await loadSpriteAsync("BlueBall");
            _sprites["PistonEntity"] = await loadSpriteAsync("BoxingGlove");
            _sprites["CoinEntity"] = await loadSpriteAsync("Coin");
        }

        private void OnFollowedPlayerMoved(EntityMoveEvent e)
        {
            CameraPosition = e.TargetPosition.ToVector2();
        }

        private void OnEntitySpawned(EntitySpawnEvent e)
        {
            var entityInfo = new EntityInfo(e.Entity, e.Position, Map.Events);
            entityInfo.Despawned += OnEntityDespawned;

            lock (_entitiesLock)
                _entities.Add(entityInfo);
        }

        private void OnEntityDespawned(EntityInfo entityInfo)
        {
            lock (_entitiesLock)
                _entities.Remove(entityInfo);
        }

        private void MoveToPlayer(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
                return;

            lock (_entitiesLock)
            {
                var player = _entities.FirstOrDefault(e => (e.Entity as PlayerEntity)?.PlayerId == playerId);

                if (player != null)
                    CameraPosition = player.TargetPosition;
            }
        }
    }
}
