using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PropertyInteriorPartHandler : ContentHandler
    {
        public PropertyInteriorPartHandler(IRepository<PropertyInteriorPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
