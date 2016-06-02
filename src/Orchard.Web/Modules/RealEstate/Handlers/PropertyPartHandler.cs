using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PropertyPartHandler : ContentHandler
    {
        public PropertyPartHandler(IRepository<PropertyPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
