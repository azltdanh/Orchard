using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;

namespace RealEstateForum.Service.Models
{
    public class ForumThreadPartRecord : ContentPartRecord
    {
        public virtual string Name { get; set; }
        public virtual string ShortName { get; set; }
        public virtual string Description { get; set; }
        public virtual int SeqOrder { get; set; }
        public virtual int? ParentThreadId { get; set; }
        public virtual string DefaultImage { get; set; }
        public virtual bool IsOpen { get; set; }
        public virtual DateTime? DateCreated { get; set; }
        public virtual string HostName { get; set; }
    }
    public class ForumThreadPart : ContentPart<ForumThreadPartRecord>
    {
        public string Name
        {
            get { return Retrieve(p => p.Name);}
            set { Store(p => p.Name, value); }
        }
        public string ShortName
        {
            get { return Retrieve(p => p.ShortName); }
            set { Store(p => p.ShortName, value); }
        }
        public string Description
        {
            get { return Retrieve(p => p.Description); }
            set { Store(p => p.Description, value); }
        }
        public int SeqOrder
        {
            get { return Retrieve(p => p.SeqOrder); }
            set { Store(p => p.SeqOrder, value); }
        }
        public int? ParentThreadId
        {
            get { return Retrieve(p=>p.ParentThreadId); }
            set { Store(p=>p.ParentThreadId,value); }
        }
        public string DefaultImage
        {
            get { return Retrieve(p => p.DefaultImage); }
            set { Store(p => p.DefaultImage, value); }
        }
        public bool IsOpen
        {
            get { return Retrieve(p => p.IsOpen); }
            set { Store(p => p.IsOpen, value); }
        }
        public DateTime? DateCreated
        {
            get { return Retrieve(p => p.DateCreated); }
            set { Store(p => p.DateCreated, value); }
        }
        public string HostName
        {
            get { return Retrieve(p => p.HostName); }
            set { Store(p => p.HostName, value); }
        }
    }
}