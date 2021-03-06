﻿using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Presentation;
using System.Threading.Tasks;
using System.Collections;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A button that can be pressed and released by entites
    /// stepping onto it or leaving it.
    /// </summary>
    public class ButtonTile : Tile, ITrigger
    {
        /// <summary>
        /// Indicates whether the button is pressed
        /// and therefore turned on.
        /// </summary>
        public bool IsOn { get; }

        /// <summary>
        /// The subset of entities that can trigger a button press.
        /// </summary>
        public EntityFilterMode Sensitivity { get; }

        public override string VisualKey => $"ButtonTile-{Sensitivity}-{(IsOn ? "On" : "Off")}";

        public ButtonTile(bool isOn, EntityFilterMode sensitivity)
        {
            IsOn = isOn;
            Sensitivity = sensitivity;
        }

        internal override Task OnEntityMoveTransactionCompletedAsync(IMovesCompletedArgs e)
        {
            if (Entity != Entity.None && Sensitivity.Includes(Entity.GetType()))
            {
                e.Result = Compose(new ButtonTile(true, Sensitivity), Entity);
                if (!IsOn) e.Emit(new ButtonToggleEvent(true));
            }
            else
            {
                e.Result = Compose(new ButtonTile(false, Sensitivity), Entity);
                if (IsOn) e.Emit(new ButtonToggleEvent(false));
            }
            return Task.CompletedTask;
        }

        protected override IEnumerable GetHashProperties()
        {
            yield return IsOn;
            yield return Sensitivity;
        }
    }

    public class ButtonToggleEvent : IGameEvent, IAudioHint
    {
        public bool IsOn { get; }
        public string AudioKey => $"ButtonTile-Toggled-{(IsOn ? "On" : "Off")}";

        public ButtonToggleEvent(bool isOn)
        {
            IsOn = isOn;
        }
    }
}
