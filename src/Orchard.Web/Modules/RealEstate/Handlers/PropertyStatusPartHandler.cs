using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PropertyStatusPartHandler : ContentHandler
    {
        public PropertyStatusPartHandler(IRepository<PropertyStatusPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
