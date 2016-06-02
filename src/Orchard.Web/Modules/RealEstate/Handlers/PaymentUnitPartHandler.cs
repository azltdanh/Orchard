using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PaymentUnitPartHandler : ContentHandler
    {
        public PaymentUnitPartHandler(IRepository<PaymentUnitPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
