using System.IO;
using System.Linq;
using Encodeous.DirtyProxy;

var scraper = new ProxyScraper(ProxyScraper.DefaultList, checkProxies: false);
var proxies = await scraper.ScrapeAsync();

await File.WriteAllLinesAsync("validProxies.txt", proxies.ValidProxies.Select(x=>x.ToString()));
await File.WriteAllLinesAsync("validSources.txt", proxies.ValidSources.Select(x=>x.Trim()));
await File.WriteAllLinesAsync("allProxies.txt", proxies.Proxies.Select(x=>x.ToString()));