using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.Transactions
{
    /// <summary>
    /// A transaction that can simulate entity move operations and record changes
    /// caused by such operations.
    /// </summary>
    public class EntityMoveTransaction : MapTransaction, IMapTransactionWithMoveSupport
    {
        /// <inheritdoc/>
        public Stack<EntityMoveInfo> Moves { get; } = new Stack<EntityMoveInfo>();
        
        /// <inheritdoc/>
        public EntityMoveInfo CurrentMove => Moves.Count == 0 ? null : Moves.Peek();

        public EntityMoveTransaction(IGameplayMap map) : base(map)
        {
        }

        internal EntityMoveTransaction(EntityMoveTransaction previousTransaction) : base(previousTransaction)
        {
        }

        /// <inheritdoc/>
        public Task<bool> MoveAsync(Point sourcePosition, Point targetPosition)
        {
            return ((IGameplayMap)_map).MoveAsync(sourcePosition, targetPosition, this);
        }
    }
}
