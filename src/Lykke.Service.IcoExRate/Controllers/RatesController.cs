using System;
using System.Net;
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
        [ProducesResponseType(typeof(decimal?), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRate([Required] Market market, [Required] Pair pair, [Required] DateTime dateTimeUtc)
        {
            var result = await _exRateService.GetRate(pair, market, dateTimeUtc);

            return Ok(result?.ExchangeRate);
        }

        [HttpGet("{pair}/{dateTimeUtc}")]
        [ProducesResponseType(typeof(RateResponse[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllRatesByPairAndDateTime([Required] Pair pair, [Required] DateTime dateTimeUtc)
        {
            var rates = new RateResponse[]
            {
                await GetRateResponse(pair, Market.Lykke, dateTimeUtc),
                await GetRateResponse(pair, Market.Kraken, dateTimeUtc),
                await GetRateResponse(pair, Market.Bitfinex, dateTimeUtc)
            };

            return Ok(rates);
        }

        [HttpGet("{pair}/{dateTimeUtc}/average")]
        [ProducesResponseType(typeof(AverageRateResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAverageRateByPairAndDateTime([Required] Pair pair, [Required] DateTime dateTimeUtc)
        {
            return Ok(await GetAverageResponse(pair, dateTimeUtc));
        }

        [HttpGet("{pair}/latest")]
        [ProducesResponseType(typeof(RateResponse[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllLatestRatesByPair([Required] Pair pair)
        {
            var now = DateTime.UtcNow;

            var rates = new RateResponse[]
            {
                await GetRateResponse(pair, Market.Lykke, now),
                await GetRateResponse(pair, Market.Kraken, now),
                await GetRateResponse(pair, Market.Bitfinex, now)
            };

            return Ok(rates);
        }

        [HttpGet("{pair}/latest/average")]
        [ProducesResponseType(typeof(AverageRateResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLatestAverageRateByPairAndDateTime([Required] Pair pair)
        {
            return Ok(await GetAverageResponse(pair, DateTime.UtcNow));
        }

        [HttpGet("latest")]
        [ProducesResponseType(typeof(RateResponse[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllLatestRates()
        {
            var now = DateTime.UtcNow;

            var rates = new RateResponse[]
            {
                await GetRateResponse(Pair.BTCUSD, Market.Lykke, now),
                await GetRateResponse(Pair.BTCUSD, Market.Kraken, now),
                await GetRateResponse(Pair.BTCUSD, Market.Bitfinex, now),
                await GetRateResponse(Pair.ETHUSD, Market.Lykke, now),
                await GetRateResponse(Pair.ETHUSD, Market.Kraken, now),
                await GetRateResponse(Pair.ETHUSD, Market.Bitfinex, now)
            };

            return Ok(rates);
        }

        [HttpGet("latest/average")]
        [ProducesResponseType(typeof(AverageRateResponse[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetLatestAverageRateByPairAndDateTime()
        {
            var now = DateTime.UtcNow;

            var averages = new AverageRateResponse[]
            {
                await GetAverageResponse(Pair.BTCUSD, now),
                await GetAverageResponse(Pair.ETHUSD, now)
            };

            return Ok(averages);
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
