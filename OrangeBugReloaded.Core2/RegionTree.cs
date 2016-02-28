using System.Collections.Generic;
using System.Linq;

namespace OrangeBugReloaded.Core
{
    public class RegionTree : IRegionTree
    {
        private readonly Dictionary<int, RegionInfo> _regions;

        public RegionInfo DefaultRegion => _regions[RegionInfo.Default.Id];

        public RegionTree()
        {
            // Add a default region
            _regions = new Dictionary<int, RegionInfo>();
            _regions.Add(RegionInfo.Default.Id, RegionInfo.Default);
        }

        public RegionTree(IEnumerable<RegionInfo> regions)
        {
            _regions = regions.ToDictionary(r => r.Id);

            // Ensure there's a default region
            if (!_regions.ContainsKey(RegionInfo.Default.Id))
                _regions.Add(RegionInfo.Default.Id, RegionInfo.Default);
        }

        public RegionInfo this[int id]
        {
            get
            {
                RegionInfo region;
                return _regions.TryGetValue(id, out region) ? region : RegionInfo.Empty;
            }
        }

        /// <summary>
        /// Gets regions that are derived from the region with the
        /// specified ID.
        /// </summary>
        /// <param name="id">Region ID</param>
        /// <returns>List of regions</returns>
        public IReadOnlyCollection<RegionInfo> GetDerivedRegions(int id)
        {
            return _regions.Values.Where(r => r.Parent == id).ToArray();
        }

        /// <summary>
        /// Gets the base region of the region with the specified ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RegionInfo GetBaseRegion(int id)
        {
            return this[this[id].Parent];
        }
    }
}
