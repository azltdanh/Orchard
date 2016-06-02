using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class AdsPaymentConfigPartHandler : ContentHandler
    {
        public AdsPaymentConfigPartHandler(IRepository<AdsPaymentConfigPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
