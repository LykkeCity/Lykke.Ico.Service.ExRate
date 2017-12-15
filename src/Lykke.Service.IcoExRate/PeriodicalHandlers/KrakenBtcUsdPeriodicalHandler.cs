using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.IcoExRate.Core.Services;
using Lykke.Service.IcoExRate.Core.Domain;

namespace Lykke.Service.IcoExRate.PeriodicalHandlers
{
    public class KrakenBtcUsdPeriodicalHandler : TimerPeriod
    {
        private readonly ILog _log;
        private readonly IExRateService _exRateService;

        public KrakenBtcUsdPeriodicalHandler(int period, ILog log, IExRateService exRateService) :
            base(nameof(KrakenBtcUsdPeriodicalHandler), (int)TimeSpan.FromSeconds(period).TotalMilliseconds, log)
        {
            _log = log;
            _exRateService = exRateService;
        }

        public override async Task Execute()
        {
            try
            {
                await _exRateService.SaveRate(Pair.BTCUSD, Market.Kraken);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(
                    nameof(KrakenBtcUsdPeriodicalHandler),
                    nameof(Execute),
                    "Exchange: Kraken, AssetPair: BTCUSD",
                    ex);
            }

            await Task.CompletedTask;
        }
    }
}
