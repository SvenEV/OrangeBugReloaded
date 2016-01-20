using Newtonsoft.Json;
using OrangeBugReloaded.Core.Designing;
using OrangeBugReloaded.Core.Entities;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A wall no <see cref="Entity"/> can pass.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [GameObjectMetadata(Name = "Wall", Description = "Acts as a barrier for every entity.")]
    public class WallTile : Tile
    {
        internal override Task<bool> CanAttachEntityAsync(EntityMoveContext e) => False;
        internal override Task<bool> CanDetachEntityAsync(EntityMoveContext e) => False;
    }
}
