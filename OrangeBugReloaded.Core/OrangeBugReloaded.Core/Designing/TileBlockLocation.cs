using OrangeBugReloaded.Core.Foundation;
using OrangeBugReloaded.Core.Tiles;

namespace OrangeBugReloaded.Core.Designing
{
    /// <summary>
    /// Represents a location in a <see cref="TileBlock"/>.
    /// </summary>
    public class TileBlockLocation : BindableBase, ILocation
    {
        private bool _isSelected;
        private Tile _tile;

        /// <inheritdoc/>
        public Point Position { get; }

        /// <inheritdoc/>
        public Tile Tile
        {
            get { return _tile; }
            internal set
            {
                if (_tile == value)
                    return;

                if (_tile != null)
                    _tile.Location = null;

                if (value != null)
                    value.Location = this;

                Set(ref _tile, value);
            }
        }

        /// <summary>
        /// Indicates whether the <see cref="TileBlockLocation"/>
        /// is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { Set(ref _isSelected, value); }
        }

        /// <summary>
        /// Initializes a new <see cref="TileBlockLocation"/>.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="tile">
        /// The <see cref="Tile"/> at this <see cref="TileBlockLocation"/> or null if there is none
        /// </param>
        public TileBlockLocation(Point position, Tile tile)
        {
            Position = position;
            Tile = tile;
        }
    }
}
