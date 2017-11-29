using Lykke.Service.IcoExRate.Core.Settings.ServiceSettings;
using Lykke.Service.IcoExRate.Core.Settings.SlackNotifications;

namespace Lykke.Service.IcoExRate.Core.Settings
{
    public class AppSettings
    {
        public IcoExRateSettings IcoExRateService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
