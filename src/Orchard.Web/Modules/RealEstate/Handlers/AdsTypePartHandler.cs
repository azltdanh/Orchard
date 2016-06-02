using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class AdsTypePartHandler : ContentHandler
    {
        public AdsTypePartHandler(IRepository<AdsTypePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
