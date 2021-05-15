using System;
using System.IO;
using System.Net.Http;

namespace Encodeous.DirtyProxy
{
    public class DiscoveryWrapper
    {
        public DiscoveryWrapper(string url, Action<string> callback)
        {
            Url = url;
            Callback = callback;
        }

        public string Url { get; }
        public Action<string> Callback { get; }
    }
}