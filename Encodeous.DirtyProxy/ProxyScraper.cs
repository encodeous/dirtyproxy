using System;
using System.Buffers.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Encodeous.DirtyProxy
{
    public class ProxyScraper : IDisposable
    {
        /// <summary>
        /// Number of tasks to run for proxy checking. This is a high number due to the fact that these tasks are mostly waiting
        /// </summary>
        public static int CheckTasks = 300;
        /// <summary>
        /// Default user agent
        /// </summary>
        public const string DefaultAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
        // Stored in base 64 to reduce LOC
        /// <summary>
        /// A list of valid sources (as of 5/15/2021)
        /// </summary>
        public static string[] DefaultList { get; } =
            Encoding.UTF8.GetString(Convert.FromBase64String("aHR0cHM6Ly9mcmVlLXByb3h5LWxpc3QubmV0LwpodHRwOi8vcHJveHlkYi5uZXQvCmh0dHA6Ly93d3cuYWxpdmVwcm94eS5jb20vc29ja3M1LWxpc3QvCm" +
                                                             "h0dHA6Ly93d3cuY3liZXJzeW5kcm9tZS5uZXQvcGxhLmh0bWwKaHR0cDovL3d3dy5wcm94ei5jb20vcHJveHlfbGlzdF9jYV8wLmh0bWwKaHR0cDovL3d3dy5wcm9" +
                                                             "4ei5jb20vcHJveHlfbGlzdF9oaWdoX2Fub255bW91c18wLmh0bWwKaHR0cDovL3Byb3h5LmlwY24ub3JnL3Byb3h5bGlzdDIuaHRtbApodHRwOi8vdG9ydnBuLmNv" +
                                                             "bS9wcm94eWxpc3QuaHRtbApodHRwOi8vd3d3LnByb3h6LmNvbS9wcm94eV9saXN0X2Fub255bW91c191c18wLmh0bWwKaHR0cDovL3d3dy5wcm94ei5jb20vcHJve" +
                                                             "HlfbGlzdF9jbl9zc2xfMC5odG1sCmh0dHA6Ly93d3cucHJveHouY29tL3Byb3h5X2xpc3RfanBfMC5odG1sCmh0dHA6Ly93d3cucHJveHouY29tL3Byb3h5X2xpc3" +
                                                             "RfdWtfMC5odG1sCmh0dHA6Ly9kb2dkZXYubmV0L1Byb3h5L1VTP3BvcnQ9ODAKaHR0cDovL3d3dy5hdG9taW50ZXJzb2Z0LmNvbS9wcm9kdWN0cy9hbGl2ZS1wcm9" +
                                                             "4eS9wcm94eS1saXN0LwpodHRwOi8vd3d3LmFsaXZlcHJveHkuY29tL2Zhc3Rlc3QtcHJveGllcy8KaHR0cDovL3d3dy5hdG9taW50ZXJzb2Z0LmNvbS9hbm9ueW1v" +
                                                             "dXNfcHJveHlfbGlzdApodHRwOi8vd3d3LnByb3h6LmNvbS9wcm94eV9saXN0X2ZyXzAuaHRtbApodHRwOi8vd3d3LmF0b21pbnRlcnNvZnQuY29tL2hpZ2hfYW5vb" +
                                                             "nltaXR5X2VsaXRlX3Byb3h5X2xpc3QKaHR0cDovL2RvZ2Rldi5uZXQvUHJveHkvYWxsCmh0dHA6Ly93d3cucHJveHlsaXN0cy5uZXQvCmh0dHA6Ly93d3cuaHR0cH" +
                                                             "R1bm5lbC5nZS9Qcm94eUxpc3RGb3JGcmVlLmFzcHgKaHR0cDovL3d3dy5wcm94eWxpc3RzLm5ldC9wcm94eWxpc3Quc2h0bWw/SFRUUApodHRwOi8vYW5vbi1wcm9" +
                                                             "4eS5ydS98aHRtbHwwCmh0dHA6Ly9wcm94aWVzLm15LXByb3h5LmNvbS9wcm94eS1saXN0LTEuaHRtbApodHRwOi8vZ2xvYmFscHJveGllcy5ibG9nc3BvdC5jb20v" +
                                                             "Cmh0dHA6Ly9wcm94aWVzLm15LXByb3h5LmNvbS9wcm94eS1saXN0LTIuaHRtbApodHRwOi8vYW5vbi1wcm94eS5ydS8KaHR0cDovL3d3dy5zc2xwcm94aWVzLm9yZ" +
                                                             "y8KaHR0cDovL3d3dy5zb2NrczI0Lm9yZy9mZWVkcy9wb3N0cy9kZWZhdWx0Cmh0dHA6Ly93d3cucHJveHlsaXN0cy5uZXQvaHR0cC50eHQKaHR0cDovL2FhOC5uYX" +
                                                             "JvZC5ydS9pbmRleC8wLTkKaHR0cDovL3d3dy5wcm94eWxpc3RzLm5ldC9odHRwX2hpZ2hhbm9uLnR4dApodHRwOi8vcHJveHlsaXN0cy5uZXQvaHR0cC50eHQKaHR" +
                                                             "0cDovL2ZyZWUtcHJveHktbGlzdC5uZXQvYW5vbnltb3VzLXByb3h5Lmh0bWwKaHR0cDovL3Byb3h5bGlzdHMubmV0L2h0dHBfaGlnaGFub24udHh0Cmh0dHA6Ly9h" +
                                                             "YjU3LnJ1L2Rvd25sb2Fkcy9wcm94eWxpc3QudHh0Cmh0dHA6Ly93d3cudXMtcHJveHkub3JnLwpodHRwczovL3Jhdy5naXRodWJ1c2VyY29udGVudC5jb20vY2xhc" +
                                                             "mtldG0vcHJveHktbGlzdC9tYXN0ZXIvcHJveHktbGlzdC50eHQKaHR0cDovL2ZyZWUtc29ja3MyNC5ibG9nc3BvdC5pbi8KaHR0cDovL3Byb3h5NTAtNTAuYmxvZ3" +
                                                             "Nwb3QuaW4vCmh0dHA6Ly9nbG9iYWxwcm94aWVzLmJsb2dzcG90LmNvbS9zZWFyY2gvbGFiZWwvVVMlMjBQcm94aWVzCmh0dHA6Ly9mcmVlcHJlbWl1bXByb3h5LmJ" +
                                                             "sb2dzcG90LmNvbQpodHRwOi8vYWE4Lm5hcm9kLnJ1L2luZGV4LzAtMTAKaHR0cDovL3Byb3h5c2VhcmNoZXIuc291cmNlZm9yZ2UubmV0L1Byb3h5JTIwTGlzdC5w" +
                                                             "aHA/dHlwZT1zb2NrcwpodHRwOi8vcHJveHlzZWFyY2hlci5zb3VyY2Vmb3JnZS5uZXQvUHJveHklMjBMaXN0LnBocCUzRnR5cGUlM0RodHRwCmh0dHA6Ly9yb290a" +
                                                             "mF6ei5jb20vcHJveGllcy9wcm94aWVzLnR4dApodHRwczovL2NoaW5hcHJveHlsaXN0LndvcmRwcmVzcy5jb20vZmVlZC8KaHR0cDovL3NzbHByb3hpZXMyNC5ibG" +
                                                             "9nc3BvdC5ubC9mZWVkcy9wb3N0cy9kZWZhdWx0Cmh0dHA6Ly93d3cuc3NscHJveGllczI0LnRvcC9mZWVkcy9wb3N0cy9kZWZhdWx0Cmh0dHA6Ly9wcm94eS1oZWF" +
                                                             "2ZW4uYmxvZ3Nwb3QuY29tLwpodHRwOi8vc3NscHJveGllczI0LmJsb2dzcG90LmNhL2ZlZWRzL3Bvc3RzL2RlZmF1bHQKaHR0cDovL2FhOC5uYXJvZC5ydS9pbmRl" +
                                                             "eC8wLTgKaHR0cHM6Ly9mcmVlLXNvY2tzMjQuYmxvZ3Nwb3QuaW4vZmVlZHMvcG9zdHMvZGVmYXVsdD9hbHQ9cnNzCmh0dHA6Ly9mcmVlLXNvY2tzMjQuYmxvZ3Nwb" +
                                                             "3QuaW4vZmVlZHMvcG9zdHMvZGVmYXVsdD9hbHQ9cnNzCmh0dHA6Ly9hbGV4YS5scjJiLmNvbS9wcm94eWxpc3QudHh0Cmh0dHA6Ly9hYnNlbnRpdXMubmFyb2Qucn" +
                                                             "UvCmh0dHBzOi8vYXV0b3Byb3h5YmxvZy53b3JkcHJlc3MuY29tL2ZlZWQvCmh0dHA6Ly93d3cuY2hhbmdlaXBzLmNvbS8KaHR0cDovL21tbS1kb3dubG9hZHMuYXQ" +
                                                             "udWEvYmxvZwpodHRwOi8vZmVlZHMuZmVlZGJ1cm5lci5jb20vQW5vbnltb3VzRGFpbHlQcm94eUxpc3QKaHR0cDovL2ZyZWVwcm94eWxpc3RzZGFpbHkuYmxvZ3Nw" +
                                                             "b3QuaW4vZmVlZHMvcG9zdHMvZGVmYXVsdApodHRwOi8vcHJveHlzZXJ2ZXJsaXN0LTI0LmJsb2dzcG90LmNvbS9mZWVkcy9wb3N0cy9kZWZhdWx0Cmh0dHA6Ly9wc" +
                                                             "m94eS1odW50ZXIuYmxvZ3Nwb3QuY29tL2ZlZWRzL3Bvc3RzL2RlZmF1bHQ=")).Split("\n");
        private static Regex reg =
            new ("(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?):\\d{1,5}", RegexOptions.Compiled);
        private ProxyStorage _storage;
        private HttpClient _client;
        private CancellationTokenSource _appLifetimeSource;
        private CancellationToken _appLifetime;
        private string[] _proxySources;
        private string _checkUrl, _agent;
        private bool _isVerifying;
        private double _checkTimeout = 10;

        /// <summary>
        /// Configure a new Proxy Scraper
        /// </summary>
        /// <param name="sources">Proxy Lists to scrape from</param>
        /// <param name="userAgent">User agent used</param>
        /// <param name="checkProxies">Check if the proxies can connect to a url</param>
        /// <param name="checkUrl">Url to check against</param>
        /// <param name="scrapeTimeout">Timeout (seconds) for each proxy source</param>
        /// <param name="checkTimeout">Timeout (seconds) for each proxy check request</param>
        public ProxyScraper(string[] sources, string userAgent = DefaultAgent,
            bool checkProxies = true, string checkUrl = "http://www.youtube.com", double scrapeTimeout = 5, double checkTimeout = 5)
        {
            _checkTimeout = checkTimeout;
            _isVerifying = checkProxies;
            _checkUrl = checkUrl;
            _agent = userAgent;
            _proxySources = sources;
            _appLifetimeSource = new CancellationTokenSource();
            _appLifetime = _appLifetimeSource.Token;
            _storage = new ProxyStorage();
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(scrapeTimeout);
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(_agent);
            if (checkProxies)
            {
                StartTasks(IsValid);
            }
            else
            {
                StartTasks(null);
            }
        }
        
        /// <summary>
        /// Configure a new Proxy Scraper with a custom proxy checker
        /// </summary>
        /// <param name="sources">Proxy Lists to scrape from</param>
        /// <param name="proxyChecker">Custom proxy checker</param>
        /// <param name="userAgent">User agent used</param>
        /// <param name="scrapeTimeout">Timeout (seconds) for each proxy source</param>
        public ProxyScraper(string[] sources, Func<IPEndPoint, ValueTask<bool>> proxyChecker, string userAgent = DefaultAgent, double scrapeTimeout = 5)
        {
            _isVerifying = true;
            _agent = userAgent;
            _proxySources = sources;
            _appLifetimeSource = new CancellationTokenSource();
            _appLifetime = _appLifetimeSource.Token;
            _storage = new ProxyStorage();
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(scrapeTimeout);
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(_agent);
            StartTasks(proxyChecker);
        }

        private void StartTasks(Func<IPEndPoint, ValueTask<bool>> proxyChecker)
        {
            if (proxyChecker is not null)
            {
                for (int i = 0; i < CheckTasks; i++)
                {
                    Task.Run(async () =>
                    {
                        while (!_appLifetime.IsCancellationRequested)
                        {
                            var prox = await _storage.VerificationQueue.Reader.ReadAsync(_appLifetime);
                            try
                            {
                                if (await proxyChecker(prox))
                                {
                                    _storage.VerifiedProxies.Enqueue(prox);
                                }
                                _storage.Proxies.Enqueue(prox);
                            }
                            catch
                            {
                            
                            }
                        }
                    });
                }
            }
            else
            {
                Task.Run(async () =>
                {
                    while (!_appLifetime.IsCancellationRequested)
                    {
                        var prox = await _storage.VerificationQueue.Reader.ReadAsync(_appLifetime);
                        _storage.Proxies.Enqueue(prox);
                    }
                });
            }
            for (int i = 0; i < 4; i++)
            {
                Task.Run(async () =>
                {
                    while (!_appLifetime.IsCancellationRequested)
                    {
                        var req = await _storage.DiscoveryQueue.Reader.ReadAsync(_appLifetime);
                        try
                        {
                            req.Callback(await _client.GetStringAsync(req.Url, _appLifetime));
                        }
                        catch
                        {
                            
                        }
                    }
                });
            }
        }

        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private volatile bool isAdding = false;

        /// <summary>
        /// Start scraping with the configured parameters. Each instance can only have one concurrent scraper.
        /// </summary>
        /// <param name="scrapeCount">The target number of valid proxies to scrape before exiting</param>
        /// <returns></returns>
        public async Task<ScrapeResult> ScrapeAsync(int scrapeCount = Int32.MaxValue)
        {
            await _semaphoreSlim.WaitAsync(_appLifetime);
            Console.WriteLine("Starting to Scrape Proxies... This will take a while.");
            var goodSources = new List<string>();
            
            _storage.VerifiedProxies = new ConcurrentQueue<IPEndPoint>();

            isAdding = true;
            
            var task = Task.Run(async () =>
            {
                while (!_appLifetime.IsCancellationRequested && (_storage.VerificationQueue.Reader.Count > 0 ||
                                                                 _storage.DiscoveryQueue.Reader.Count > 0 || isAdding) 
                                                             && _storage.VerifiedProxies.Count < scrapeCount)
                {
                    if (_isVerifying)
                    {
                        Console.WriteLine(
                            $"{_storage.Proxies.Count - _storage.VerifiedProxies.Count} down, {_storage.VerifiedProxies.Count} online, {_storage.VerificationQueue.Reader.Count} waiting to be verified.");
                    }
                    else
                    {
                        Console.WriteLine(
                            $"{_storage.Proxies.Count} proxies discovered");
                    }
                    await Task.Delay(1000, _appLifetime);
                }
                _appLifetimeSource.Cancel();
            }, _appLifetime);
            
            foreach (var src in _proxySources.Distinct())
            {
                await _storage.DiscoveryQueue.Writer.WriteAsync(new DiscoveryWrapper(src, async (x) =>
                {
                    if (await GetProxies(x))
                    {
                        goodSources.Add(src);
                    }
                }), _appLifetime);
            }

            isAdding = false;

            await task;

            return new ScrapeResult()
            {
                ValidProxies = _storage.VerifiedProxies.Distinct().ToList(),
                Proxies = _storage.Proxies.Distinct().ToList(),
                ValidSources = goodSources
            };
        }

        private async Task<bool> GetProxies(string html)
        {
            bool good = false;
            var match = reg.Match(html);
            while (match.Success)
            {
                good = true;
                try
                {
                    var prox = new IPEndPoint(IPAddress.Parse(match.Value.Split(":")[0]),
                        int.Parse(match.Value.Split(":")[1]));
                    if (!_storage.UniqueProxies.ContainsKey(prox))
                    {
                        while(!_storage.UniqueProxies.TryAdd(prox, 0)){}
                        await _storage.VerificationQueue.Writer.WriteAsync(prox, _appLifetime);
                    }
                }
                catch
                {
                    
                }
                match = match.NextMatch();
            }

            return good;
        }
        
        private async ValueTask<bool> IsValid(IPEndPoint prox)
        {
            try
            {
                WebClient wc = new();
                wc.Headers[HttpRequestHeader.UserAgent] = _agent;
                wc.Proxy = new WebProxy()
                {
                    Address = new Uri($"http://{prox.Address}:{prox.Port}"),
                };
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_checkTimeout));
                cts.Token.Register(wc.CancelAsync);
                var res = await wc.OpenReadTaskAsync(_checkUrl);
                await res.DisposeAsync();
                return true;
            }
            catch
            {
                try
                {
                    WebClient wc = new();
                    wc.Headers[HttpRequestHeader.UserAgent] = _agent;
                    wc.Proxy = new WebProxy()
                    {
                        Address = new Uri($"socks5://{prox.Address}:{prox.Port}"),
                    };
                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_checkTimeout));
                    cts.Token.Register(wc.CancelAsync);
                    var res = await wc.OpenReadTaskAsync(_checkUrl);
                    await res.DisposeAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        
        /// <summary>
        /// Stops any current operations, and destroys the client
        /// </summary>
        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            _appLifetimeSource.Cancel();
            _client?.Dispose();
            _appLifetimeSource?.Dispose();
            _semaphoreSlim?.Dispose();
        }
    }
}