using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class AdsPaymentHistoryPartHandler : ContentHandler
    {
        public AdsPaymentHistoryPartHandler(IRepository<AdsPaymentHistoryPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
