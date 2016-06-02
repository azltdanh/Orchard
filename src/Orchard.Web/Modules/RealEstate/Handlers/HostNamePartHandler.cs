using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class HostNamePartHandler : ContentHandler
    {
        public HostNamePartHandler(IRepository<HostNamePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
