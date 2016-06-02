using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstateForum.Service.Models;


namespace RealEstateForum.Service.Handlers
{
    public class UnitInvestPartHandler : ContentHandler
    {
        public UnitInvestPartHandler(IRepository<UnitInvestPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}