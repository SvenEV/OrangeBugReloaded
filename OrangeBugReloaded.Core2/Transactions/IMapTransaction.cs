namespace OrangeBugReloaded.Core.Transactions
{
    public interface IMapTransaction : IReadOnlyMapTransaction, IMap
    {
        void Cancel();
    }
}
