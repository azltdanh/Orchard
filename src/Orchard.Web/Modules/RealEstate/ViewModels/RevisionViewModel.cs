using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;
using Orchard.Users.Models;

namespace RealEstate.ViewModels
{
    public class RevisionViewModel
    {
    }

    public class RevisionCreateViewModel
    {
        public DateTime CreatedDate { get; set; }
        public UserPartRecord CreatedUser { get; set; }
        public string FieldName { get; set; }
        public string ValueBefore { get; set; }
        public string ValueAfter { get; set; }
        public string ContentType { get; set; }
        public int ContentTypeRecordId { get; set; }

        public IContent Revision { get; set; }
    }

    public class RevisionEditViewModel
    {
        public DateTime CreatedDate
        {
            get { return Revision.As<RevisionPart>().CreatedDate; }
            set { Revision.As<RevisionPart>().CreatedDate = value; }
        }
        public UserPartRecord CreatedUser
        {
            get { return Revision.As<RevisionPart>().CreatedUser; }
            set { Revision.As<RevisionPart>().CreatedUser = value; }
        }
        public string FieldName
        {
            get { return Revision.As<RevisionPart>().FieldName; }
            set { Revision.As<RevisionPart>().FieldName = value; }
        }

        public string ValueBefore
        {
            get { return Revision.As<RevisionPart>().ValueBefore; }
            set { Revision.As<RevisionPart>().ValueBefore = value; }
        }

        public string ValueAfter
        {
            get { return Revision.As<RevisionPart>().ValueAfter; }
            set { Revision.As<RevisionPart>().ValueAfter = value; }
        }

        public string ContentType
        {
            get { return Revision.As<RevisionPart>().ContentType; }
            set { Revision.As<RevisionPart>().ContentType = value; }
        }

        public int ContentTypeRecordId
        {
            get { return Revision.As<RevisionPart>().ContentTypeRecordId; }
            set { Revision.As<RevisionPart>().ContentTypeRecordId = value; }
        }

        public IContent Revision { get; set; }
    }

    public class RevisionIndexViewModel
    {
        public IList<RevisionEntry> Revisions { get; set; }
        public RevisionIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class RevisionEntry
    {
        public RevisionPartRecord Revision { get; set; }
        public bool IsChecked { get; set; }
    }

    public class RevisionIndexOptions
    {
        public string Search { get; set; }
        public RevisionOrder Order { get; set; }
        public RevisionFilter Filter { get; set; }
        public RevisionBulkAction BulkAction { get; set; }
    }

    public enum RevisionOrder
    {
        SeqOrder,
        Name
    }

    public enum RevisionFilter
    {
        All
    }

    public enum RevisionBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
