using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPropertyLegalStatusService : IDependency
    {
        bool VerifyPropertyLegalStatusUnicity(string legalName);
        bool VerifyPropertyLegalStatusUnicity(int id, string legalName);
    }

    public class PropertyLegalStatusService : IPropertyLegalStatusService
    {
        private readonly IContentManager _contentManager;

        public PropertyLegalStatusService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyPropertyLegalStatusUnicity(string legalName)
        {
            return
                !_contentManager.Query<PropertyLegalStatusPart, PropertyLegalStatusPartRecord>()
                    .Where(a => a.Name == legalName)
                    .List()
                    .Any();
        }

        public bool VerifyPropertyLegalStatusUnicity(int id, string legalName)
        {
            return
                !_contentManager.Query<PropertyLegalStatusPart, PropertyLegalStatusPartRecord>()
                    .Where(a => a.Name == legalName)
                    .List()
                    .Any(a => a.Id != id);
        }
    }
}