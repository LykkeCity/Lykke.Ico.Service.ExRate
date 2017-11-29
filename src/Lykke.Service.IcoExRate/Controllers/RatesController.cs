using System;
using System.Net;
using Lykke.Service.IcoExRate.Core.Services;
using Lykke.Service.IcoExRate.Models;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.IcoExRate.Core.Domain;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

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

        private async Task<RateResponse> GetRateResponse(Pair pair, Market market, DateTime createdUtc)
        {
            var rate = await _exRateService.GetRate(pair, market, createdUtc);

            return new RateResponse
            {
                Name = Enum.GetName(typeof(Market), market),
                Rate = rate?.ExchangeRate,
                CreatedUtc = rate?.CreatedUtc
            };
        }
    }
}
