using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPropertyAdvantageService : IDependency
    {
        bool VerifyPropertyAdvantageUnicity(string advantageName);
        bool VerifyPropertyAdvantageUnicity(int id, string advantageName);
    }

    public class PropertyAdvantageService : IPropertyAdvantageService
    {
        private readonly IContentManager _contentManager;

        public PropertyAdvantageService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyPropertyAdvantageUnicity(string advantageName)
        {
            return
                !_contentManager.Query<PropertyAdvantagePart, PropertyAdvantagePartRecord>()
                    .Where(a => a.Name == advantageName)
                    .List()
                    .Any();
        }

        public bool VerifyPropertyAdvantageUnicity(int id, string advantageName)
        {
            return
                !_contentManager.Query<PropertyAdvantagePart, PropertyAdvantagePartRecord>()
                    .Where(a => a.Name == advantageName)
                    .List()
                    .Any(a => a.Id != id);
        }
    }
}