namespace OrangeBugReloaded.Core.Events
{
    public class ChunkAddedEvent : IGameEvent
    {
        public IChunk Chunk { get; }

        public ChunkAddedEvent(IChunk chunk)
        {
            Chunk = chunk;
        }
    }
}
