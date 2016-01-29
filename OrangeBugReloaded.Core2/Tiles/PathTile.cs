namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A tile that any <see cref="Entity"/> can move over at any time.
    /// </summary>
    public class PathTile : Tile
    {
        public static PathTile Default { get; } = new PathTile();

        private PathTile() { }
    }
}
