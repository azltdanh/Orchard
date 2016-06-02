using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPropertyTypeGroupService : IDependency
    {
        bool VerifyPropertyTypeGroupUnicity(string typeGroupName);
        bool VerifyPropertyTypeGroupUnicity(int id, string typeGroupName);
    }

    public class PropertyTypeGroupService : IPropertyTypeGroupService
    {
        private readonly IContentManager _contentManager;

        public PropertyTypeGroupService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyPropertyTypeGroupUnicity(string typeGroupName)
        {
            return
                !_contentManager.Query<PropertyTypeGroupPart, PropertyTypeGroupPartRecord>()
                    .Where(typeGroup => typeGroup.Name == typeGroupName)
                    .List()
                    .Any();
        }

        public bool VerifyPropertyTypeGroupUnicity(int id, string typeGroupName)
        {
            return
                !_contentManager.Query<PropertyTypeGroupPart, PropertyTypeGroupPartRecord>()
                    .Where(typeGroup => typeGroup.Name == typeGroupName)
                    .List()
                    .Any(typeGroup => typeGroup.Id != id);
        }
    }
}