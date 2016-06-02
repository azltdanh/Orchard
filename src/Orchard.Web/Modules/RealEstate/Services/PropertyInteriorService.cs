using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPropertyInteriorService : IDependency
    {
        bool VerifyPropertyInteriorUnicity(string interiorName);
        bool VerifyPropertyInteriorUnicity(int id, string interiorName);
    }

    public class PropertyInteriorService : IPropertyInteriorService
    {
        private readonly IContentManager _contentManager;

        public PropertyInteriorService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyPropertyInteriorUnicity(string interiorName)
        {
            return
                !_contentManager.Query<PropertyInteriorPart, PropertyInteriorPartRecord>()
                    .Where(a => a.Name == interiorName)
                    .List()
                    .Any();
        }

        public bool VerifyPropertyInteriorUnicity(int id, string interiorName)
        {
            return
                !_contentManager.Query<PropertyInteriorPart, PropertyInteriorPartRecord>()
                    .Where(a => a.Name == interiorName)
                    .List()
                    .Any(a => a.Id != id);
        }
    }
}