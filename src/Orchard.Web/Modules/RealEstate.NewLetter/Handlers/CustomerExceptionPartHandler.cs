using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.NewLetter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.NewLetter.Handlers
{
    public class CustomerExceptionPartHandler : ContentHandler
    {
        public CustomerExceptionPartHandler(IRepository<CustomerEmailExceptionPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}