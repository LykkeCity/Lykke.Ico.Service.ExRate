using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.SettingsReader;
using Common.Log;
using System.Linq;
using System.Collections.Generic;
using Lykke.Service.IcoExRate.Core.Domain;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.IcoExRate.AzureRepositories.Rate
{
    public class RateRepository : IRateRepository
    {
        private readonly INoSQLTableStorage<RateEntity> _tableLykkeBtcUsd;
        private readonly INoSQLTableStorage<RateEntity> _tableLykkeEthUsd;
        private readonly INoSQLTableStorage<RateEntity> _tableKrakenBtcUsd;
        private readonly INoSQLTableStorage<RateEntity> _tableKrakenEthUsd;
        private readonly INoSQLTableStorage<RateEntity> _tableBitfinexBtcUsd;
        private readonly INoSQLTableStorage<RateEntity> _tableBitfinexEthUsd;
        private static string GetPartitionKey() => DateTime.UtcNow.ToString("yyyy-MM-dd");
        private static string GetRowKey(DateTime created) => (DateTime.MaxValue.Ticks - created.ToUniversalTime().Ticks).ToString().PadLeft(19, '0');

        public RateRepository(IReloadingManager<string> connectionStringManager, ILog log)
        {
            _tableLykkeBtcUsd = AzureTableStorage<RateEntity>.Create(connectionStringManager, $"Rates{nameof(Market.Lykke)}{nameof(Pair.BTCUSD)}", log);
            _tableLykkeEthUsd = AzureTableStorage<RateEntity>.Create(connectionStringManager, $"Rates{nameof(Market.Lykke)}{nameof(Pair.ETHUSD)}", log);
            _tableKrakenBtcUsd = AzureTableStorage<RateEntity>.Create(connectionStringManager, $"Rates{nameof(Market.Kraken)}{nameof(Pair.BTCUSD)}", log);
            _tableKrakenEthUsd = AzureTableStorage<RateEntity>.Create(connectionStringManager, $"Rates{nameof(Market.Kraken)}{nameof(Pair.ETHUSD)}", log);
            _tableBitfinexBtcUsd = AzureTableStorage<RateEntity>.Create(connectionStringManager, $"Rates{nameof(Market.Bitfinex)}{nameof(Pair.BTCUSD)}", log);
            _tableBitfinexEthUsd = AzureTableStorage<RateEntity>.Create(connectionStringManager, $"Rates{nameof(Market.Bitfinex)}{nameof(Pair.ETHUSD)}", log);
        }

        public async Task<IRate> GetRateAsync(Pair pair, Market market, DateTime created)
        {
            var table = GetTable(pair, market);
            var query = new TableQuery<RateEntity>()
            {
                FilterString = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, GetRowKey(created)),
                TakeCount = 1
            };

            var page = new AzureStorage.Tables.Paging.PagingInfo { ElementCount = 1 };
            var result = await table.ExecuteQueryWithPaginationAsync(query, page);

            return result.FirstOrDefault();
        }

        public async Task SaveAsync(Pair pair, Market market, decimal? rate)
        {
            var table = GetTable(pair, market);
            var entity = new RateEntity
            {
                PartitionKey = GetPartitionKey(),
                RowKey = GetRowKey(DateTime.UtcNow),
                ExchangeRate = rate
            };

            await table.InsertAsync(entity);
        }

        private INoSQLTableStorage<RateEntity> GetTable(Pair pair, Market market)
        {
            switch (market)
            {
                case Market.Lykke:
                    switch (pair)
                    {
                        case Pair.BTCUSD:
                            return _tableLykkeBtcUsd;
                        case Pair.ETHUSD:
                            return _tableLykkeEthUsd;
                    }
                    break;
                case Market.Kraken:
                    switch (pair)
                    {
                        case Pair.BTCUSD:
                            return _tableKrakenBtcUsd;
                        case Pair.ETHUSD:
                            return _tableKrakenEthUsd;
                    }
                    break;
                case Market.Bitfinex:
                    switch (pair)
                    {
                        case Pair.BTCUSD:
                            return _tableBitfinexBtcUsd;
                        case Pair.ETHUSD:
                            return _tableBitfinexEthUsd;
                    }
                    break;
            }

            throw new Exception($"Not supported market: {Enum.GetName(typeof(Market), market)} and pair: {Enum.GetName(typeof(Pair), pair)}");
        }
    }
}
