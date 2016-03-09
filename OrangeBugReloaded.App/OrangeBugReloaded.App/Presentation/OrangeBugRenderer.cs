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
        private const float _dipsPerUnit = 200;
        private const float _unitsPerDip = 1 / _dipsPerUnit;

        private static readonly Dictionary<Point, float> _radiansForDirection = new Dictionary<Point, float>
        {
            { Point.North, 0 },
            { Point.West, Mathf.PI * 3 / 2 },
            { Point.South, Mathf.PI },
            { Point.East, Mathf.PI / 2 },
        };

        private static readonly CanvasTextFormat _textFormat = new CanvasTextFormat
        {
            HorizontalAlignment = CanvasHorizontalAlignment.Center,
            VerticalAlignment = CanvasVerticalAlignment.Center,
            FontSize = 8
        };

        private readonly Dictionary<string, CanvasBitmap> _sprites = new Dictionary<string, CanvasBitmap>();
        private readonly List<EntityInfo> _entities = new List<EntityInfo>();
        private readonly object _entitiesLock = new object();
        private readonly CoordinateSystem _coords = new CoordinateSystem();
        private CanvasAnimatedControl _canvas;
        private IGameplayMap _map;
        private float _currentZoomLevel = .2f;
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
            _currentZoomLevel = Mathf.Lerp(_currentZoomLevel, Mathf.Clamp(ZoomLevel, .01f, 1), _zoomLevelDamping * deltaTime);
            _currentCameraPosition = Vector2.Lerp(_currentCameraPosition, CameraPosition, _cameraPositionDamping * deltaTime);

            // Update coordinate system
            _coords.DipsPerUnit = 200;
            _coords.CameraPosition = _currentCameraPosition;
            _coords.ZoomLevel = _currentZoomLevel;
            _coords.CanvasSize = sender.Size.ToVector2();

            // Draw tiles
            using (var spriteBatch = g.CreateSpriteBatch())
            {
                foreach (var kvp in Map.ChunkLoader.Chunks.ToArray())
                {
                    for (var y = 0; y < Chunk.Size; y++)
                    {
                        for (var x = 0; x < Chunk.Size; x++)
                        {
                            var tileInfo = kvp.Value[x, y];

                            var position = kvp.Key * Chunk.Size + new Point(x, y);
                            //DrawSprite(g, tileInfo.Tile, position.ToVector2());
                            DrawSpriteBatched(spriteBatch, tileInfo.Tile, position.ToVector2());

                            if (DisplayDebugInfo)
                            {
                                // Draw tile version for testing purposes
                                var textPosition = _coords.GameToCanvasPoint(position.ToVector2());
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
                        //DrawSprite(g, entityInfo.Entity, entityInfo.CurrentPosition);
                        DrawSpriteBatched(spriteBatch, entityInfo.Entity, entityInfo.CurrentPosition);
                    }
                }
            }

            var pluginDrawArgs = new PluginDrawEventArgs(args, this);
            Plugins.RaiseOnDraw(pluginDrawArgs);
        }

        public void DrawSprite(CanvasDrawingSession g, object o, Vector2 position)
        {
            var sprite = GetSprite(o);
            var canvasPosition = _coords.GameToCanvasPoint(position);
            var rect = new F.Rect(canvasPosition.X, canvasPosition.Y, _currentZoomLevel * _coords.DipsPerUnit, _currentZoomLevel * _coords.DipsPerUnit);

            var orientation = (o as GameObject)?.VisualOrientation ?? Point.Zero;

            if (orientation.IsDirection)
            {
                var rotation = _radiansForDirection[orientation];
                var m = Matrix4x4.CreateRotationZ(rotation, new Vector3(canvasPosition.X + (_currentZoomLevel * _coords.DipsPerUnit) / 2, canvasPosition.Y + (_currentZoomLevel * _coords.DipsPerUnit) / 2, 0));
                g.DrawImage(sprite, rect, sprite.Bounds, 1, CanvasImageInterpolation.Linear, m);
            }
            else
            {
                g.DrawImage(sprite, rect);
            }
        }

        public void DrawSpriteBatched(CanvasSpriteBatch g, object o, Vector2 position)
        {
            var sprite = GetSprite(o);


            var canvasPosition = _coords.GameToCanvasPoint(position);
            var rect = new F.Rect(canvasPosition.X, canvasPosition.Y, _currentZoomLevel, _currentZoomLevel);

            var orientation = (o as GameObject)?.VisualOrientation ?? Point.Zero;

            var m = _coords.GameToCanvasMatrix(position, orientation.IsDirection ? _radiansForDirection[orientation] : 0);

            if (orientation.IsDirection)
            {
                var rotation = _radiansForDirection[orientation];
                var rotCenter = new Vector2(m.Translation.X + _coords.DipsPerUnit / 2, m.Translation.Y + _coords.DipsPerUnit / 2);

                //g.DrawImage(sprite, rect, sprite.Bounds, 1, CanvasImageInterpolation.Linear, m);
                g.Draw(sprite, m);
            }
            else
            {
                //g.DrawImage(sprite, rect);
                g.Draw(sprite, m);
            }
        }

        private Matrix3x2 TransformGamePositionMatrix(Vector2 position)
        {
            var v = new Vector2(
                (position.X - _currentCameraPosition.X - .5f) * _currentZoomLevel + (float)_canvas.Size.Width / 2,
                -(position.Y - _currentCameraPosition.Y + .5f) * _currentZoomLevel + (float)_canvas.Size.Height / 2);

            var center = new Vector2(
                (float)_canvas.Size.Width / 2,
                (float)_canvas.Size.Height / 2);

            var transform =
                //Matrix3x2.CreateTranslation(position * new Vector2(200, -200)) *
                //Matrix3x2.CreateTranslation(_currentCameraPosition * new Vector2(200,200))*
                //Matrix3x2.CreateScale(_currentZoomLevel / 200) *
                Matrix3x2.CreateTranslation(center) *
                Matrix3x2.CreateTranslation(position * _currentZoomLevel) *
                //Matrix3x2.CreateTranslation(new Vector2(200, 0)) *
                Matrix3x2.Identity;

            return transform;
        }

        public CanvasBitmap GetSprite(object o)
        {
            return _sprites.TryGetValue((o as GameObject)?.VisualKey, _sprites["NoSprite"]);
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
            CameraPosition = e.TargetPosition.ToVector2() + new Vector2(.5f, -.5f);
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
