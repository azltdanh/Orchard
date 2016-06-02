using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PropertyTypeConstructionPartHandler : ContentHandler
    {
        public PropertyTypeConstructionPartHandler(IRepository<PropertyTypeConstructionPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
