using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Tiles;

namespace OrangeBugReloaded.Core
{
    public class SampleChunks
    {
        public static Chunk Chunk1 { get; } = CreateSampleChunk();
        public static Chunk Chunk2 { get; } = CreateSampleChunk2();
        public static Chunk Chunk3 { get; } = CreateSampleChunk3();

        private static TileInfo v0(Tile tile) => new TileInfo(tile, 0);

        private static Chunk CreateEmptyChunk()
        {
            var chunk = new Chunk();

            for (var y = 1; y < Chunk.Size - 1; y++)
                for (var x = 1; x < Chunk.Size - 1; x++)
                    chunk[x, y] = v0(PathTile.Default);

            return chunk;

        }

        private static Chunk CreateSampleChunk()
        {
            var chunk = CreateEmptyChunk();

            chunk[2, 2] = v0(Tile.Compose(PathTile.Default, BoxEntity.Default));
            chunk[3, 2] = v0(new ButtonTile(false, EntityFilterMode.EntitiesExceptPlayer));
            chunk[4, 2] = v0(new TeleporterTile(new Point(4, 4), EntityFilterMode.Entities));
            chunk[4, 4] = v0(new TeleporterTile(new Point(4, 2), EntityFilterMode.Entities));

            chunk[3, 3] = v0(new GateTile(new Point(3, 2), false));

            chunk[5, 5] = v0(Tile.Compose(PathTile.Default, new BalloonEntity(InkColor.Green)));
            chunk[3, 5] = v0(new PinTile(InkColor.Red));
            chunk[6, 3] = v0(new PinTile(InkColor.Green));

            chunk[4, 5] = v0(new InkTile(InkColor.Red));
            chunk[3, 4] = v0(new InkTile(InkColor.Green));
            chunk[6, 5] = v0(new InkTile(InkColor.Blue));

            chunk[0, 3] = v0(PathTile.Default);
            chunk[0, 4] = v0(PathTile.Default);
            chunk[0, 5] = v0(PathTile.Default);
            chunk[0, 6] = v0(Tile.Compose(new CornerTile(Point.South), CoinEntity.Default));

            for (var y = 1; y < Chunk.Size - 1; y++)
                for (var x = 1; x < Chunk.Size - 1; x++)
                    chunk.SetMetadata(new Point(x, y), new TileMetadata(chunk[x, y].Tile, 0));

            return chunk;
        }

        private static Chunk CreateSampleChunk2()
        {
            var chunk = CreateEmptyChunk();

            chunk[7, 3] = v0(PathTile.Default);
            chunk[7, 4] = v0(PathTile.Default);
            chunk[7, 5] = v0(PathTile.Default);
            chunk[7, 6] = v0(new CornerTile(Point.West));

            chunk[3, 3] = v0(new PistonTile(new Point(3, 2), Point.East));
            chunk[4, 3] = v0(Tile.Compose(new ButtonTile(false, EntityFilterMode.Entities), BoxEntity.Default));
            chunk[5, 3] = v0(new InkTile(InkColor.Blue));

            chunk[2, 5] = v0(new PistonTile(new Point(-4, 3), Point.South));
            chunk[3, 5] = v0(new GateTile(new Point(-4, 3), false));

            chunk[1, 1] = v0(new CornerTile(Point.East));
            chunk[1, 6] = v0(new CornerTile(Point.South));
            chunk[6, 1] = v0(new CornerTile(Point.North));
            chunk[6, 2] = v0(new CornerTile(Point.West));
            chunk[3, 2] = v0(new CornerTile(Point.South));

            chunk[3, 0] = v0(PathTile.Default);

            return chunk;
        }

        private static Chunk CreateSampleChunk3()
        {
            var chunk = CreateEmptyChunk();

            chunk[3, 7] = v0(new CornerTile(Point.North));
            chunk[2, 7] = v0(new CornerTile(Point.South));

            return chunk;
        }
    }
}
