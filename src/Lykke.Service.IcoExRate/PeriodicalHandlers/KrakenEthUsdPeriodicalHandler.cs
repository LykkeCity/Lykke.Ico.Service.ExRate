using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.IcoExRate.Core.Services;
using Lykke.Service.IcoExRate.Core.Domain;

namespace Lykke.Service.IcoExRate.PeriodicalHandlers
{
    public class KrakenEthUsdPeriodicalHandler : TimerPeriod
    {
        private readonly ILog _log;
        private readonly IExRateService _exRateService;

        public KrakenEthUsdPeriodicalHandler(int period, ILog log, IExRateService exRateService) :
            base(nameof(KrakenEthUsdPeriodicalHandler), (int)TimeSpan.FromSeconds(period).TotalMilliseconds, log)
        {
            _log = log;
            _exRateService = exRateService;
        }

        public override async Task Execute()
        {
            try
            {
                await _exRateService.SaveRate(Pair.ETHUSD, Market.Kraken);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(
                    nameof(KrakenEthUsdPeriodicalHandler),
                    nameof(Execute),
                    "Failed to save rates for Kraken/ETHUSD",
                    ex);
            }

            await Task.CompletedTask;
        }
    }
}
