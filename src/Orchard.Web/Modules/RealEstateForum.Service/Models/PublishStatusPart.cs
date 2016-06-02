using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;

namespace RealEstateForum.Service.Models
{
    public class PublishStatusPartRecord : ContentPartRecord
    {
        public virtual string Name { get; set; }
    }
    public class PublishStatusPart : ContentPart<PublishStatusPartRecord>
    {
        public string Name
        {
            get { return Retrieve(r=>r.Name); }
            set { Store(r=>r.Name,value); }
        }
    }
}