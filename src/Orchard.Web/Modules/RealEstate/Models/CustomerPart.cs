using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Users.Models;
using RealEstate.Helpers;

namespace RealEstate.Models
{
    // ReSharper disable InconsistentNaming
    public class CustomerPartRecord : ContentPartRecord
    {
        public virtual int? CustomerId { get; set; }
        public virtual string IdStr { get; set; }

        // Contact
        public virtual string ContactName { get; set; }
        public virtual string ContactPhone { get; set; }
        public virtual string ContactAddress { get; set; }
        public virtual string ContactEmail { get; set; }

        // Status
        public virtual CustomerStatusPartRecord Status { get; set; }
        public virtual DateTime? StatusChangedDate { get; set; }
        public virtual string Note { get; set; }

        // Purposes
        //public virtual IList<CustomerPurposePartRecordContent> Purposes { get; set; }

        // Requirements
        public virtual IList<CustomerRequirementRecord> Requirements { get; set; }

        // Properties
        public virtual IList<CustomerPropertyRecord> Properties { get; set; }

        // Group
        public virtual UserGroupPartRecord UserGroup { get; set; }

        // User
        public virtual DateTime CreatedDate { get; set; }
        public virtual UserPartRecord CreatedUser { get; set; }
        public virtual DateTime LastUpdatedDate { get; set; }
        public virtual UserPartRecord LastUpdatedUser { get; set; }
        public virtual DateTime? LastCallDate { get; set; }
        public virtual UserPartRecord LastCallUser { get; set; }

        #region Ads

        public virtual bool Published { get; set; }
        public virtual DateTime? AdsExpirationDate { get; set; }

        public virtual bool AdsVIP { get; set; }
        public virtual DateTime? AdsVIPExpirationDate { get; set; }

        #endregion
    }

    public class CustomerPart : ContentPart<CustomerPartRecord>
    {
        public int? CustomerId
        {
            get { return Retrieve(r => r.CustomerId); }
            set { Store(r => r.CustomerId, value); }
        }

        public string IdStr
        {
            get { return Retrieve(r => r.IdStr); }
            set { Store(r => r.IdStr, value); }
        }

        // Flag & Status

        public CustomerStatusPartRecord Status
        {
            get { return Record.Status; }
            set { Record.Status = value; }
        }

        public DateTime? StatusChangedDate
        {
            get { return Retrieve(r => r.StatusChangedDate); }
            set { Store(r => r.StatusChangedDate, value); }
        }

        public string Note
        {
            get { return Retrieve(r => r.Note); }
            set { Store(r => r.Note, value); }
        }

        // Purposes

        public IEnumerable<CustomerPurposePartRecord> Purposes {
            //get { return Record.Purposes.Select(r => r.CustomerPurposePartRecord); }
            get; set; }

        // Requirements

        public IEnumerable<CustomerRequirementRecord> Requirements
        {
            get { return Record.Requirements; }
        }

        // Properties

        public IEnumerable<PropertyPartRecord> Properties
        {
            get { return Record.Properties.Select(r => r.PropertyPartRecord); }
        }

        public string DisplayForUrl
        {
            get
            {
                string url = "";

                if (Requirements.Any(r => r.AdsTypePartRecord != null))
                    url += Requirements.First().AdsTypePartRecord.Name.ToSlug();

                if (Requirements.Any(r => r.PropertyTypeGroupPartRecord != null))
                    url += "-" + Requirements.First().PropertyTypeGroupPartRecord.Name.ToSlug();

                if (Requirements.Any(r => r.LocationProvincePartRecord != null))
                    url += "-tai-" + Requirements.First().LocationProvincePartRecord.Name.ToSlug();

                if (Requirements.Min(r => r.MinPrice).HasValue)
                    url += "-gia-tu-" + Requirements.Min(r => r.MinPrice);
                if (Requirements.Max(r => r.MaxPrice).HasValue)
                    url += "-den-" + Requirements.Max(r => r.MaxPrice);

                if (Requirements.Min(r => r.MinPrice).HasValue || Requirements.Max(r => r.MaxPrice).HasValue)
                    url += "-" + Requirements.First().PaymentMethodPartRecord.Name.ToSlug();

                return url;
            }
        }

        #region User

        // Group

        public UserGroupPartRecord UserGroup
        {
            get { return Record.UserGroup; }
            set { Record.UserGroup = value; }
        }

        // CreatedUser

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

        public DateTime LastUpdatedDate
        {
            get { return Retrieve(r => r.LastUpdatedDate); }
            set { Store(r => r.LastUpdatedDate, value); }
        }

        public UserPartRecord LastUpdatedUser
        {
            get { return Record.LastUpdatedUser; }
            set { Record.LastUpdatedUser = value; }
        }

        public DateTime? LastCallDate
        {
            get { return Retrieve(r => r.LastCallDate); }
            set { Store(r => r.LastCallDate, value); }
        }

        public UserPartRecord LastCallUser
        {
            get { return Record.LastCallUser; }
            set { Record.LastCallUser = value; }
        }

        #endregion

        #region Ads

        public bool Published
        {
            get { return Retrieve(r => r.Published); }
            set { Store(r => r.Published, value); }
        }

        public DateTime? AdsExpirationDate
        {
            get { return Retrieve(r => r.AdsExpirationDate); }
            set { Store(r => r.AdsExpirationDate, value); }
        }

        public bool AdsVIP
        {
            get { return Retrieve(r => r.AdsVIP); }
            set { Store(r => r.AdsVIP, value); }
        }

        public DateTime? AdsVIPExpirationDate
        {
            get { return Retrieve(r => r.AdsVIPExpirationDate); }
            set { Store(r => r.AdsVIPExpirationDate, value); }
        }

        #endregion

        #region Contact

        public string ContactName
        {
            get { return Retrieve(r => r.ContactName); }
            set { Store(r => r.ContactName, value); }
        }

        public string ContactPhone
        {
            get { return Retrieve(r => r.ContactPhone); }
            set { Store(r => r.ContactPhone, value); }
        }

        public string ContactAddress
        {
            get { return Retrieve(r => r.ContactAddress); }
            set { Store(r => r.ContactAddress, value); }
        }

        public string ContactEmail
        {
            get { return Retrieve(r => r.ContactEmail); }
            set { Store(r => r.ContactEmail, value); }
        }

        #endregion
    }
    // ReSharper restore InconsistentNaming
}