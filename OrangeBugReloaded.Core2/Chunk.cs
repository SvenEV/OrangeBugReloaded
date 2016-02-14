using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Tiles;
using System;
using System.Linq;

namespace OrangeBugReloaded.Core
{
    public interface IReadOnlyChunk
    {
        [Obsolete("Use key of Dictionary<Point, IChunk> instead", true)]
        Point Index { get; }
        bool IsEmpty { get; }
        bool HasChanged { get; }
        TileInfo this[Point position] { get; }
        TileInfo this[int x, int y] { get; }
        TileMetadata GetMetadata(Point position);
        IReadOnlyChunk Clone();
        void OnSaved();
    }

    public interface IChunk : IReadOnlyChunk
    {
        new TileInfo this[Point position] { get; set; }
        new TileInfo this[int x, int y] { get; set; }
        new IChunk Clone();
        void SetMetadata(Point position, TileMetadata value);
    }

    public class Chunk : IChunk
    {
        public const int Size = 8;

        private readonly TileInfo[] _tiles;
        private readonly TileMetadata[] _tileMetadata;

        public Point Index { get; } = Point.Zero;

        public bool IsEmpty => _tileMetadata.All(t => t == TileMetadata.Empty);

        public bool HasChanged { get; private set; }

        public static Chunk SampleChunk { get; } = CreateSampleChunk();
        public static Chunk SampleChunk2 { get; } = CreateSampleChunk2();

        public Chunk(Point index)
        {
            Index = index;
            _tiles = Enumerable.Repeat(new TileInfo(Tile.Empty, 0), Size * Size).ToArray();
            _tileMetadata = Enumerable.Repeat(TileMetadata.Empty, Size * Size).ToArray();
        }

        private static Chunk CreateSampleChunk()
        {
            var chunk = new Chunk(Point.Zero);
            var v0 = new Func<Tile, TileInfo>(t => new TileInfo(t, 0));

            for (var y = 1; y < Size - 1; y++)
                for (var x = 1; x < Size - 1; x++)
                    chunk[x, y] = v0(PathTile.Default);

            //chunk[1, 2] = v0(Tile.Compose(PathTile.Default, new PlayerEntity("local", Point.North)));
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

            for (var y = 1; y < Size - 1; y++)
                for (var x = 1; x < Size - 1; x++)
                    chunk.SetMetadata(new Point(x, y), new TileMetadata(chunk[x, y].Tile, 0));

            return chunk;
        }

        private static Chunk CreateSampleChunk2()
        {
            var chunk = new Chunk(Point.West);
            var v0 = new Func<Tile, TileInfo>(t => new TileInfo(t, 0));

            for (var y = 1; y < Size - 1; y++)
                for (var x = 1; x < Size - 1; x++)
                    chunk[x, y] = v0(PathTile.Default);

            chunk[7, 3] = v0(PathTile.Default);
            chunk[7, 4] = v0(PathTile.Default);

            chunk[3, 3] = v0(new PistonTile(new Point(3, 2), Point.East));
            chunk[4, 3] = v0(Tile.Compose(new ButtonTile(false, EntityFilterMode.Entities), BoxEntity.Default));
            chunk[5, 3] = v0(new InkTile(InkColor.Blue));

            chunk[2, 5] = v0(new PistonTile(new Point(-4, 3), Point.South));
            chunk[3, 5] = v0(new GateTile(new Point(-4, 3), false));

            for (var y = 1; y < Size - 1; y++)
                for (var x = 1; x < Size - 1; x++)
                    chunk.SetMetadata(new Point(x, y), new TileMetadata(chunk[x, y].Tile, 0));

            return chunk;
        }

        public TileInfo this[Point position]
        {
            get { return this[position.X, position.Y]; }
            set { this[position.X, position.Y] = value; }
        }

        public TileInfo this[int x, int y]
        {
            get { return _tiles[Size * y + x]; }
            set
            {
                if (!Equals(_tiles[Size * y + x], value))
                {
                    _tiles[Size * y + x] = value;
                    HasChanged = true;
                }
            }
        }

        public TileMetadata GetMetadata(Point position) => _tileMetadata[Size * position.Y + position.X];
        public void SetMetadata(Point position, TileMetadata value) => _tileMetadata[Size * position.Y + position.X] = value;

        private Chunk(Chunk chunk)
        {
            _tiles = chunk._tiles.ToArray();
            _tileMetadata = chunk._tileMetadata.ToArray();
            Index = chunk.Index;
            HasChanged = false;
        }

        public IChunk Clone() => new Chunk(this);

        IReadOnlyChunk IReadOnlyChunk.Clone() => new Chunk(this);

        public void OnSaved()
        {
            HasChanged = false;
        }
    }
}