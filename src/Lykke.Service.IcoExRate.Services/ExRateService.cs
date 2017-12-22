using Lykke.Service.IcoExRate.Core.Services;
using System;
using Lykke.Service.IcoExRate.Core.Domain;
using System.Threading.Tasks;
using Lykke.Service.IcoExRate.Core.Settings.ServiceSettings;
using Lykke.Service.IcoExRate.AzureRepositories.Rate;
using System.Net;
using Newtonsoft.Json;
using Common.Log;

namespace Lykke.Service.IcoExRate.Services
{
    public class ExRateService : IExRateService
    {
        private readonly ILog _log;
        private readonly IRateRepository _rateRepository;
        private readonly MarketsSettings _marketSettings;

        public ExRateService(ILog log, IRateRepository rateRepository, MarketsSettings marketSettings)
        {
            _log = log;
            _rateRepository = rateRepository;
            _marketSettings = marketSettings;
        }

        public async Task<IRate> GetRate(Pair pair, Market market, DateTime created)
        {
            return await _rateRepository.GetRateAsync(pair, market, created);
        }

        public async Task SaveRate(Pair pair, Market market)
        {
            var url = GetUrl(pair, market);

            var response = await GetResponse(url);
            if (response == null)
            {
                var latestRate = await GetRate(pair, market, DateTime.UtcNow);
                if (latestRate != null)
                {
                    var diff = DateTime.UtcNow - latestRate.CreatedUtc;
                    if (diff.TotalMinutes > 10)
                    {
                        throw new Exception($"Failed to get rate for more than {diff.TotalMinutes} mins");
                    }
                }

                return;
            }
            if (response == string.Empty)
            {
                await _log.WriteInfoAsync(nameof(SaveRate), 
                    $"Url: {url}",
                    $"Empty response");
                return;
            }

            var rate = GetRate(pair, market, response);
            if (rate == 0)
            {
                await _log.WriteInfoAsync(nameof(SaveRate),
                    $"Url: {url}, Response: {response}",
                    $"0 rate is recieved");
                return;
            }

            await _rateRepository.SaveAsync(pair, market, rate);
        }

        private decimal GetRate(Pair pair, Market market, string response)
        {
            var rateStr = "";

            switch (market)
            {
                case Market.Lykke:
                    rateStr = response.Trim();
                    break;
                case Market.Kraken:
                    rateStr = GeRateKraken(pair, response);
                    break;
                case Market.Bitfinex:
                    rateStr = GeRateBitfinex(pair, response);
                    break;
            }

            if (Decimal.TryParse(rateStr, out var result))
            {
                return result;
            }

            throw new Exception($"Failed to get rate. pair: {Enum.GetName(typeof(Pair), pair)}, " +
                $"market: {Enum.GetName(typeof(Market), market)}, rateStr: {rateStr}, response: {response}");
        }

        private string GeRateBitfinex(Pair pair, string response)
        {
            try
            {
                var bitfinexObject = JsonConvert.DeserializeObject<dynamic>(response);

                switch (pair)
                {
                    case Pair.BTCUSD:
                        return bitfinexObject.last_price;
                    case Pair.ETHUSD:
                        return bitfinexObject.last_price;
                    default:
                        throw new Exception($"Not supported pair: {Enum.GetName(typeof(Pair), pair)}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get Bitfinex rate for response: {response}", ex);
            }
        }

        private string GeRateKraken(Pair pair, string response)
        {
            try
            {
                var krakenObject = JsonConvert.DeserializeObject<dynamic>(response);

                switch (pair)
                {
                    case Pair.BTCUSD:
                        return krakenObject.result.XXBTZUSD.c[0];
                    case Pair.ETHUSD:
                        return krakenObject.result.XETHZUSD.c[0];
                    default:
                        throw new Exception($"Not supported pair: {Enum.GetName(typeof(Pair), pair)}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get Kraken rate for response: {response}", ex);
            }
        }

        private string GetUrl(Pair pair, Market market)
        {
            switch (market)
            {
                case Market.Lykke:
                    switch (pair)
                    {
                        case Pair.BTCUSD:
                            return _marketSettings.LykkeUrl.Replace("{Pair}", "BTCUSD");
                        case Pair.ETHUSD:
                            return _marketSettings.LykkeUrl.Replace("{Pair}", "ETHUSD");
                    }
                    break;
                case Market.Kraken:
                    switch (pair)
                    {
                        case Pair.BTCUSD:
                            return _marketSettings.KrakenUrl.Replace("{Pair}", "XBTUSD");
                        case Pair.ETHUSD:
                            return _marketSettings.KrakenUrl.Replace("{Pair}", "ETHUSD");
                    }
                    break;
                case Market.Bitfinex:
                    switch (pair)
                    {
                        case Pair.BTCUSD:
                            return _marketSettings.BitfinexUrl.Replace("{Pair}", "BTCUSD");
                        case Pair.ETHUSD:
                            return _marketSettings.BitfinexUrl.Replace("{Pair}", "ETHUSD");
                    }
                    break;
            }

            throw new Exception($"Not supported market: {Enum.GetName(typeof(Market), market)}");
        }

        private async Task<string> GetResponse(string url)
        {
            using (var client = new TimedWebClient(10000))
            {
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                try
                {
                    return client.DownloadString(url);
                }
                catch (WebException ex)
                {
                    await _log.WriteInfoAsync(nameof(GetResponse),
                            $"erl: {url}",
                            $"Error. Status Code: {ex.Status}. Stack: {ex.ToString()}");

                    return null;
                }
            }
        }

        private class TimedWebClient : WebClient
        {
            private readonly int _timeout;

            public TimedWebClient(int timeout)
            {
                _timeout = timeout;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var objWebRequest = base.GetWebRequest(address);
                objWebRequest.Timeout = _timeout;
                return objWebRequest;
            }
        }
    }
}
