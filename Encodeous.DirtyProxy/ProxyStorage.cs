using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Channels;

namespace Encodeous.DirtyProxy
{
    public class ProxyStorage
    {
        internal Channel<IPEndPoint> VerificationQueue { get; set; } = Channel.CreateBounded<IPEndPoint>(1000);
        internal ConcurrentDictionary<IPEndPoint, byte> UniqueProxies { get; set; } = new();
        internal Channel<DiscoveryWrapper> DiscoveryQueue { get; set; } = Channel.CreateBounded<DiscoveryWrapper>(100);
        internal ConcurrentQueue<IPEndPoint> VerifiedProxies { get; set; } = new();
        internal ConcurrentQueue<IPEndPoint> Proxies { get; set; } = new();
    }
}