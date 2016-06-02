using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service.Handlers
{
    public class AdsPriceConfigPartHandler : ContentHandler
    {
        public AdsPriceConfigPartHandler(IRepository<AdsPriceConfigPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}