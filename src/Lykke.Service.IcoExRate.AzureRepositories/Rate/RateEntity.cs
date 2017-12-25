using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.IcoExRate.Core.Domain;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Lykke.Service.IcoExRate.AzureRepositories.Rate
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    public class RateEntity : AzureTableEntity, IRate
    {
        [IgnoreProperty]
        public DateTime CreatedUtc
        {
            get => this.Timestamp.UtcDateTime;
        }

        public decimal? ExchangeRate { get; set; }
    }
}
