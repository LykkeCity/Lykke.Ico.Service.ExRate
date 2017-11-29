using System;

namespace Lykke.Service.IcoExRate.Models
{
    public class RateResponse
    {
        public string Name { get; set; }
        public decimal? Rate { get; set; }
        public DateTime? CreatedUtc { get; set; }
    }
}
