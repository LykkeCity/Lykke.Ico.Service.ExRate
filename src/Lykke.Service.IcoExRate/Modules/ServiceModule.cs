using Autofac;
using Common.Log;
using Lykke.Service.IcoExRate.AzureRepositories.Rate;
using Lykke.Service.IcoExRate.Core.Services;
using Lykke.Service.IcoExRate.Core.Settings.ServiceSettings;
using Lykke.Service.IcoExRate.PeriodicalHandlers;
using Lykke.Service.IcoExRate.Services;
using Lykke.SettingsReader;

namespace Lykke.Service.IcoExRate.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<IcoExRateSettings> _settings;
        private readonly ILog _log;

        public ServiceModule(IReloadingManager<IcoExRateSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var connectionStringManager = _settings.ConnectionString(x => x.Db.DataConnString);

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterType<RateRepository>()
                .As<IRateRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<ExRateService>()
                .As<IExRateService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Markets))
                .SingleInstance();

            builder.RegisterType<LykkeBtcUsdPeriodicalHandler>()
                .As<IStartable>()
                .AutoActivate()
                .WithParameter("period", _settings.CurrentValue.Markets.Period)
                .SingleInstance();

            builder.RegisterType<LykkeEthUsdPeriodicalHandler>()
                .As<IStartable>()
                .AutoActivate()
                .WithParameter("period", _settings.CurrentValue.Markets.Period)
                .SingleInstance();

            builder.RegisterType<KrakenBtcUsdPeriodicalHandler>()
                .As<IStartable>()
                .AutoActivate()
                .WithParameter("period", _settings.CurrentValue.Markets.Period)
                .SingleInstance();

            builder.RegisterType<KrakenEthUsdPeriodicalHandler>()
                .As<IStartable>()
                .AutoActivate()
                .WithParameter("period", _settings.CurrentValue.Markets.Period)
                .SingleInstance();

            builder.RegisterType<BitfinexBtcUsdPeriodicalHandler>()
                .As<IStartable>()
                .AutoActivate()
                .WithParameter("period", _settings.CurrentValue.Markets.Period)
                .SingleInstance();

            builder.RegisterType<BitfinexEthUsdPeriodicalHandler>()
                .As<IStartable>()
                .AutoActivate()
                .WithParameter("period", _settings.CurrentValue.Markets.Period)
                .SingleInstance();
        }
    }
}
