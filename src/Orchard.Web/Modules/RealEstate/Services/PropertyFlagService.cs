using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPropertyFlagService : IDependency
    {
        bool VerifyPropertyFlagUnicity(string flagName);
        bool VerifyPropertyFlagUnicity(int id, string flagName);
    }

    public class PropertyFlagService : IPropertyFlagService
    {
        private readonly IContentManager _contentManager;

        public PropertyFlagService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyPropertyFlagUnicity(string flagName)
        {
            return
                !_contentManager.Query<PropertyFlagPart, PropertyFlagPartRecord>()
                    .Where(flag => flag.Name == flagName)
                    .List()
                    .Any();
        }

        public bool VerifyPropertyFlagUnicity(int id, string flagName)
        {
            return
                !_contentManager.Query<PropertyFlagPart, PropertyFlagPartRecord>()
                    .Where(flag => flag.Name == flagName)
                    .List()
                    .Any(flag => flag.Id != id);
        }
    }
}