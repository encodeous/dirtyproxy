# DirtyProxy

A quick and easy proxy scraper!

*That does not mean it lacks features ;)*

## Features

- High Performance Asynchronous Scraping
- Highly Configurable
- Comes with proxy checker
- Comes with a default list of proxy sites
- Lightweight (~6% CPU usage on 4-core i7)
- Filter unique proxies

## Usage

*Please note, enabling proxy checking (on by default) will take MUCH longer!*

### Using Default Parameters

```csharp
var scraper = new DirtyProxy(DirtyProxy.DefaultList);
var proxies = await scraper.ScrapeAsync();

await File.WriteAllLinesAsync("validProxies.txt", proxies.ValidProxies.Select(x=>x.ToString()));
await File.WriteAllLinesAsync("validSources.txt", proxies.ValidSources.Select(x=>x.Trim()));
await File.WriteAllLinesAsync("allProxies.txt", proxies.Proxies.Select(x=>x.ToString()));
```

### Using Custom Proxy Source List

```csharp
var sources = new[]
{
    "https://source.proxy.list",
    "https://other.source.proxy.list"
};
// You can use your own list, or the list included by default!
var scraper = new DirtyProxy(sources);

...
```

### Using Custom User Agent

```csharp
// You can use any user agent you want!
var scraper = new DirtyProxy(DirtyProxy.DefaultList, 
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62");
var proxies = await scraper.ScrapeAsync();

---
```

### Using Custom Request Timeouts

```csharp
var scraper = new DirtyProxy(DirtyProxy.DefaultList, checkTimeout: 5, scrapeTimeout: 2);
var proxies = await scraper.ScrapeAsync();

...
```


### Fast Scraping (Without proxy validation)

```csharp
// Disable proxy checking
var scraper = new DirtyProxy(DirtyProxy.DefaultList, 
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62",
    false);
var proxies = await scraper.ScrapeAsync();

...
```

### Custom Proxy Check URL

```csharp
// Make sure the proxies can successfully connect to a url
var scraper = new DirtyProxy(DirtyProxy.DefaultList, 
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62",
    true,
    "https://google.ca");
var proxies = await scraper.ScrapeAsync();

...
```