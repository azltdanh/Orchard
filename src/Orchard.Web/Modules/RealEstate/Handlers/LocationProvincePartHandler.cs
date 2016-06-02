using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class LocationProvincePartHandler : ContentHandler
    {
        public LocationProvincePartHandler(IRepository<LocationProvincePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
