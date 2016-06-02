using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface ICustomerPurposeService : IDependency
    {
        bool VerifyCustomerPurposeUnicity(string purposeName);
        bool VerifyCustomerPurposeUnicity(int id, string purposeName);
    }

    public class CustomerPurposeService : ICustomerPurposeService
    {
        private readonly IContentManager _contentManager;

        public CustomerPurposeService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyCustomerPurposeUnicity(string purposeName)
        {
            return
                !_contentManager.Query<CustomerPurposePart, CustomerPurposePartRecord>()
                    .Where(s => s.Name == purposeName)
                    .List()
                    .Any();
        }

        public bool VerifyCustomerPurposeUnicity(int id, string purposeName)
        {
            return
                !_contentManager.Query<CustomerPurposePart, CustomerPurposePartRecord>()
                    .Where(s => s.Name == purposeName)
                    .List()
                    .Any(s => s.Id != id);
        }
    }
}