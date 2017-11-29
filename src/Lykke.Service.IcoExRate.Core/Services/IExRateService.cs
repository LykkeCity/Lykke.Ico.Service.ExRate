using Lykke.Service.IcoExRate.Core.Domain;
using System;
using System.Threading.Tasks;

namespace Lykke.Service.IcoExRate.Core.Services
{
    public interface IExRateService
    {
        Task<IRate> GetRate(Pair pair, Market market, DateTime created);
        Task SaveRate(Pair pair, Market market);
    }
}
