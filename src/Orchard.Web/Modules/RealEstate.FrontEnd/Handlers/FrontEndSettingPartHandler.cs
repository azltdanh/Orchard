using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.FrontEnd.Models;

namespace RealEstate.FrontEnd.Handlers
{
    public class FrontEndSettingPartHandler : ContentHandler
    {
        public FrontEndSettingPartHandler(IRepository<FrontEndSettingPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}