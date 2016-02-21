using System.Threading.Tasks;

namespace OrangeBugReloaded.Core.ClientServer
{
    public interface IGameServer : IGameServerStub
    {
        IGameplayMap Map { get; }
    }
}
