using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPropertyFileService : IDependency
    {
        bool VerifyPropertyFileUnicity(string fileName);
        bool VerifyPropertyFileUnicity(int id, string fileName);
    }

    public class PropertyFileService : IPropertyFileService
    {
        private readonly IContentManager _contentManager;

        public PropertyFileService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyPropertyFileUnicity(string fileName)
        {
            return
                !_contentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                    .Where(file => file.Name == fileName)
                    .List()
                    .Any();
        }

        public bool VerifyPropertyFileUnicity(int id, string fileName)
        {
            return
                !_contentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                    .Where(file => file.Name == fileName)
                    .List()
                    .Any(file => file.Id != id);
        }
    }
}