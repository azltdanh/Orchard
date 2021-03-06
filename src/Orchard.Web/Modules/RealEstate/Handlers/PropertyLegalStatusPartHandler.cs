using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PropertyLegalStatusPartHandler : ContentHandler
    {
        public PropertyLegalStatusPartHandler(IRepository<PropertyLegalStatusPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
