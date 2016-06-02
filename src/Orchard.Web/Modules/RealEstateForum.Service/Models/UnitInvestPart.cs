using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;

namespace RealEstateForum.Service.Models
{
    public class UnitInvestPartRecord : ContentPartRecord
    {
        public virtual string Name { get; set; }
        public virtual string Avatar { get; set; }
        public virtual string Website { get; set; }
        public virtual string Content { get; set; }
        public virtual int SeqOrder { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual int GroupId { get; set; }
    }
    public class UnitInvestPart : ContentPart<UnitInvestPartRecord>
    {
        public string Name
        {
            get { return Retrieve(r=>r.Name); }
            set { Store(r=>r.Name,value); }
        }

        public string Avatar
        {
            get { return Retrieve(r => r.Avatar); }
            set { Store(r => r.Avatar, value); }
        }

        public string Website
        {
            get { return Retrieve(r => r.Website); }
            set { Store(r => r.Website, value); }
        }

        public string Content
        {
            get { return Retrieve(r => r.Content); }
            set { Store(r => r.Content, value); }
        }

        public int SeqOrder
        {
            get { return this.Retrieve(r => r.SeqOrder); }
            set { this.Store(r => r.SeqOrder, value); }
        }

        public bool IsEnabled
        {
            get { return this.Retrieve(r => r.IsEnabled); }
            set { this.Store(r => r.IsEnabled, value); }
        }

        public int GroupId
        {
            get { return this.Retrieve(r => r.GroupId); }
            set { this.Store(r => r.GroupId, value); }
        }
    }
}