using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PaymentExchangePartHandler : ContentHandler
    {
        public PaymentExchangePartHandler(IRepository<PaymentExchangePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
