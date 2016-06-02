using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Users.Models;

namespace RealEstate.Models
{
    public class UserGroupPartRecord : ContentPartRecord
    {
        public virtual string Name { get; set; }
        public virtual string ShortName { get; set; }
        public virtual int SeqOrder { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual double Point { get; set; }
        public virtual UserPartRecord GroupAdminUser { get; set; }
        public virtual string ContactPhone { get; set; }

        //public virtual bool NoRestrictIP { get; set; }
        public virtual string AllowedAdminSingleIPs { get; set; }
        public virtual string AllowedAdminMaskedIPs { get; set; }
        public virtual string DeniedAdminSingleIPs { get; set; }
        public virtual string DeniedAdminMaskedIPs { get; set; }

        // GroupUsers
        public virtual IList<UserInGroupRecord> GroupUsers { get; set; }

        #region Settings

        public virtual LocationProvincePartRecord DefaultProvince { get; set; }
        public virtual LocationDistrictPartRecord DefaultDistrict { get; set; }
        public virtual PropertyStatusPartRecord DefaultPropertyStatus { get; set; }
        public virtual AdsTypePartRecord DefaultAdsType { get; set; }
        public virtual PropertyTypeGroupPartRecord DefaultTypeGroup { get; set; }

        public virtual int NumberOfAdsGoodDeal { get; set; }
        //public virtual int NumberOfAdsVIP { get; set; }
        public virtual int NumberOfAdsHighlight { get; set; }

        public virtual bool ApproveAllGroup { get; set; }

        #endregion
    }

    public class UserGroupPart : ContentPart<UserGroupPartRecord>
    {
        #region General

        public string Name
        {
            get { return Retrieve(r => r.Name); }
            set { Store(r => r.Name, value); }
        }

        public string ShortName
        {
            get { return Retrieve(r => r.ShortName); }
            set { Store(r => r.ShortName, value); }
        }

        public int SeqOrder
        {
            get { return Retrieve(r => r.SeqOrder); }
            set { Store(r => r.SeqOrder, value); }
        }

        public bool IsEnabled
        {
            get { return Retrieve(r => r.IsEnabled); }
            set { Store(r => r.IsEnabled, value); }
        }

        public double Point
        {
            get { return Retrieve(r => r.Point); }
            set { Store(r => r.Point, value); }
        }

        public UserPartRecord GroupAdminUser
        {
            get { return Record.GroupAdminUser; }
            set { Record.GroupAdminUser = value; }
        }

        public string ContactPhone
        {
            get { return Retrieve(r => r.ContactPhone); }
            set { Store(r => r.ContactPhone, value); }
        }

        #endregion

        #region IPs

        //public bool NoRestrictIP
        //{
        //    get { return NoRestrictIP; }
        //    set { NoRestrictIP = value; }
        //}
        public string AllowedAdminSingleIPs
        {
            get { return Retrieve(r => r.AllowedAdminSingleIPs); }
            set { Store(r => r.AllowedAdminSingleIPs, value); }
        }

        public string AllowedAdminMaskedIPs
        {
            get { return Retrieve(r => r.AllowedAdminMaskedIPs); }
            set { Store(r => r.AllowedAdminMaskedIPs, value); }
        }

        public string DeniedAdminSingleIPs
        {
            get { return Retrieve(r => r.DeniedAdminSingleIPs); }
            set { Store(r => r.DeniedAdminSingleIPs, value); }
        }

        public string DeniedAdminMaskedIPs
        {
            get { return Retrieve(r => r.DeniedAdminMaskedIPs); }
            set { Store(r => r.DeniedAdminMaskedIPs, value); }
        }

        #endregion

        #region Settings

        public LocationProvincePartRecord DefaultProvince
        {
            get { return Record.DefaultProvince; }
            set { Record.DefaultProvince = value; }
        }

        public LocationDistrictPartRecord DefaultDistrict
        {
            get { return Record.DefaultDistrict; }
            set { Record.DefaultDistrict = value; }
        }

        public PropertyStatusPartRecord DefaultPropertyStatus
        {
            get { return Record.DefaultPropertyStatus; }
            set { Record.DefaultPropertyStatus = value; }
        }

        public AdsTypePartRecord DefaultAdsType
        {
            get { return Record.DefaultAdsType; }
            set { Record.DefaultAdsType = value; }
        }

        public PropertyTypeGroupPartRecord DefaultTypeGroup
        {
            get { return Record.DefaultTypeGroup; }
            set { Record.DefaultTypeGroup = value; }
        }

        public int NumberOfAdsGoodDeal
        {
            get { return Retrieve(r => r.NumberOfAdsGoodDeal); }
            set { Store(r => r.NumberOfAdsGoodDeal, value); }
        }

        //public int NumberOfAdsVIP
        //{
        //    get { return NumberOfAdsVIP; }
        //    set { NumberOfAdsVIP = value; }
        //}

        public int NumberOfAdsHighlight
        {
            get { return Retrieve(r => r.NumberOfAdsHighlight); }
            set { Store(r => r.NumberOfAdsHighlight, value); }
        }

        public bool ApproveAllGroup
        {
            get { return Retrieve(r => r.ApproveAllGroup); }
            set { Store(r => r.ApproveAllGroup, value); }
        }

        #endregion

        // GroupUsers
        public IEnumerable<UserPartRecord> GroupUsers
        {
            get { return Record.GroupUsers.Select(r => r.UserPartRecord); }
        }
    }
}