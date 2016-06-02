using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class LocationDistrictPartHandler : ContentHandler
    {
        public LocationDistrictPartHandler(IRepository<LocationDistrictPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
