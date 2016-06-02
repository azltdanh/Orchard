using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;
namespace RealEstate.Handlers
{
    public class NewsVideoPartHandler : ContentHandler
    {
        public NewsVideoPartHandler(IRepository<NewsVideoPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
