using System.Threading.Tasks;

namespace Lykke.Service.IcoExRate.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}