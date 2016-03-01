using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Events;
using OrangeBugReloaded.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer
{
    public abstract class GameClientBase : IGameClientStub
    {
        private readonly Subject<IGameEvent> _events = new Subject<IGameEvent>();
        private IGameServerStub _serverStub;

        public Point PlayerPosition { get; private set; }

        public GameClientInfo PlayerInfo { get; }

        public IGameplayMap Map { get; private set; }

        public IObservable<IGameEvent> Events => _events;

        public GameClientBase(GameClientInfo playerInfo)
        {
            PlayerInfo = playerInfo;
        }

        /// <summary>
        /// Initializes the client after it has successfully joined a game on
        /// a game server.
        /// </summary>
        /// <param name="spawnPosition">
        /// The position on the map where the server has created the <see cref="PlayerEntity"/>
        /// for the player
        /// </param>
        /// <param name="serverStub">
        /// An object that can invoke methods on the game server
        /// </param>
        /// <returns></returns>
        protected async Task InitializeAsync(Point spawnPosition, IGameServerStub serverStub)
        {
            _serverStub = serverStub;
            PlayerPosition = spawnPosition;
            Map = new Map(new RemoteChunkStorage(PlayerInfo.PlayerId, _serverStub));

            // Load chunk at spawn position
            await Map.GetAsync(spawnPosition);
        }

        public async Task DisconnectAsync()
        {
            if (_serverStub == null)
                return;

            await _serverStub.LeaveAsync(PlayerInfo.PlayerId);
            _serverStub = null;
            PlayerPosition = Point.Zero;
            Map = null;
        }

        public async Task<bool> MoveAsync(Point sourcePosition, Point targetPosition)
        {
            var source = await Map.GetAsync(sourcePosition);
            var target = await Map.GetAsync(targetPosition);
            var initiator = new MoveInitiator(source.Tile.Entity, sourcePosition);
            var transaction = new TransactionWithMoveSupport(initiator);
            var result = await Map.MoveAsync(sourcePosition, targetPosition, transaction);

            var request = new RemoteMoveRequest(
                new VersionedPoint(sourcePosition, source.Version),
                new VersionedPoint(targetPosition, target.Version),
                result.Transaction.Changes.Select(c => new VersionedPoint(c.Key, c.Value.Version)));

            var remoteMoveResult = await _serverStub.MoveAsync(request, PlayerInfo.PlayerId);

            if (remoteMoveResult.IsSuccessful)
            {
                // Commit transaction
                await transaction.CommitAsync(Map, remoteMoveResult.NewVersion);
                AnalyzeEvents(transaction.Events);
                return true;
            }
            else
            {
                // Our map was not up to date => apply updated chunks
                foreach (var kvp in remoteMoveResult.ChunkUpdates)
                    await ApplyChunkAsync(kvp.Value, kvp.Key);
            }

            return false;
        }

        public async Task<bool> MovePlayerAsync(Point direction)
        {
            direction.EnsureDirection();
            return await MoveAsync(PlayerPosition, PlayerPosition + direction);
        }

        public async Task OnUpdate(ClientUpdate e)
        {
            foreach (var change in e.TileUpdates)
            {
                await Map.SetAsync(change.Position, change.TileInfo);
            }

            foreach (var ev in e.Events)
            {
                var moveEvent = ev as EntityMoveEvent;

                if (moveEvent != null && !Map.ChunkLoader.IsLoadedOrLoading(moveEvent.SourcePosition / Chunk.Size))
                {
                    // The entity comes from a chunk we have not loaded => spawn it first
                    Map.Emit(new EntitySpawnEvent(moveEvent.SourcePosition, moveEvent.Target.Entity));
                }

                Map.Emit(ev);

                if (moveEvent != null && !Map.ChunkLoader.IsLoadedOrLoading(moveEvent.TargetPosition / Chunk.Size))
                {
                    // The entity moves into an area we have not loaded => despawn it after the move
                    Map.Emit(new EntityDespawnEvent(moveEvent.TargetPosition, moveEvent.Source.Entity));
                }
            }

            AnalyzeEvents(e.Events);

            foreach (var ev in e.Events)
                _events.OnNext(ev);
        }

        private async Task ApplyChunkAsync(IChunk chunk, Point index)
        {
            // TODO: It would be better to directly replace the chunk via ChunkLoader
            for (var y = 0; y < Chunk.Size; y++)
            {
                for (var x = 0; x < Chunk.Size; x++)
                {
                    var position = index * Chunk.Size + new Point(x, y);
                    await Map.SetAsync(position, chunk[x, y]);
                    await Map.SetMetadataAsync(position, chunk.GetMetadata(new Point(x, y)));
                }
            }
        }

        private async void AnalyzeEvents(IEnumerable<IGameEvent> events)
        {
            var playerMoveEvent = events.OfType<EntityMoveEvent>()
                .FirstOrDefault(ev => (ev.Source.Entity as PlayerEntity)?.PlayerId == PlayerInfo.PlayerId);

            if (playerMoveEvent != null)
            {
                // It is the player that has been moved
                PlayerPosition = playerMoveEvent.TargetPosition;

                // Make sure we have that part of the map loaded
                await Map.GetAsync(PlayerPosition);
            }
        }
    }
}
