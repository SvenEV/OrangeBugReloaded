namespace OrangeBugReloaded.Core.ClientServer.Net
{
    public class NetGameServerInfo
    {
        public string RemoteAddress { get; }
        public string RemotePort { get; }

        public NetGameServerInfo(string remoteAddress, string remotePort)
        {
            RemoteAddress = remoteAddress;
            RemotePort = remotePort;
        }
    }
}
