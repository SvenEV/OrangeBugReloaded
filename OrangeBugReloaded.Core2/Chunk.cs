using System.Linq;

namespace OrangeBugReloaded.Core
{
    public interface IReadOnlyChunk
    {
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

        public bool IsEmpty => _tileMetadata.All(t => t == TileMetadata.Empty);

        public bool HasChanged { get; private set; }

        public Chunk()
        {
            _tiles = Enumerable.Repeat(new TileInfo(Tile.Empty, 0), Size * Size).ToArray();
            _tileMetadata = Enumerable.Repeat(TileMetadata.Empty, Size * Size).ToArray();
        }

        private Chunk(Chunk chunk)
        {
            _tiles = chunk._tiles.ToArray();
            _tileMetadata = chunk._tileMetadata.ToArray();
            HasChanged = false;
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

        public TileMetadata GetMetadata(Point position)
            => _tileMetadata[Size * position.Y + position.X];

        public void SetMetadata(Point position, TileMetadata value)
            => _tileMetadata[Size * position.Y + position.X] = value;

        public IChunk Clone() => new Chunk(this);

        IReadOnlyChunk IReadOnlyChunk.Clone() => new Chunk(this);

        public void OnSaved()
        {
            HasChanged = false;
        }
    }
}