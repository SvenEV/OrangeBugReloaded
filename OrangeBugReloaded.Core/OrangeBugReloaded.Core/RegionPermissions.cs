using OrangeBugReloaded.Core.Tiles;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Describes the permissions of users regarding
    /// actions on the <see cref="Tile"/>s of a <see cref="Region"/>.
    /// 
    /// TODO:
    /// Quick and dirty right now: Each tile has an
    /// owner who is the only one allowed to edit the tile.
    /// </summary>
    public class RegionPermissions
    {
        /// <summary>
        /// The user who owns the <see cref="Region"/>.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Checks whether the specified user is allowed to
        /// design the <see cref="Region"/>.
        /// </summary>
        /// <param name="name">Username</param>
        /// <returns>True if tile can be edited; false otherwise</returns>
        public bool CanDesign(string name) => name == Owner;

        internal RegionPermissions Clone() => (RegionPermissions)MemberwiseClone();
    }
}