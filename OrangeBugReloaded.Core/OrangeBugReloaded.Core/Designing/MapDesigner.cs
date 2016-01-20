using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Foundation;
using OrangeBugReloaded.Core.Tiles;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Designing
{
    /// <summary>
    /// Supports the manipulation of <see cref="IMap"/> objets.
    /// </summary>
    public class MapDesigner : BindableBase
    {
        private IMap _map;
        private TileBlock _tileBlock;
        private LockToken _lockToken;
        private GameObjectDescription _selectedCatalogItem = null;
        private OrangeBugGameObject _selectedCatalogItemInstance;

        /// <summary>
        /// The <see cref="Designing.TileBlock"/> that contains the tiles
        /// that are being edited.
        /// </summary>
        public TileBlock TileBlock
        {
            get { return _tileBlock; }
            private set { Set(ref _tileBlock, value); }
        }

        /// <summary>
        /// The <see cref="Tile"/> or <see cref="Entity"/> type
        /// that is selected for drawing operations.
        /// </summary>
        public GameObjectDescription SelectedCatalogItem
        {
            get { return _selectedCatalogItem; }
            set
            {
                if (Set(ref _selectedCatalogItem, value))
                {
                    if (value == null)
                        _selectedCatalogItemInstance = null;
                    else
                        _selectedCatalogItemInstance = (OrangeBugGameObject)Activator.CreateInstance(value.Type);
                }
            }
        }

        /// <summary>
        /// An instance of the type specified in <see cref="SelectedCatalogItem"/>.
        /// This acts as a template that can be configured before
        /// it is drawn onto the map.
        /// </summary>
        public OrangeBugGameObject SelectedCatalogItemInstance
        {
            get { return _selectedCatalogItemInstance; }
            private set { Set(ref _selectedCatalogItemInstance, value); }
        }

        /// <summary>
        /// The <see cref="TileBlockLocation"/>s that are selected. This is always a subset
        /// of the locations in the <see cref="TileBlock"/>
        /// </summary>
        public ObservableCollection<TileBlockLocation> SelectedLocations { get; } = new ObservableCollection<TileBlockLocation>();

        /// <summary>
        /// A collection of all <see cref="Tile"/> and <see cref="Entity"/>
        /// types that can be used to compose the map.
        /// </summary>
        public GameObjectDescription[] CatalogItems { get; } = GameInfo.TileTypes.Concat(GameInfo.EntityTypes).ToArray();

        /// <summary>
        /// Initializes a new <see cref="MapDesigner"/>
        /// for the specified <see cref="IMap"/>.
        /// </summary>
        /// <param name="map">Map</param>
        /// <param name="lockToken">Lock token</param>
        /// <param name="tiles">TileBlock</param>
        private MapDesigner(IMap map, LockToken lockToken, TileBlock tiles)
        {
            _map = map;
            _lockToken = lockToken;
            _tileBlock = tiles;
        }


        /// <summary>
        /// Creates a "construction site" at the specified area
        /// and returns a <see cref="MapDesigner"/> that can be
        /// used to edit the map.
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="area">Area in map coordinates</param>
        /// <returns><see cref="MapDesigner"/></returns>
        public static async Task<MapDesigner> CreateAsync(IMap map, Rectangle area)
        {
            var locker = LockContext.Create<MapLocation>(loc => loc.DesignerLock);//, "LocalPlayer"); // TODO: Change "LocalPlayer"

            // Inside 'area', select only the locations that we are allowed to manipulate.
            // Copy the tile templates there into a TileBlock for editing.

            var startLocation = await map.TryGetAsync(area.BottomLeft);

            // Check if we are allowed to design the selected region
            Region r;
            if (!(map.Metadata.Regions.TryGetValue(startLocation.RegionName ?? "", out r) && r.Permissions.CanDesign("LocalPlayer")))
            {
                Logger.LogWarning("You are not allowed to design that region");
                return null;
            }

            var locations = await startLocation.SelectRegionAsync((a, b) => a.RegionName == b.RegionName);

            var lockToken = locker.Lock(locations);

            if (lockToken.IsValid)
            {
                return new MapDesigner(map, lockToken, new TileBlock(locations));
            }
            else
            {
                // Could not lock specified area for designing
                return null;
            }
        }

        /// <summary>
        /// Closes the "construction site" and unlocks and
        /// reactivates all tiles.
        /// </summary>
        /// <returns>Task</returns>
        public async Task DisposeAsync()
        {
            // Apply modified tiles in TileBlock to MapLocations
            foreach (var location in TileBlock)
            {
                var mapLocation = await _map.TryGetAsync(location.Position);
                mapLocation.TileTemplate = location.Tile?.DeepClone();
            }

            // Unlock tiles and entities
            _lockToken.Dispose();

            // DEBUG: Immediately restore the tiles to reflect the changes
            var restoreLocation = await _map.TryGetAsync(TileBlock.First().Position);
            await restoreLocation.RestoreRegionAsync();
        }

        /// <summary>
        /// Selects all <see cref="TileBlockLocation"/>s that are contained
        /// in <paramref name="area"/> and <see cref="TileBlock"/>.
        /// The current selection is replaced with the new one.
        /// </summary>
        /// <param name="area">Area in which the locations are selected</param>
        public void Select(Rectangle area)
        {
            ClearSelection();

            foreach (var location in area.Select(p => TileBlock[p]).Where(o => o != null))
            {
                location.IsSelected = true;
                SelectedLocations.Add(location);
            }
        }

        /// <summary>
        /// Deselects all selected <see cref="TileBlockLocation"/>s.
        /// </summary>
        public void ClearSelection()
        {
            foreach (var location in SelectedLocations)
                location.IsSelected = false;

            SelectedLocations.Clear();
        }

        /// <summary>
        /// Removes the <see cref="Tile"/> instance on each
        /// selected location (see <see cref="SelectedLocations"/>).
        /// </summary>
        public void Clear()
        {
            foreach (var location in SelectedLocations)
            {
                location.Tile = null;
            }
        }

        /// <summary>
        /// Applies the <see cref="SelectedCatalogItemInstance"/> to
        /// each selected location (see <see cref="SelectedLocations"/>).
        /// </summary>
        public void Fill()
        {
            if (_selectedCatalogItem == null)
                return;

            foreach (var location in SelectedLocations)
            {
                if (_selectedCatalogItem.IsTileType)
                {
                    location.Tile = ((Tile)SelectedCatalogItemInstance).DeepClone();
                }
                else if (_selectedCatalogItem.IsEntityType)
                {
                    var entity = ((Entity)SelectedCatalogItemInstance).Clone();
                    var supportingTile = new PathTile { Entity = entity };
                    entity.Owner = supportingTile;

                    location.Tile = supportingTile;

                    // TODO: Enable drawing entities on top of existing tiles
                    // Not sure if we can use CanAttachAsync() to test if the tile
                    // accepts the entity...
                }
            }
        }
    }
}
