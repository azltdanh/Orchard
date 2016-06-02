using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PropertyTypeGroupPartHandler : ContentHandler
    {
        public PropertyTypeGroupPartHandler(IRepository<PropertyTypeGroupPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
