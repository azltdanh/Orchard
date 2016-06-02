using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;

namespace RealEstateForum.Service.Models
{
    public class FilterRulesPartRecord : ContentPartRecord
    {
        public virtual string FromWord { get; set; }
        public virtual string ToWord { get; set; }
    }
    public class FilterRulesPart : ContentPart<FilterRulesPartRecord>
    {
        public string FromWord
        {
            get { return Retrieve(p=>p.FromWord); }
            set { Store(p=>p.FromWord,value); }
        }
        public string ToWord
        {
            get { return Retrieve(p => p.ToWord); }
            set { Store(p => p.ToWord, value); }
        }
    }
}