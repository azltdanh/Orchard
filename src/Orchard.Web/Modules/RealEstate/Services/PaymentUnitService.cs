using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPaymentUnitService : IDependency
    {
        bool VerifyPaymentUnitUnicity(string paymentUnitName);
        bool VerifyPaymentUnitUnicity(int id, string paymentUnitName);
    }

    public class PaymentUnitService : IPaymentUnitService
    {
        private readonly IContentManager _contentManager;

        public PaymentUnitService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyPaymentUnitUnicity(string paymentUnitName)
        {
            return
                !_contentManager.Query<PaymentUnitPart, PaymentUnitPartRecord>()
                    .Where(a => a.Name == paymentUnitName)
                    .List()
                    .Any();
        }

        public bool VerifyPaymentUnitUnicity(int id, string paymentUnitName)
        {
            return
                !_contentManager.Query<PaymentUnitPart, PaymentUnitPartRecord>()
                    .Where(a => a.Name == paymentUnitName)
                    .List()
                    .Any(a => a.Id != id);
        }
    }
}