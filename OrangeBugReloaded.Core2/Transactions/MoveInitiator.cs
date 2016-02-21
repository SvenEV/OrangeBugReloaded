namespace OrangeBugReloaded.Core.Transactions
{
    public struct MoveInitiator
    {
        public static MoveInitiator Empty { get; } = new MoveInitiator(null, Point.Zero);

        public object Object { get; }
        public Point Position { get; }

        public MoveInitiator(object obj, Point position)
        {
            Object = obj;
            Position = position;
        }
    }
}
