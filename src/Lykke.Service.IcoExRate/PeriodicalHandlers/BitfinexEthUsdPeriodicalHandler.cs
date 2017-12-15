using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.IcoExRate.Core.Services;
using Lykke.Service.IcoExRate.Core.Domain;

namespace Lykke.Service.IcoExRate.PeriodicalHandlers
{
    public class BitfinexEthUsdPeriodicalHandler : TimerPeriod
    {
        private readonly ILog _log;
        private readonly IExRateService _exRateService;

        public BitfinexEthUsdPeriodicalHandler(int period, ILog log, IExRateService exRateService) :
            base(nameof(BitfinexEthUsdPeriodicalHandler), (int)TimeSpan.FromSeconds(period).TotalMilliseconds, log)
        {
            _log = log;
            _exRateService = exRateService;
        }

        public override async Task Execute()
        {
            try
            {
                await _exRateService.SaveRate(Pair.ETHUSD, Market.Bitfinex);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(
                    nameof(BitfinexEthUsdPeriodicalHandler),
                    nameof(Execute),
                    "Exchange: Bitfinex, AssetPair: ETHUSD",
                    ex);
            }

            await Task.CompletedTask;
        }
    }
}
