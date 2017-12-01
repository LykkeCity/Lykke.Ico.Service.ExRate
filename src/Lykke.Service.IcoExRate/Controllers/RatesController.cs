using System;
using Lykke.Service.IcoExRate.Core.Services;
using Lykke.Service.IcoExRate.Models;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.IcoExRate.Core.Domain;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Lykke.Service.IcoExRate.Controllers
{
    [Route("api/rates")]
    public class RatesController : Controller
    {
        private readonly IExRateService _exRateService;

        public RatesController(IExRateService exRateService)
        {
            _exRateService = exRateService;
        }

        [HttpGet("{market}/{pair}/{dateTimeUtc}")]
        public async Task<RateResponse> GetRate([Required] Market market, [Required] Pair pair, [Required] DateTime dateTimeUtc)
        {
            return await GetRateResponse(pair, Market.Lykke, dateTimeUtc);
        }

        [HttpGet("{pair}/{dateTimeUtc}")]
        public async Task<RateResponse[]> GetAllRatesByPairAndDateTime([Required] Pair pair, [Required] DateTime dateTimeUtc)
        {
            return new RateResponse[]
            {
                await GetRateResponse(pair, Market.Lykke, dateTimeUtc),
                await GetRateResponse(pair, Market.Kraken, dateTimeUtc),
                await GetRateResponse(pair, Market.Bitfinex, dateTimeUtc)
            };
        }

        [HttpGet("{pair}/{dateTimeUtc}/average")]
        public async Task<AverageRateResponse> GetAverageRateByPairAndDateTime([Required] Pair pair, [Required] DateTime dateTimeUtc)
        {
            return await GetAverageResponse(pair, dateTimeUtc);
        }

        [HttpGet("{dateTimeUtc}")]
        public async Task<RateResponse[]> GetAllRatesByDateTime([Required] DateTime dateTimeUtc)
        {
            return new RateResponse[]
            {
                await GetRateResponse(Pair.BTCUSD, Market.Lykke, dateTimeUtc),
                await GetRateResponse(Pair.BTCUSD, Market.Kraken, dateTimeUtc),
                await GetRateResponse(Pair.BTCUSD, Market.Bitfinex, dateTimeUtc),
                await GetRateResponse(Pair.ETHUSD, Market.Lykke, dateTimeUtc),
                await GetRateResponse(Pair.ETHUSD, Market.Kraken, dateTimeUtc),
                await GetRateResponse(Pair.ETHUSD, Market.Bitfinex, dateTimeUtc)
            };
        }

        [HttpGet("{dateTimeUtc}/average")]
        public async Task<AverageRateResponse[]> GetAllAverageRatesByDateTime([Required] DateTime dateTimeUtc)
        {
            return new AverageRateResponse[]
            {
                await GetAverageResponse(Pair.BTCUSD, dateTimeUtc),
                await GetAverageResponse(Pair.ETHUSD, dateTimeUtc)
            };
        }

        private async Task<AverageRateResponse> GetAverageResponse(Pair pair, DateTime dateTimeUtc)
        {
            dateTimeUtc = dateTimeUtc.ToUniversalTime();

            var rates = new RateResponse[]
            {
                await GetRateResponse(pair, Market.Lykke, dateTimeUtc),
                await GetRateResponse(pair, Market.Kraken, dateTimeUtc),
                await GetRateResponse(pair, Market.Bitfinex, dateTimeUtc)
            };

            // remove rates that do not have rate and are older by 10 minutes
            var validRates = rates
                .Where(f => f.Rate.HasValue && f.CreatedUtc.HasValue && dateTimeUtc.Subtract(f.CreatedUtc.Value).Minutes < 10)
                .OrderBy(f => f.Rate)
                .ToList();

            if (!validRates.Any())
            {
                return null;
            }

            if (validRates.Count() >= 3)
            {
                // remove more distant rate
                var firtDiff = validRates[1].Rate - validRates[0].Rate;
                var lastDiff = validRates[validRates.Count() - 1].Rate - validRates[validRates.Count() - 2].Rate;

                if (firtDiff > lastDiff)
                {
                    validRates.Remove(validRates.First());
                }
                else
                {
                    validRates.Remove(validRates.Last());
                }
            }

            return new AverageRateResponse
            {
                Pair = Enum.GetName(typeof(Pair), pair),
                AverageRate = validRates.Average(f => f.Rate),
                Rates = rates
            };
        }

        private async Task<RateResponse> GetRateResponse(Pair pair, Market market, DateTime createdUtc)
        {
            var rate = await _exRateService.GetRate(pair, market, createdUtc);

            return new RateResponse
            {
                Market = Enum.GetName(typeof(Market), market),
                Pair = Enum.GetName(typeof(Pair), pair),
                Rate = rate?.ExchangeRate,
                CreatedUtc = rate?.CreatedUtc
            };
        }
    }
}
