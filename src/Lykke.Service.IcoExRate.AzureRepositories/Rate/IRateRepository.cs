using System;
using System.Threading.Tasks;
using Lykke.Service.IcoExRate.Core.Domain;

namespace Lykke.Service.IcoExRate.AzureRepositories.Rate
{
    public interface IRateRepository
    {
        Task<IRate> GetRateAsync(Pair pair, Market market, DateTime created);
        Task SaveAsync(Pair pair, Market market, decimal? rate);
    }
}
