using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPropertyStatusService : IDependency
    {
        bool VerifyPropertyStatusUnicity(string statusName);
        bool VerifyPropertyStatusUnicity(int id, string statusName);
    }

    public class PropertyStatusService : IPropertyStatusService
    {
        private readonly IContentManager _contentManager;

        public PropertyStatusService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyPropertyStatusUnicity(string statusName)
        {
            return
                !_contentManager.Query<PropertyStatusPart, PropertyStatusPartRecord>()
                    .Where(a => a.Name == statusName)
                    .List()
                    .Any();
        }

        public bool VerifyPropertyStatusUnicity(int id, string statusName)
        {
            return
                !_contentManager.Query<PropertyStatusPart, PropertyStatusPartRecord>()
                    .Where(a => a.Name == statusName)
                    .List()
                    .Any(a => a.Id != id);
        }
    }
}