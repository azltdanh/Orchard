using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PropertyAdvantagePartHandler : ContentHandler
    {
        public PropertyAdvantagePartHandler(IRepository<PropertyAdvantagePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
