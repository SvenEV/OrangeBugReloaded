using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrangeBugReloaded.Core.Designing;
using OrangeBugReloaded.Core.Entities;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A button implementing <see cref="ITrigger"/>.
    /// <seealso cref="GateTile"/>
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [GameObjectMetadata(Name = "Button")]
    public class ButtonTile : Tile, ITrigger
    {
        private bool _isOn;

        [JsonProperty(PropertyName = "Sensitivity")]
        [JsonConverter(typeof(StringEnumConverter))]
        private EntityFilterMode _sensitivity;

        /// <inheritdoc/>
        public bool IsOn
        {
            get { return _isOn; }
            set { Set(ref _isOn, value); }
        }

        /// <summary>
        /// Determines how sensitive the button is.
        /// </summary>
        [Editable]
        public EntityFilterMode Sensitivity
        {
            get { return _sensitivity; }
            set { Set(ref _sensitivity, value); }
        }

        /// <inheritdoc/>
        protected override Task OnEntityMovesCompletedAsync(EntityMoveContext e)
        {
            // We use OnEntityMovementCompleted() so that in movement chains
            // where an entity leaves the button and then another entity enters
            // the button the IsOn-state stays the same
            // (does not switch to off and then on again instantly).

            RefreshTriggerState();
            return Done; 
        }

        private void RefreshTriggerState()
        {
            if (Entity != null)
                IsOn = Sensitivity.Includes(Entity.GetType());
            else
                IsOn = false;
        }

        /// <inheritdoc/>
        protected override Task OnActivateAsync()
        {
            RefreshTriggerState();
            return Done;
        }
    }
}
