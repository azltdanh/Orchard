using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PropertyFilePartHandler : ContentHandler
    {
        public PropertyFilePartHandler(IRepository<PropertyFilePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
