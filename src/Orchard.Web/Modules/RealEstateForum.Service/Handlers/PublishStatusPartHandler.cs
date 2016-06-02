using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstateForum.Service.Models;


namespace RealEstateForum.Service.Handlers
{
    public class PublishStatusPartHandler : ContentHandler
    {
        public PublishStatusPartHandler(IRepository<PublishStatusPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}