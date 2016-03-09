using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Presentation;
using System.Threading.Tasks;
using System.Collections;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A gate that can be opened and closed by an <see cref="ITrigger"/>
    /// such as a <see cref="ButtonTile"/>.
    /// In its closed state the gate behaves like a <see cref="WallTile"/>,
    /// in its opened state it behaves like a <see cref="PathTile"/>.
    /// </summary>
    public class GateTile : Tile
    {
        /// <summary>
        /// The position of the <see cref="ITrigger"/> that
        /// opens and closes the gate.
        /// </summary>
        [MapDependency]
        public Point TriggerPosition { get; }

        /// <summary>
        /// Indicates whether the gate is open.
        /// </summary>
        public bool IsOpen { get; }

        public override string VisualKey => $"GateTile-{(IsOpen ? "Open" : "Closed")}";


        public GateTile(Point triggerPosition, bool isOpen)
        {
            TriggerPosition = triggerPosition;
            IsOpen = isOpen;
        }

        internal override async Task AttachEntityAsync(IAttachArgs e)
        {
            if (IsOpen)
            {
                await base.AttachEntityAsync(e);
            }
            else
            {
                e.StopRecording();
            }
        }

        internal override async Task DetachEntityAsync(IDetachArgs e)
        {
            if (IsOpen)
            {
                await base.DetachEntityAsync(e);
            }
            else
            {
                e.StopRecording();
            }
        }

        internal override async Task OnEntityMoveTransactionCompletedAsync(IMovesCompletedArgs e)
        {
            // Check trigger state
            var trigger = (await e.GetAsync(TriggerPosition)).Tile as ITrigger;

            if (trigger?.IsOn ?? false)
            {
                e.Result = Compose(new GateTile(TriggerPosition, true), Entity);
                if (!IsOpen) e.Emit(new GateTileOpenCloseEvent(true));
            }
            else
            {
                if (Entity == Entity.None)
                {
                    e.Result = new GateTile(TriggerPosition, false);
                    if (IsOpen) e.Emit(new GateTileOpenCloseEvent(false));
                }
                else
                {
                    e.Result = Compose(new GateTile(TriggerPosition, true), Entity);
                    if (!IsOpen) e.Emit(new GateTileOpenCloseEvent(true));
                }
            }
        }

        protected override IEnumerable GetHashProperties()
        {
            yield return TriggerPosition;
            yield return IsOpen;
        }
    }

    public class GateTileOpenCloseEvent : IGameEvent, IAudioHint
    {
        public bool IsOpen { get; }
        public string AudioKey => $"GateTile-{(IsOpen ? "Opened" : "Closed")}";

        public GateTileOpenCloseEvent(bool isOpen)
        {
            IsOpen = isOpen;
        }
    }
}
