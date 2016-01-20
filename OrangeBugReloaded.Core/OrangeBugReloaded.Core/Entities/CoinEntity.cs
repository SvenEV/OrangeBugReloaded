using Newtonsoft.Json;
using OrangeBugReloaded.Core.Designing;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// A coin that can be collected by <see cref="PlayerEntity"/> instances.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [GameObjectMetadata(Name = "Coin")]
    public class CoinEntity : Entity
    {
        internal override async Task<bool> TryDetachAsync(EntityMoveContext e)
        {
            // This makes coins collectable by PlayerEntities
            if (e.CurrentMove.Entity is PlayerEntity)
            {
                await e.Map.DestroyEntityAsync(Owner.Location.Position, e);
                return true;
            }

            return false;
        }
    }
}
