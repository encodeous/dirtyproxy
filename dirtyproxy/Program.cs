using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Encodeous.DirtyProxy;

ProxyScraper.CheckTasks = 3000;

var scraper = new ProxyScraper(ProxyScraper.DefaultList);
var proxies = await scraper.ScrapeAsync();

await File.WriteAllLinesAsync("validProxies.txt", proxies.ValidProxies.Select(x=>x.ToString()));
await File.WriteAllLinesAsync("validSources.txt", proxies.ValidSources.Select(x=>x.Trim()));
await File.WriteAllLinesAsync("allProxies.txt", proxies.Proxies.Select(x=>x.ToString()));