using System;
using System.Threading.Tasks;
using Lykke.Service.IcoExRate.Client.AutorestClient.Models;
using System.Collections.Generic;

namespace Lykke.Service.IcoExRate.Client
{
    public interface IIcoExRateClient
    {
        Task<RateResponse> GetRate(Market market, Pair pair, DateTime dateTimeUtc);
        Task<IList<RateResponse>> GetRates(Pair pair, DateTime dateTimeUtc);
        Task<AverageRateResponse> GetAverageRate(Pair pair, DateTime dateTimeUtc);
        Task<IList<AverageRateResponse>> GetAverageRates(DateTime dateTimeUtc);
    }
}
