using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class CustomerPurposePartHandler : ContentHandler
    {
        public CustomerPurposePartHandler(IRepository<CustomerPurposePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
