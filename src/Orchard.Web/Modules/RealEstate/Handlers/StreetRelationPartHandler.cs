using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class StreetRelationPartHandler : ContentHandler
    {
        public StreetRelationPartHandler(IRepository<StreetRelationPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
