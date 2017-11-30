using System;

namespace Lykke.Service.IcoExRate.Models
{
    public class RateResponse
    {
        public string Market { get; set; }
        public string Pair { get; set; }
        public decimal? Rate { get; set; }
        public DateTime? CreatedUtc { get; set; }
    }

    public class AverageRateResponse
    {
        public string Pair { get; set; }
        public decimal? AverageRate { get; set; }
        public RateResponse[] Rates { get; set; }
    }
}
