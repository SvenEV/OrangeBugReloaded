using System;
using System.Linq;
using System.Numerics;
using OrangeBugReloaded.Core;
using System.Reactive.Linq;
using OrangeBugReloaded.Core.Events;
using System.Diagnostics;
using System.Collections.Generic;
using OrangeBugReloaded.Core.Rendering;
using F = Windows.Foundation;

namespace OrangeBugReloaded.App.Common
{
    class EntityMoveAnimationPlugin : IRendererPlugin
    {
        private List<Animation> _animations = new List<Animation>();
        private IDisposable _eventSubscription;

        public void Initialize(Map map)
        {
            _eventSubscription = map.Events.OfType<EntityMoveEvent>().Subscribe(OnEntityMoved);
        }

        private void OnEntityMoved(EntityMoveEvent e)
        {
            Debug.WriteLine($"{e.SourceEntity.GetType().Name} at {e.SourcePosition} -> {e.TargetEntity.GetType().Name} at {e.TargetPosition}");
            _animations.Add(new Animation(e));
        }

        public void OnDraw(PluginDrawEventArgs e)
        {
            for (var i = 0; i < _animations.Count; i++)
            {
                var animation = _animations[i];
                var entity = animation.Event.SourceEntity;
                var sprite = e.Renderer.Sprites[VisualHintAttribute.GetVisualName(entity)];
                var sourcePosition = e.Renderer.TransformPosition(animation.Event.SourcePosition);
                var targetPosition = e.Renderer.TransformPosition(animation.Event.TargetPosition);

                var t = (DateTime.Now - animation.StartTime).TotalSeconds / animation.Duration.TotalSeconds;

                if (t > 1)
                {
                    _animations.RemoveAt(i);
                    i--;
                }
                else
                {
                    var finalPosition = Vector2.Lerp(sourcePosition, targetPosition, (float)t);
                    var rect = new F.Rect(finalPosition.X, finalPosition.Y, e.Renderer.ZoomLevel, e.Renderer.ZoomLevel);

                    e.Args.DrawingSession.DrawImage(sprite, rect);
                }
            }
        }

        public void Dispose() => _eventSubscription.Dispose();

        struct Animation
        {
            public EntityMoveEvent Event { get; }
            public DateTime StartTime { get; }
            public TimeSpan Duration { get; }

            public Animation(EntityMoveEvent e)
            {
                Event = e;
                StartTime = DateTime.Now;
                Duration = TimeSpan.FromSeconds(.2);
            }
        }
    }
}
