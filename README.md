# DirtyProxy

[![Nuget Url](https://img.shields.io/nuget/v/Encodeous.DirtyProxy)](https://www.nuget.org/packages/Encodeous.DirtyProxy)

A quick and easy proxy scraper!

*That does not mean it lacks features* ;)

## Features

- High-Performance Asynchronous Scraping
- Highly Configurable
- Comes with proxy checker
- Comes with a default list of proxy sites
- Lightweight (~6% CPU usage on a 4-core i7)
- Filter unique proxies

## Usage

*Please note, enabling proxy checking (on by default) will take MUCH longer!*

### Using Default Parameters

```c#
var scraper = new ProxyScraper(ProxyScraper.DefaultList);
var proxies = await scraper.ScrapeAsync();

await File.WriteAllLinesAsync("validProxies.txt", proxies.ValidProxies.Select(x=>x.ToString()));
await File.WriteAllLinesAsync("validSources.txt", proxies.ValidSources.Select(x=>x.Trim()));
await File.WriteAllLinesAsync("allProxies.txt", proxies.Proxies.Select(x=>x.ToString()));
```

### Using Custom Proxy Source List

```c#
var sources = new[]
{
    "https://source.proxy.list",
    "https://other.source.proxy.list"
};
// You can use your own list, or the list included by default!
var scraper = new ProxyScraper(sources);

...
```

### Using Custom Proxy Checker

```c#
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

...
```

### Using Custom User Agent

```c#
// You can use any user agent you want!
var scraper = new ProxyScraper(ProxyScraper.DefaultList, "Your user agent");
var proxies = await scraper.ScrapeAsync();

...
```

### Using Custom Request Timeouts

```c#
var scraper = new ProxyScraper(ProxyScraper.DefaultList, checkTimeout: 5, scrapeTimeout: 2);
var proxies = await scraper.ScrapeAsync();

...
```


### Fast Scraping (Without proxy validation)

```c#
// Disable proxy checking
var scraper = new ProxyScraper(ProxyScraper.DefaultList, checkProxies: false);
var proxies = await scraper.ScrapeAsync();

...
```

### Custom Proxy Check URL

```c#
// Make sure the proxies can successfully connect to a url
var scraper = new ProxyScraper(ProxyScraper.DefaultList, checkUrl: "https://google.ca");
var proxies = await scraper.ScrapeAsync();

...
```

### Misc Configuration
```c#
// number of tasks for proxy checking (mainly waiting)
ProxyScraper.CheckTasks = 300;
```