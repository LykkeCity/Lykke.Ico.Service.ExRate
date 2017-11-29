using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.IcoExRate.Core.Services;
using Lykke.Service.IcoExRate.Core.Domain;

namespace Lykke.Service.IcoExRate.PeriodicalHandlers
{
    public class LykkeBtcUsdPeriodicalHandler : TimerPeriod
    {
        private readonly ILog _log;
        private readonly IExRateService _exRateService;

        public LykkeBtcUsdPeriodicalHandler(int period, ILog log, IExRateService exRateService) :
            base(nameof(LykkeBtcUsdPeriodicalHandler), (int)TimeSpan.FromSeconds(period).TotalMilliseconds, log)
        {
            _log = log;
            _exRateService = exRateService;
        }

        public override async Task Execute()
        {
            try
            {
                await _exRateService.SaveRate(Pair.BTCUSD, Market.Lykke);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(
                    nameof(LykkeBtcUsdPeriodicalHandler), 
                    nameof(Execute), 
                    "Failed to save rates for Lykke/BTCUSD", 
                    ex);
            }

            await Task.CompletedTask;
        }
    }
}
