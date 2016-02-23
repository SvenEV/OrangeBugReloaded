﻿using System.Collections.Generic;
using System.Threading;

namespace OrangeBugReloaded.Core.ClientServer
{
    public class ClientConnection
    {
        public IGameClientInfo ClientInfo { get; }

        public IGameClientStub ClientStub { get; }
        
        public HashSet<Point> LoadedChunks { get; } = new HashSet<Point>();

        public SemaphoreSlim MoveSemaphore { get; } = new SemaphoreSlim(1);

        public ClientConnection(IGameClientInfo clientInfo, IGameClientStub clientStub)
        {
            ClientInfo = clientInfo;
            ClientStub = clientStub;
        }
    }
}
