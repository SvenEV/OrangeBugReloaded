using Newtonsoft.Json;
using OrangeBugReloaded.Core.Designing;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// A box that can be moved by <see cref="PlayerEntity"/> instances.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [GameObjectMetadata(Name = "Box")]
    public class BoxEntity : Entity
    {
        /// <inheritdoc/>
        internal override async Task<bool> TryDetachAsync(EntityMoveContext e) =>
            e.CurrentMove.Entity is PlayerEntity && e.Initiator is PlayerEntity && await TryMoveTowardsAsync(e.CurrentMove.Offset, e); // This enables boxes to be pushed
    }
}
