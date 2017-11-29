using System;

namespace Lykke.Service.IcoExRate.Core.Domain
{
    public interface IRate
    {
        DateTime CreatedUtc { get; }
        decimal? ExchangeRate { get; set; }
    }
}
