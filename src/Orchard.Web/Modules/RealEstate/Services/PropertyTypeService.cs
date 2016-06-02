using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPropertyTypeService : IDependency
    {
        bool VerifyPropertyTypeUnicity(string typeName);
        bool VerifyPropertyTypeUnicity(int id, string typeName);
        IEnumerable<PropertyTypeGroupPartRecord> GetGroups();
    }

    public class PropertyTypeService : IPropertyTypeService
    {
        private readonly IContentManager _contentManager;

        public PropertyTypeService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyPropertyTypeUnicity(string typeName)
        {
            return
                !_contentManager.Query<PropertyTypePart, PropertyTypePartRecord>()
                    .Where(type => type.Name == typeName)
                    .List()
                    .Any();
        }

        public bool VerifyPropertyTypeUnicity(int id, string typeName)
        {
            return
                !_contentManager.Query<PropertyTypePart, PropertyTypePartRecord>()
                    .Where(type => type.Name == typeName)
                    .List()
                    .Any(type => type.Id != id);
        }

        public IEnumerable<PropertyTypeGroupPartRecord> GetGroups()
        {
            return
                _contentManager.Query<PropertyTypeGroupPart, PropertyTypeGroupPartRecord>().List().Select(a => a.Record);
        }
    }
}