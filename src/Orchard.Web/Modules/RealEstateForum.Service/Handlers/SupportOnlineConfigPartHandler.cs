using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service.Handlers
{
    public class SupportOnlineConfigPartHandler : ContentHandler
    {
        public SupportOnlineConfigPartHandler(IRepository<SupportOnlineConfigPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}