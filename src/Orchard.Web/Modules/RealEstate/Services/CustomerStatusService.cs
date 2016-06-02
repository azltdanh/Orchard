using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface ICustomerStatusService : IDependency
    {
        bool VerifyCustomerStatusUnicity(string statusName);
        bool VerifyCustomerStatusUnicity(int id, string statusName);
    }

    public class CustomerStatusService : ICustomerStatusService
    {
        private readonly IContentManager _contentManager;

        public CustomerStatusService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyCustomerStatusUnicity(string statusName)
        {
            return
                !_contentManager.Query<CustomerStatusPart, CustomerStatusPartRecord>()
                    .Where(s => s.Name == statusName)
                    .List()
                    .Any();
        }

        public bool VerifyCustomerStatusUnicity(int id, string statusName)
        {
            return
                !_contentManager.Query<CustomerStatusPart, CustomerStatusPartRecord>()
                    .Where(s => s.Name == statusName)
                    .List()
                    .Any(s => s.Id != id);
        }
    }
}