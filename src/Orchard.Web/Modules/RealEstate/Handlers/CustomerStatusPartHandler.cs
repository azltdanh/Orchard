using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class CustomerStatusPartHandler : ContentHandler
    {
        public CustomerStatusPartHandler(IRepository<CustomerStatusPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
