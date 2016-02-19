using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A barrier that entities cannot move over.
    /// </summary>
    public class WallTile : Tile
    {
        public static WallTile Default { get; } = new WallTile();

        private WallTile() { }

        internal override Task AttachEntityAsync(IAttachArgs e)
        {
            e.Cancel();
            return Task.CompletedTask;
        }

        internal override Task DetachEntityAsync(IDetachArgs e)
        {
            e.Cancel();
            return Task.CompletedTask;
        }
    }
}
