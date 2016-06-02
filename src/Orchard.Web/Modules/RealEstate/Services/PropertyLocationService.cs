using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPropertyLocationService : IDependency
    {
        bool VerifyPropertyLocationUnicity(string locationName);
        bool VerifyPropertyLocationUnicity(int id, string locationName);
    }

    public class PropertyLocationService : IPropertyLocationService
    {
        private readonly IContentManager _contentManager;

        public PropertyLocationService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyPropertyLocationUnicity(string locationName)
        {
            return
                !_contentManager.Query<PropertyLocationPart, PropertyLocationPartRecord>()
                    .Where(a => a.Name == locationName)
                    .List()
                    .Any();
        }

        public bool VerifyPropertyLocationUnicity(int id, string locationName)
        {
            return
                !_contentManager.Query<PropertyLocationPart, PropertyLocationPartRecord>()
                    .Where(a => a.Name == locationName)
                    .List()
                    .Any(a => a.Id != id);
        }
    }
}