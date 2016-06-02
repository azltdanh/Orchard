using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PropertyLocationPartHandler : ContentHandler
    {
        public PropertyLocationPartHandler(IRepository<PropertyLocationPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
