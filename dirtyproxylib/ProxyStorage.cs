using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Channels;

namespace dirtyproxylib
{
    public class ProxyStorage
    {
        public Channel<IPEndPoint> VerificationQueue { get; set; } = Channel.CreateBounded<IPEndPoint>(100);
        public Channel<DiscoveryWrapper> DiscoveryQueue { get; set; } = Channel.CreateBounded<DiscoveryWrapper>(10);
        public ConcurrentQueue<IPEndPoint> VerifiedProxies { get; set; } = new();
        public ConcurrentQueue<IPEndPoint> Proxies { get; set; } = new();
    }
}