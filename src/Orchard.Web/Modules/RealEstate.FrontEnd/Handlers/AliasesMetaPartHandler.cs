using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.FrontEnd.Models;

namespace RealEstate.FrontEnd.Handlers
{
    public class AliasesMetaPartHandler : ContentHandler
    {
        public AliasesMetaPartHandler(IRepository<AliasesMetaPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}