using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateForum.Service.Models
{
    public class ForumPostStatusPartRecord : ContentPartRecord
    {
        public virtual string Name { get; set; }
        public virtual string ShortName{ get; set; }
        public virtual string CssClass { get; set; }
        public virtual int SeqOrder { get; set; }
        public virtual bool IsEnabled { get; set; }
    }
    public class ForumPostStatusPart : ContentPart<ForumPostStatusPartRecord>
    {
        public string Name
        {
            get { return this.Retrieve(r=>r.Name); }
            set { this.Store(r=>r.Name,value); }
        }
        public string ShortName
        {
            get { return this.Retrieve(r=>r.ShortName); }
            set { this.Store(r=>r.ShortName,value); }
        }
        public string CssClass
        {
            get { return this.Retrieve(r=>r.CssClass); }
            set { this.Store(r=>r.CssClass,value); }
        }
        public int SeqOrder
        {
            get { return this.Retrieve(r=>r.SeqOrder); }
            set { this.Store(r=>r.SeqOrder,value); }
        }
        public bool IsEnabled
        {
            get { return this.Retrieve(r=>r.IsEnabled); }
            set { this.Store(r=>r.IsEnabled,value); }
        }
    }
}