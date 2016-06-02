using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PropertyTypePartHandler : ContentHandler
    {
        public PropertyTypePartHandler(IRepository<PropertyTypePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
