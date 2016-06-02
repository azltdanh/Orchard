using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPaymentMethodService : IDependency
    {
        bool VerifyPaymentMethodUnicity(string paymentMethodName);
        bool VerifyPaymentMethodUnicity(int id, string paymentMethodName);
    }

    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IContentManager _contentManager;

        public PaymentMethodService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyPaymentMethodUnicity(string paymentMethodName)
        {
            return
                !_contentManager.Query<PaymentMethodPart, PaymentMethodPartRecord>()
                    .Where(a => a.Name == paymentMethodName)
                    .List()
                    .Any();
        }

        public bool VerifyPaymentMethodUnicity(int id, string paymentMethodName)
        {
            return
                !_contentManager.Query<PaymentMethodPart, PaymentMethodPartRecord>()
                    .Where(a => a.Name == paymentMethodName)
                    .List()
                    .Any(a => a.Id != id);
        }
    }
}