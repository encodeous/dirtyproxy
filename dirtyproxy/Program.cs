using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Encodeous.DirtyProxy;

ProxyScraper.CheckTasks = 1;

var scraper = new ProxyScraper(ProxyScraper.DefaultList, async proxy =>
{
    try
    {
        var wc = new WebClient();
        wc.Proxy = new WebProxy(proxy.ToString());
        wc.Headers[HttpRequestHeader.UserAgent] = ProxyScraper.DefaultAgent;
        // timeout in 10 seconds
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        cts.Token.Register(wc.CancelAsync);
        await wc.OpenReadTaskAsync("https://google.com");
        cts.Dispose();
        return true;
    }
    catch
    {
        return false;
    }
});
var proxies = await scraper.ScrapeAsync();

await File.WriteAllLinesAsync("validProxies.txt", proxies.ValidProxies.Select(x=>x.ToString()));
await File.WriteAllLinesAsync("validSources.txt", proxies.ValidSources.Select(x=>x.Trim()));
await File.WriteAllLinesAsync("allProxies.txt", proxies.Proxies.Select(x=>x.ToString()));