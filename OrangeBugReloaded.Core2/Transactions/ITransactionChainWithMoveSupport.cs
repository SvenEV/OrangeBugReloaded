namespace OrangeBugReloaded.Core.Transactions
{
    public interface ITransactionChainWithMoveSupport : ITransactionChain, ISupportsMove
    {
        /// <summary>
        /// Gets the latest transaction in the chain.
        /// </summary>
        new ITransactionWithMoveSupport CurrentTransaction { get; }

        /// <summary>
        /// Gets the first/earliest transaction in the chain.
        /// </summary>
        new ITransactionWithMoveSupport RootTransaction { get; }

        /// <summary>
        /// An arbitrary tag value that can be used to mark the object that
        /// has caused the chain of transactions.
        /// This value is shared across all transactions within a chain.
        /// </summary>
        object Initiator { get; set; }
    }
}
