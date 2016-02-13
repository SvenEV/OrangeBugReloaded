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
    }
}
