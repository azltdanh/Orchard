using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.NewLetter.Models
{
    public class CustomerEmailExceptionPartRecord : ContentPartRecord
    {
        public virtual string EmailException { get; set; }
        public virtual string CodeRandom { get; set; }
        public virtual bool StatusActive { get; set; }
    }
    public class CustomerEmailExceptionPart :ContentPart<CustomerEmailExceptionPartRecord>
    {
        public string EmailException
        {
            get { return Retrieve(r=>r.EmailException); }
            set { Store(r=>r.EmailException,value); }
        }

        public string CodeRandom
        {
            get { return Retrieve(r=>r.CodeRandom); }
            set { Store(r=>r.CodeRandom,value); }
        }

        public bool StatusActive
        {
            get { return Retrieve(r=>r.StatusActive); }
            set { Store(r=>r.StatusActive,value); }
        }
    }
}