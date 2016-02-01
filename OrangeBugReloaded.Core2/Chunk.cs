using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Tiles;
using System.Linq;
using System;

namespace OrangeBugReloaded.Core
{
    public interface IReadOnlyChunk
    {
        Point Index { get; }
        bool IsEmpty { get; }
        bool HasChanged { get; }
        Tile this[Point index, MapLayer layer] { get; }
        Tile this[int x, int y, MapLayer layer] { get; }
        IReadOnlyChunk Clone();
        void OnSaved();
    }

    public interface IChunk : IReadOnlyChunk
    {
        new Tile this[Point index, MapLayer layer] { get; set; }
        new Tile this[int x, int y, MapLayer layer] { get; set; }
    }

    public class Chunk : IChunk
    {
        public const int Size = 8;

        private Tile[] _tiles;
        private Tile[] _designedTiles;

        public Point Index { get; } = Point.Zero;

        public bool IsEmpty =>
            _tiles.All(t => t == Tile.Empty) &&
            _designedTiles.All(t => t == Tile.Empty);

        public bool HasChanged { get; internal set; }

        public static Chunk SampleChunk { get; } = CreateSampleChunk();
        public static Chunk SampleChunk2 { get; } = CreateSampleChunk2();

        public Chunk(Point index)
        {
            Index = index;
            _tiles = Enumerable.Repeat(Tile.Empty, Size * Size).ToArray();
            _designedTiles = _tiles.ToArray();
        }

        private static Chunk CreateSampleChunk()
        {
            var chunk = new Chunk(Point.Zero);

            for (var y = 1; y < Size - 1; y++)
                for (var x = 1; x < Size - 1; x++)
                    chunk[x, y, MapLayer.Gameplay] = PathTile.Default;

            chunk[1, 2, MapLayer.Gameplay] = Tile.Compose(PathTile.Default, PlayerEntity.PlayerLookingNorth);
            chunk[2, 2, MapLayer.Gameplay] = Tile.Compose(PathTile.Default, BoxEntity.Default);
            chunk[3, 2, MapLayer.Gameplay] = new ButtonTile(false, EntityFilterMode.EntitiesExceptPlayer);
            chunk[4, 2, MapLayer.Gameplay] = new TeleporterTile(new Point(4, 4));
            //chunk[4, 4, MapLayer.Gameplay] = new TeleporterTile(new Point(4, 2));

            chunk[3, 3, MapLayer.Gameplay] = new GateTile(new Point(3, 2), false);

            chunk[5, 5, MapLayer.Gameplay] = Tile.Compose(PathTile.Default, new BalloonEntity(InkColor.Green));
            chunk[3, 5, MapLayer.Gameplay] = new PinTile(InkColor.Red);
            chunk[6, 3, MapLayer.Gameplay] = new PinTile(InkColor.Green);

            chunk[4, 5, MapLayer.Gameplay] = new InkTile(InkColor.Red);
            chunk[4, 4, MapLayer.Gameplay] = new InkTile(InkColor.Green);
            chunk[6, 5, MapLayer.Gameplay] = new InkTile(InkColor.Blue);

            chunk[0, 3, MapLayer.Gameplay] = PathTile.Default;
            chunk[0, 4, MapLayer.Gameplay] = PathTile.Default;
            return chunk;
        }

        private static Chunk CreateSampleChunk2()
        {
            var chunk = new Chunk(Point.West);

            for (var y = 1; y < Size - 1; y++)
                for (var x = 1; x < Size - 1; x++)
                    chunk[x, y, MapLayer.Gameplay] = PathTile.Default;

            chunk[3, 3, MapLayer.Gameplay] = new PistonTile(new Point(3, 2), Point.East);
            chunk[4, 3, MapLayer.Gameplay] = Tile.Compose(new ButtonTile(false, EntityFilterMode.Entities), BoxEntity.Default);
            chunk[5, 3, MapLayer.Gameplay] = new InkTile(InkColor.Blue);

            chunk[2, 5, MapLayer.Gameplay] = new PistonTile(new Point(-4, 3), Point.South);
            chunk[3, 5, MapLayer.Gameplay] = new GateTile(new Point(-4, 3), false);

            chunk[7, 3, MapLayer.Gameplay] = PathTile.Default;
            chunk[7, 4, MapLayer.Gameplay] = PathTile.Default;
            return chunk;
        }

        public Tile this[Point index, MapLayer layer]
        {
            get { return this[index.X, index.Y, layer]; }
            set { this[index.X, index.Y, layer] = value; }
        }

        public Tile this[int x, int y, MapLayer layer]
        {
            get { return GetTilesForLayer(layer)[Size * y + x]; }
            set
            {
                var tiles = GetTilesForLayer(layer);
                if (!Equals(tiles[Size * y + x], value))
                {
                    tiles[Size * y + x] = value;
                    HasChanged = true;
                }
            }
        }

        private Chunk(Chunk chunk)
        {
            _tiles = chunk._tiles.ToArray();
            _designedTiles = chunk._designedTiles.ToArray();
            Index = chunk.Index;
            HasChanged = false;
        }

        private Tile[] GetTilesForLayer(MapLayer layer)
        {
            switch (layer)
            {
                case MapLayer.Gameplay: return _tiles;
                case MapLayer.Design: return _designedTiles;
                default: throw new NotImplementedException();
            }
        }

        public IReadOnlyChunk Clone() => new Chunk(this);

        public void OnSaved()
        {
            HasChanged = false;
        }
    }
}