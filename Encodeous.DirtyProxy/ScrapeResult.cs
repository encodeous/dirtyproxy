using System.Collections.Generic;
using System.Net;

namespace Encodeous.DirtyProxy
{
    public class ScrapeResult
    {
        /// <summary>
        /// A list of valid proxy sources. A valid source is a source that contains at least 1 proxy address
        /// </summary>
        public List<string> ValidSources { get; init; }
        /// <summary>
        /// A list of ALL proxies, some of these may or may not be valid
        /// </summary>
        public List<IPEndPoint> Proxies { get; init; }
        /// <summary>
        /// A list of all valid proxies, checked against the specified url
        /// </summary>
        public List<IPEndPoint> ValidProxies { get; init; }
    }
}