﻿using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Tiles
{
    /// <summary>
    /// A barrier that entities cannot move over.
    /// </summary>
    public class WallTile : Tile
    {
        public static WallTile Default { get; } = new WallTile();

        private WallTile() { }

        internal override Task AttachEntityAsync(TileEventArgs e)
        {
            e.Transaction.Cancel();
            return Task.CompletedTask;
        }

        internal override Task DetachEntityAsync(TileEventArgs e)
        {
            e.Transaction.Cancel();
            return Task.CompletedTask;
        }
    }
}
