using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPropertyTypeConstructionService : IDependency
    {
        bool VerifyPropertyTypeConstructionUnicity(string typeConstructionName, int propertyTypeId);
        bool VerifyPropertyTypeConstructionUnicity(int id, string typeConstructionName, int propertyTypeId);
        IEnumerable<PropertyTypeGroupPartRecord> GetPropertyGroups();
        IEnumerable<PropertyTypePartRecord> GetPropertyTypes();
        void SetAsDefaultInFloorsRange(PropertyTypeConstructionPart record);
    }

    public class PropertyTypeConstructionService : IPropertyTypeConstructionService
    {
        private readonly IContentManager _contentManager;

        public PropertyTypeConstructionService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyPropertyTypeConstructionUnicity(string typeConstructionName, int propertyTypeId)
        {
            return
                !_contentManager.Query<PropertyTypeConstructionPart, PropertyTypeConstructionPartRecord>()
                    .Where(r => r.Name == typeConstructionName && r.PropertyType.Id == propertyTypeId)
                    .List()
                    .Any();
        }

        public bool VerifyPropertyTypeConstructionUnicity(int id, string typeConstructionName, int propertyTypeId)
        {
            return
                !_contentManager.Query<PropertyTypeConstructionPart, PropertyTypeConstructionPartRecord>()
                    .Where(r => r.Name == typeConstructionName && r.PropertyType.Id == propertyTypeId)
                    .List()
                    .Any(r => r.Id != id);
        }

        public IEnumerable<PropertyTypeGroupPartRecord> GetPropertyGroups()
        {
            return
                _contentManager.Query<PropertyTypeGroupPart, PropertyTypeGroupPartRecord>().List().Select(a => a.Record);
        }

        public IEnumerable<PropertyTypePartRecord> GetPropertyTypes()
        {
            return _contentManager.Query<PropertyTypePart, PropertyTypePartRecord>().List().Select(a => a.Record);
        }

        public void SetAsDefaultInFloorsRange(PropertyTypeConstructionPart record)
        {
            IEnumerable<PropertyTypeConstructionPart> list =
                _contentManager.Query<PropertyTypeConstructionPart, PropertyTypeConstructionPartRecord>()
                    .Where(
                        r =>
                            r.PropertyType == record.PropertyType && r.MinFloor == record.MinFloor &&
                            r.MaxFloor == record.MaxFloor).List();

            foreach (PropertyTypeConstructionPart item in list)
            {
                item.IsDefaultInFloorsRange = false;
            }

            record.IsDefaultInFloorsRange = true;
        }
    }
}