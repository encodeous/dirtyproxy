using System.IO;
using System.Linq;
using dirtyproxylib;

// Configure custom timeouts
var scraper = new DirtyProxy(DirtyProxy.DefaultList, checkTimeout: 5, scrapeTimeout: 2);
var proxies = await scraper.ScrapeAsync();

await File.WriteAllLinesAsync("validProxies.txt", proxies.ValidProxies.Select(x=>x.ToString()));
await File.WriteAllLinesAsync("validSources.txt", proxies.ValidSources.Select(x=>x.Trim()));
await File.WriteAllLinesAsync("allProxies.txt", proxies.Proxies.Select(x=>x.ToString()));