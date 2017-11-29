using System;
using Common.Log;

namespace Lykke.Service.IcoExRate.Client
{
    public class IcoExRateClient : IIcoExRateClient, IDisposable
    {
        private readonly ILog _log;

        public IcoExRateClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
