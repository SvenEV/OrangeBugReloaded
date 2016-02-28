using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.Events;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace OrangeBugReloaded.App.Presentation
{
    class EntityInfo : IDisposable
    {
        private readonly IDisposable _sub1, _sub2;

        public event Action<EntityInfo> Despawned;

        public Entity Entity { get; private set; }

        public Vector2 SourcePosition { get; private set; }

        public Vector2 TargetPosition { get; private set; }

        public Vector2 CurrentPosition { get; private set; }

        public DateTime AnimationStartTime { get; private set; }

        public TimeSpan AnimationDuration { get; private set; }

        public EntityInfo(Entity entity, Point position, IObservable<IGameEvent> eventStream)
        {
            Entity = entity;
            SourcePosition = TargetPosition = CurrentPosition = position.ToVector2();

            _sub1 = eventStream
                .OfType<EntityMoveEvent>()
                .Where(e => e.SourcePosition.ToVector2() == TargetPosition)
                .Subscribe(ApplyMoveEvent);

            _sub2 = eventStream
                .OfType<EntityDespawnEvent>()
                .Where(e => e.Position.ToVector2() == TargetPosition)
                .Subscribe(OnDespawn);
        }

        public void ApplyMoveEvent(EntityMoveEvent e)
        {
            Debug.WriteLine($"{e.Source.Entity.GetType().Name} at {e.SourcePosition} -> {e.Target.Entity.GetType().Name} at {e.TargetPosition}");

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

        private void OnDespawn(EntityDespawnEvent e)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            Despawned?.Invoke(this);
        }

        public void Dispose()
        {
            _sub1.Dispose();
            _sub2.Dispose();
        }
    }
}
