namespace OrangeBugReloaded.Core.Events
{
    public class ChunkRemovedEvent : IGameEvent
    {
        public IChunk Chunk { get; }

        public ChunkRemovedEvent(IChunk chunk)
        {
            Chunk = chunk;
        }
    }
}
