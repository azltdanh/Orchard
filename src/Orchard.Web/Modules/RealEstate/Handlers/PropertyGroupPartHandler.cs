using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PropertyGroupPartHandler : ContentHandler
    {
        public PropertyGroupPartHandler(IRepository<PropertyGroupPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
