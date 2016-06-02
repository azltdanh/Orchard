using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class ExchangeTypePartHandler : ContentHandler
    {
        public ExchangeTypePartHandler(IRepository<ExchangeTypePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
