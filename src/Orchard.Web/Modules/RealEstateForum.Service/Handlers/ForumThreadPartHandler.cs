using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstateForum.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateForum.Service.Handlers
{
    public class ForumThreadPartHandler : ContentHandler
    {
        public ForumThreadPartHandler(IRepository<ForumThreadPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}