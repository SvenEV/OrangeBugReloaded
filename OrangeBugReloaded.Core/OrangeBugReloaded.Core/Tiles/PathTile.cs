using Newtonsoft.Json;
using OrangeBugReloaded.Core.Designing;
using OrangeBugReloaded.Core.Entities;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A basic tile where any <see cref="Entity"/> can move onto.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [GameObjectMetadata(Name = "Path", Description = "A tile where players and other entities can move on.")]
    public class PathTile : Tile
    {
        [JsonProperty(PropertyName = nameof(CausesInfection))]
        private bool _causesInfection;

        /// <summary>
        /// Indicates whether the <see cref="PlayerEntity"/> is infected
        /// when attached to this <see cref="Tile"/>.
        /// If infected, the player leaves behind a trace of crosses
        /// that other <see cref="Entity"/> instances can not enter.
        /// </summary>
        [Editable]
        public bool CausesInfection
        {
            get { return _causesInfection; }
            set { Set(ref _causesInfection, value); }
        }

        internal override async Task<bool> CanAttachEntityAsync(EntityMoveContext e) =>
            (Entity == null && (!CausesInfection || e.CurrentMove.Entity is PlayerEntity)) ||
            (Entity != null && (!CausesInfection || e.CurrentMove.Entity is PlayerEntity) && await Entity.TryDetachAsync(e));

        /// <inheritdoc/>
        protected override Task OnEntityAttachedAsync(EntityMoveContext e)
        {
            var player = Entity as PlayerEntity;

            if (player != null)
            {
                player.IsInfected |= CausesInfection;
                CausesInfection |= player.IsInfected;
            }

            return Done;
        }
    }
}
