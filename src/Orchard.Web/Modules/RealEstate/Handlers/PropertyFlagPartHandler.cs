using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PropertyFlagPartHandler : ContentHandler
    {
        public PropertyFlagPartHandler(IRepository<PropertyFlagPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
