﻿using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.Events;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Threading;

namespace OrangeBugReloaded.App.Presentation
{
    class EntityInfo : IDisposable
    {
        private readonly IDisposable _moveSubscription;
        private readonly IDisposable _despawnSubscription;
        private readonly AsyncManualResetEvent _finishEvent = new AsyncManualResetEvent();

        public event Action<EntityInfo> Despawned;
        public event Action<EntityInfo> Moved;

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

            _moveSubscription = eventStream
                .OfType<EntityMoveEvent>()
                .Where(e => e.SourcePosition.ToVector2() == TargetPosition)
                .Subscribe(OnMoved);

            _despawnSubscription = eventStream
                .OfType<EntityDespawnEvent>()
                .Where(e => e.Position.ToVector2() == TargetPosition)
                .Subscribe(OnDespawn);
        }

        private void OnMoved(EntityMoveEvent e)
        {
            Debug.WriteLine($"{e.Source.Entity.GetType().Name} at {e.SourcePosition} -> {e.Target.Entity.GetType().Name} at {e.TargetPosition}");

            // TODO: Think about animations that should happen one after another
            Entity = e.Target.Entity;
            SourcePosition = CurrentPosition;
            TargetPosition = e.TargetPosition.ToVector2();
            AnimationStartTime = DateTime.Now;
            AnimationDuration = TimeSpan.FromSeconds(.3);
            _finishEvent.Reset();

            Moved?.Invoke(this);
        }

        /// <summary>
        /// Updates the animation progress.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns>False, if the animation completed</returns>
        public bool Advance()
        {
            var t = GetAnimationProgress();
            CurrentPosition = Vector2.Lerp(SourcePosition, TargetPosition, 1 - (t - 1) * (t - 1));

            if (t >= 1)
                _finishEvent.Set();

            return t <= 1;
        }

        private async void OnDespawn(EntityDespawnEvent e)
        {
            _despawnSubscription.Dispose();

            // Wait for animation to finish
            await _finishEvent.WaitAsync();

            _moveSubscription.Dispose();
            Despawned?.Invoke(this);
        }

        private float GetAnimationProgress()
        {
            var t = (float)((DateTime.Now - AnimationStartTime).TotalSeconds / AnimationDuration.TotalSeconds);
            t = Mathf.Clamp01(t);
            return t;
        }

        public void Dispose()
        {
            _moveSubscription.Dispose();
            _despawnSubscription.Dispose();
        }
    }
}
