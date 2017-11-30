using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.IcoExRate.Client.AutorestClient;
using Lykke.Service.IcoExRate.Client.AutorestClient.Models;
using System.Collections.Generic;

namespace Lykke.Service.IcoExRate.Client
{
    public class IcoExRateClient : IIcoExRateClient, IDisposable
    {
        private readonly ILog _log;
        private IIcoExRateAPI _service;


        public IcoExRateClient(string serviceUrl, ILog log)
        {
            _service = new IcoExRateAPI(new Uri(serviceUrl));
            _log = log;
        }

        public void Dispose()
        {
            if (_service == null)
                return;
            _service.Dispose();
            _service = null;
        }

        public async Task<AverageRateResponse> GetAverageRate(Pair pair, DateTime dateTimeUtc)
        {
            return await _service.ApiRatesByPairByDateTimeUtcAverageGetAsync(pair, dateTimeUtc);
        }

        public async Task<IList<AverageRateResponse>> GetAverageRates(DateTime dateTimeUtc)
        {
            return await _service.ApiRatesByDateTimeUtcAverageGetAsync(dateTimeUtc);
        }

        public async Task<RateResponse> GetRate(Market market, Pair pair, DateTime dateTimeUtc)
        {
            return await _service.ApiRatesByMarketByPairByDateTimeUtcGetAsync(market, pair, dateTimeUtc);
        }

        public async Task<IList<RateResponse>> GetRates(Pair pair, DateTime dateTimeUtc)
        {
            return await _service.ApiRatesByPairByDateTimeUtcGetAsync(pair, dateTimeUtc);
        }
    }
}
