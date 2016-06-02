using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PaymentMethodPartHandler : ContentHandler
    {
        public PaymentMethodPartHandler(IRepository<PaymentMethodPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
