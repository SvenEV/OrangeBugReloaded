using Newtonsoft.Json;

namespace OrangeBugReloaded.Core.ClientServer
{
    public struct VersionedPoint
    {
        public static VersionedPoint Empty { get; } = new VersionedPoint(Point.Zero, -1);

        public Point Position { get; }

        public int Version { get; }

        [JsonConstructor]
        public VersionedPoint(Point position, int version)
        {
            Position = position;
            Version = version;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(VersionedPoint))
                return false;

            var other = (VersionedPoint)obj;
            return other.Position == Position && other.Version == Version;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;
                hash = hash * 23 + Position.GetHashCode();
                hash = hash * 23 + Version.GetHashCode();
                return hash;
            }
        }

        public static implicit operator Point(VersionedPoint p) => p.Position;
    }
}
