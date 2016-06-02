using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Users.Models;

namespace RealEstate.Models
{
    public class RevisionPartRecord : ContentPartRecord
    {
        public virtual DateTime CreatedDate { get; set; }
        public virtual UserPartRecord CreatedUser { get; set; }
        public virtual string FieldName { get; set; }
        public virtual string ValueBefore { get; set; }
        public virtual string ValueAfter { get; set; }
        public virtual string ContentType { get; set; }
        public virtual int ContentTypeRecordId { get; set; }
    }

    public class RevisionPart : ContentPart<RevisionPartRecord>
    {
        public DateTime CreatedDate
        {
            get { return Retrieve(r => r.CreatedDate); }
            set { Store(r => r.CreatedDate, value); }
        }

        public UserPartRecord CreatedUser
        {
            get { return Record.CreatedUser; }
            set { Record.CreatedUser = value; }
        }

        public string FieldName
        {
            get { return Retrieve(r => r.FieldName); }
            set { Store(r => r.FieldName, value); }
        }

        public string ValueBefore
        {
            get { return Retrieve(r => r.ValueBefore); }
            set { Store(r => r.ValueBefore, value); }
        }

        public string ValueAfter
        {
            get { return Retrieve(r => r.ValueAfter); }
            set { Store(r => r.ValueAfter, value); }
        }

        public string ContentType
        {
            get { return Retrieve(r => r.ContentType); }
            set { Store(r => r.ContentType, value); }
        }

        public int ContentTypeRecordId
        {
            get { return Retrieve(r => r.ContentTypeRecordId); }
            set { Store(r => r.ContentTypeRecordId, value); }
        }
    }
}