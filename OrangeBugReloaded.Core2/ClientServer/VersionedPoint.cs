namespace OrangeBugReloaded.Core.ClientServer
{
    public struct VersionedPoint
    {
        public static VersionedPoint Empty { get; } = new VersionedPoint(Point.Zero, -1);

        public Point Position { get; }
        public int Version { get; }

        public VersionedPoint(Point position, int version)
        {
            Position = position;
            Version = version;
        }

        public static implicit operator Point(VersionedPoint p) => p.Position;

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(VersionedPoint))
                return false;

            var other = (VersionedPoint)obj;
            return other.Position == Position && other.Version == Version;
        }

        public override int GetHashCode()
            => new { Position, Version }.GetHashCode();
    }
}
