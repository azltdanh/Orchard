using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPaymentExchangeService : IDependency
    {
        // ReSharper disable InconsistentNaming
        double GetLatestRateUSD();
        double GetLatestRateSJC();
        // ReSharper restore InconsistentNaming
    }

    public class PaymentExchangeService : IPaymentExchangeService
    {
        private readonly IContentManager _contentManager;

        public PaymentExchangeService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public double GetLatestRateUSD()
        {
            if (
                _contentManager.Query<PaymentExchangePart, PaymentExchangePartRecord>()
                    .Where(ex => ex.USD > 0)
                    .List()
                    .Any())
                return
                    _contentManager.Query<PaymentExchangePart, PaymentExchangePartRecord>()
                        .Where(ex => ex.USD > 0)
                        .List()
                        .OrderByDescending(ex => ex.Date)
                        .First()
                        .USD;

            return 0;
        }

        public double GetLatestRateSJC()
        {
            if (
                _contentManager.Query<PaymentExchangePart, PaymentExchangePartRecord>()
                    .Where(ex => ex.SJC > 0)
                    .List()
                    .Any())
                return
                    _contentManager.Query<PaymentExchangePart, PaymentExchangePartRecord>()
                        .Where(ex => ex.SJC > 0)
                        .List()
                        .OrderByDescending(ex => ex.Date)
                        .First()
                        .SJC;

            return 0;
        }
    }
}