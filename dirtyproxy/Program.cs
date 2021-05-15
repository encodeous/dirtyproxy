using System;
using System.IO;
using System.Linq;
using dirtyproxylib;

var prox = new DirtyProxy(DirtyProxy.DefaultList);
var proxies = await prox.ScrapeAsync();

await File.WriteAllLinesAsync("validProxies.txt", proxies.ValidProxies.Select(x=>x.ToString()));
await File.WriteAllLinesAsync("validSources.txt", proxies.ValidSources.Select(x=>x.Trim()));
await File.WriteAllLinesAsync("allProxies.txt", proxies.Proxies.Select(x=>x.ToString()));