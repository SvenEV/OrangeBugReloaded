using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Tiles;
using System.Linq;

namespace OrangeBugReloaded.Core
{
    public interface IReadOnlyChunk
    {
        Point Index { get; }
        bool IsEmpty { get; }
        bool HasChanged { get; }
        Tile this[Point position] { get; }
        Tile this[int x, int y] { get; }
        TileMetadata GetMetadata(Point position);
        IReadOnlyChunk Clone();
        void OnSaved();
    }

    public interface IChunk : IReadOnlyChunk
    {
        new Tile this[Point position] { get; set; }
        new Tile this[int x, int y] { get; set; }
        void SetMetadata(Point position, TileMetadata value);
    }

    public class Chunk : IChunk
    {
        public const int Size = 8;

        private readonly Tile[] _tiles;
        private readonly TileMetadata[] _tileMetadata;

        public Point Index { get; } = Point.Zero;

        public bool IsEmpty => _tileMetadata.All(t => t == TileMetadata.Empty);

        public bool HasChanged { get; private set; }

        public static Chunk SampleChunk { get; } = CreateSampleChunk();
        public static Chunk SampleChunk2 { get; } = CreateSampleChunk2();

        public Chunk(Point index)
        {
            Index = index;
            _tiles = Enumerable.Repeat(Tile.Empty, Size * Size).ToArray();
            _tileMetadata = Enumerable.Repeat(TileMetadata.Empty, Size * Size).ToArray();
        }

        private static Chunk CreateSampleChunk()
        {
            var chunk = new Chunk(Point.Zero);

            for (var y = 1; y < Size - 1; y++)
                for (var x = 1; x < Size - 1; x++)
                    chunk[x, y] = PathTile.Default;

            chunk[1, 2] = Tile.Compose(PathTile.Default, new PlayerEntity("local", Point.North));
            chunk[2, 2] = Tile.Compose(PathTile.Default, BoxEntity.Default);
            chunk[3, 2] = new ButtonTile(false, EntityFilterMode.EntitiesExceptPlayer);
            chunk[4, 2] = new TeleporterTile(new Point(4, 4));
            chunk[4, 4] = new TeleporterTile(new Point(4, 2));
                          
            chunk[3, 3] = new GateTile(new Point(3, 2), false);
                          
            chunk[5, 5] = Tile.Compose(PathTile.Default, new BalloonEntity(InkColor.Green));
            chunk[3, 5] = new PinTile(InkColor.Red);
            chunk[6, 3] = new PinTile(InkColor.Green);
                          
            chunk[4, 5] = new InkTile(InkColor.Red);
            chunk[3, 4] = new InkTile(InkColor.Green);
            chunk[6, 5] = new InkTile(InkColor.Blue);
                          
            chunk[0, 3] = PathTile.Default;
            chunk[0, 4] = PathTile.Default;

            for (var y = 1; y < Size - 1; y++)
                for (var x = 1; x < Size - 1; x++)
                    chunk.SetMetadata(new Point(x, y), new TileMetadata(chunk[x, y], 0));

            return chunk;
        }

        private static Chunk CreateSampleChunk2()
        {
            var chunk = new Chunk(Point.West);

            for (var y = 1; y < Size - 1; y++)
                for (var x = 1; x < Size - 1; x++)
                    chunk[x, y] = PathTile.Default;

            chunk[7, 3] = PathTile.Default;
            chunk[7, 4] = PathTile.Default;

            chunk[3, 3] = new PistonTile(new Point(3, 2), Point.East);
            chunk[4, 3] = Tile.Compose(new ButtonTile(false, EntityFilterMode.Entities), BoxEntity.Default);
            chunk[5, 3] = new InkTile(InkColor.Blue);

            chunk[2, 5] = new PistonTile(new Point(-4, 3), Point.South);
            chunk[3, 5] = new GateTile(new Point(-4, 3), false);

            for (var y = 1; y < Size - 1; y++)
                for (var x = 1; x < Size - 1; x++)
                    chunk.SetMetadata(new Point(x, y), new TileMetadata(chunk[x, y], 0));

            return chunk;
        }

        public Tile this[Point position]
        {
            get { return this[position.X, position.Y]; }
            set { this[position.X, position.Y] = value; }
        }

        public Tile this[int x, int y]
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

        public IReadOnlyChunk Clone() => new Chunk(this);

        public void OnSaved()
        {
            HasChanged = false;
        }
    }
}