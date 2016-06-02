using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class LocationWardPartHandler : ContentHandler
    {
        public LocationWardPartHandler(IRepository<LocationWardPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
