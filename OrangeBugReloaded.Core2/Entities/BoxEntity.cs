using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Entities
{
    /// <summary>
    /// A crate that can be pushed around.
    /// </summary>
    public class BoxEntity : Entity
    {
        public static BoxEntity Default { get; } = new BoxEntity();

        private BoxEntity() { }

        public override Task DetachAsync(EntityDetachArgs e)
            => DetachByPushingAsync(e);
    }
}
