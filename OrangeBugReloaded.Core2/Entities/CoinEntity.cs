using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// A coin that can be collected by players.
    /// TODO: Collecting coins does not have any effect yet.
    /// </summary>
    public class CoinEntity : Entity
    {
        public static CoinEntity Default { get; } = new CoinEntity();

        public override Task DetachAsync(EntityDetachArgs e)
            => DetachByCollectingAsync(e);
    }
}
