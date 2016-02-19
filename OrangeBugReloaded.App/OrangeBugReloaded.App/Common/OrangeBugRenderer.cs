using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.UI;
using F = Windows.Foundation;

namespace OrangeBugReloaded.App.Common
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

        private Dictionary<string, CanvasBitmap> _sprites = new Dictionary<string, CanvasBitmap>();
        private CanvasAnimatedControl _canvas;
        private IGameplayMap _map;
        private float _currentZoomLevel = 40;
        private Vector2 _currentCameraPosition = Vector2.Zero;
        private Dictionary<Point, EntityInfo> _entities = new Dictionary<Point, EntityInfo>();
        private IDisposable _eventSubscription;

        public IGameplayMap Map
        {
            get { return _map; }
            set
            {
                if (!Equals(_map, value))
                {
                    _eventSubscription?.Dispose();

                    _map = value;

                    if (value != null)
                        _eventSubscription = value.Events.OfType<EntityMoveEvent>().Subscribe(OnEntityMoved);

                    Plugins.OnMapChanged(value);
                }
            }
        }

        public PluginCollection Plugins { get; }

        public IReadOnlyDictionary<string, CanvasBitmap> Sprites => _sprites;

        public bool DisplayDebugInfo { get; set; } = false;

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
            if (canvas == null)
                throw new ArgumentNullException(nameof(canvas));

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
                        if (tileInfo.Tile.Entity != Entity.None)
                            DrawSprite(g, tileInfo.Tile.Entity, position.ToVector2());

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

            var finishedAnimations = new List<Point>();

            foreach (var entityInfo in _entities)
            {
                var isValid = entityInfo.Value.Advance();

                if (!isValid)
                    finishedAnimations.Add(entityInfo.Key);

                DrawSprite(g, entityInfo.Value.Entity, entityInfo.Value.CurrentPosition);
            }

            foreach (var p in finishedAnimations)
            {
                var entityInfo = _entities[p];
                _entities.Remove(p);
                _entities[new Point((int)entityInfo.TargetPosition.X, (int)entityInfo.TargetPosition.Y)] = entityInfo;
            }

            // Draw entities
            //foreach (var chunk in Map.ChunkLoader.Chunks.Values)
            //{
            //    for (var y = 0; y < Chunk.Size; y++)
            //    {
            //        for (var x = 0; x < Chunk.Size; x++)
            //        {
            //            var position = chunk.Index * Chunk.Size + new Point(x, y);

            //            if (_animations.ContainsKey(position))
            //            {
            //                var animation = _animations[position];
            //                var t = (float)((DateTime.Now - animation.StartTime).TotalSeconds / animation.Duration.TotalSeconds);

            //                var interpolatedPosition = Vector2.Lerp(
            //                    animation.Event.SourcePosition.ToVector2(),
            //                    animation.Event.TargetPosition.ToVector2(), Mathf.Clamp01(t));

            //                var entity = t <= .5 ?
            //                    animation.Event.Source.Entity :
            //                    animation.Event.Target.Entity;

            //                DrawSprite(g, entity, interpolatedPosition);

            //                if (t > 1)
            //                    _animations.Remove(position);
            //            }
            //            else
            //            {
            //                var entity = chunk[x, y].Entity;

            //                if (entity != Entity.None)
            //                    DrawSprite(g, entity, position.ToVector2());
            //            }
            //        }
            //    }
            //}

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

        private void OnEntityMoved(EntityMoveEvent e)
        {
            Debug.WriteLine($"{e.Source.Entity.GetType().Name} at {e.SourcePosition} -> {e.Target.Entity.GetType().Name} at {e.TargetPosition}");

            // TODO
        }



        class EntityInfo
        {
            public Entity Entity { get; private set; }

            public Vector2 SourcePosition { get; private set; }

            public Vector2 TargetPosition { get; private set; }

            public Vector2 CurrentPosition { get; private set; }

            public DateTime AnimationStartTime { get; private set; }

            public TimeSpan AnimationDuration { get; private set; }

            public EntityInfo(Entity entity, Point position)
            {
                Entity = entity;
                SourcePosition = TargetPosition = CurrentPosition = position.ToVector2();
            }

            public void ApplyMoveEvent(EntityMoveEvent e)
            {
                // TODO: Think about animations that should happen one after another
                SourcePosition = CurrentPosition;
                TargetPosition = e.TargetPosition.ToVector2();
                AnimationStartTime = DateTime.Now;
                AnimationDuration = TimeSpan.FromSeconds(.6);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="deltaTime"></param>
            /// <returns>False, if the animation completed</returns>
            public bool Advance()
            {
                var t = (float)((DateTime.Now - AnimationStartTime).TotalSeconds / AnimationDuration.TotalSeconds);

                CurrentPosition = Vector2.Lerp(SourcePosition, TargetPosition, Mathf.Clamp01(t));

                //var entity = t <= .5 ?
                //    animation.Event.Source.Entity :
                //    animation.Event.Target.Entity;

                return t <= 1;
            }
        }




        struct Animation
        {
            public EntityMoveEvent Event { get; }
            public DateTime StartTime { get; }
            public TimeSpan Duration { get; }

            public Animation(EntityMoveEvent e)
            {
                Event = e;
                StartTime = DateTime.Now;
                Duration = TimeSpan.FromSeconds(.6);
            }
        }
    }
}
