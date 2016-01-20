using Newtonsoft.Json;
using OrangeBugReloaded.Core.Designing;
using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Foundation;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A gate that can be opened and closed by an <see cref="ITrigger"/> instance.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [GameObjectMetadata(Name = "Gate")]
    public class GateTile : Tile
    {
        [JsonProperty(PropertyName = "TriggerPosition")]
        private Point _triggerPosition = Point.Zero;

        private bool _isOpen = false;

        private bool _isCloseRequested = false;

        /// <summary>
        /// The position of the <see cref="ITrigger"/> that
        /// opens and closes the <see cref="GateTile"/>.
        /// </summary>
        [Editable("Trigger Position")]
        public Point TriggerPosition
        {
            get { return _triggerPosition; }
            set { Set(ref _triggerPosition, value); }
        }

        /// <summary>
        /// Indicates whether the gate is opened.
        /// </summary>
        public bool IsOpen
        {
            get { return _isOpen; }
            set { Set(ref _isOpen, value); }
        }

        private void RefreshIsOpen(object sender, BindablePropertyChangedEventArgs e) =>
            RefreshIsOpen((bool)e.NewValue);

        private void RefreshIsOpen(bool isTriggerOn)
        {
            if (isTriggerOn)
            {
                IsOpen = true;
                _isCloseRequested = false;
            }
            else
            {
                if (Entity == null)
                    IsOpen = false;
                else
                    _isCloseRequested = true;
            }
        }

        /// <inheritdoc/>
        protected override async Task OnActivateAsync()
        {
            ((MapLocation)Location).Map.Subscribe(_triggerPosition, nameof(ITrigger.IsOn), RefreshIsOpen);

            // React to initial trigger state
            var trigger = (await ((MapLocation)Location).Map.TryGetAsync(_triggerPosition))?.Tile as ITrigger;
            if (trigger != null) RefreshIsOpen(trigger.IsOn);
        }

        /// <inheritdoc/>
        protected override Task OnDeactivateAsync()
        {
            ((MapLocation)Location).Map.Unsubscribe(_triggerPosition, nameof(ITrigger.IsOn), RefreshIsOpen);
            return Done;
        }

        internal override async Task<bool> CanAttachEntityAsync(EntityMoveContext e) =>
            IsOpen && await base.CanAttachEntityAsync(e);

        /// <inheritdoc/>
        protected override Task OnEntityMovesCompletedAsync(EntityMoveContext e)
        {
            // We can't use OnEntityDetached(...) instead because there may be
            // future movements in the movement chain that cause OnEntityAttached(...)
            // to be called which would lead to a closed gate with an entity attached.

            if (Entity == null && _isCloseRequested)
            {
                IsOpen = false;
                _isCloseRequested = false;
            }

            return Done;
        }

        /// <inheritdoc/>
        public override string ToString() => base.ToString() + $" (Trigger: {TriggerPosition})";
    }
}
