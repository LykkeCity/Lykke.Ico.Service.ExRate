using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.IcoExRate.Core.Services;
using Lykke.Service.IcoExRate.Core.Domain;

namespace Lykke.Service.IcoExRate.PeriodicalHandlers
{
    public class BitfinexBtcUsdPeriodicalHandler : TimerPeriod
    {
        private readonly ILog _log;
        private readonly IExRateService _exRateService;

        public BitfinexBtcUsdPeriodicalHandler(int period, ILog log, IExRateService exRateService) :
            base(nameof(BitfinexBtcUsdPeriodicalHandler), (int)TimeSpan.FromSeconds(period).TotalMilliseconds, log)
        {
            _log = log;
            _exRateService = exRateService;
        }

        public override async Task Execute()
        {
            try
            {
                await _exRateService.SaveRate(Pair.BTCUSD, Market.Bitfinex);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(
                    nameof(BitfinexBtcUsdPeriodicalHandler),
                    nameof(Execute),
                    "Failed to save rates for Bitfinex/BTCUSD",
                    ex);
            }

            await Task.CompletedTask;
        }
    }
}
