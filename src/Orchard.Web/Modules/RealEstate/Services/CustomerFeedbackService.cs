using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface ICustomerFeedbackService : IDependency
    {
        bool VerifyCustomerFeedbackUnicity(string feedbackName);
        bool VerifyCustomerFeedbackUnicity(int id, string feedbackName);
    }

    public class CustomerFeedbackService : ICustomerFeedbackService
    {
        private readonly IContentManager _contentManager;

        public CustomerFeedbackService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyCustomerFeedbackUnicity(string feedbackName)
        {
            return
                !_contentManager.Query<CustomerFeedbackPart, CustomerFeedbackPartRecord>()
                    .Where(s => s.Name == feedbackName)
                    .List()
                    .Any();
        }

        public bool VerifyCustomerFeedbackUnicity(int id, string feedbackName)
        {
            return
                !_contentManager.Query<CustomerFeedbackPart, CustomerFeedbackPartRecord>()
                    .Where(s => s.Name == feedbackName)
                    .List()
                    .Any(s => s.Id != id);
        }
    }
}