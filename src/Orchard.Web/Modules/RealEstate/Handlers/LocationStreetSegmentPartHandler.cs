using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class LocationStreetSegmentPartHandler : ContentHandler
    {
        public LocationStreetSegmentPartHandler(IRepository<LocationStreetSegmentPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
