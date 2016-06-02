using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class LocationStreetPartHandler : ContentHandler
    {
        public LocationStreetPartHandler(IRepository<LocationStreetPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
