using System.Collections.Generic;
using Orchard.ContentManagement;
using RealEstate.FrontEnd.Models;
using RealEstate.Models;

namespace RealEstate.FrontEnd.ViewModels
{
    internal class AliasesMetaViewModel
    {
    }

    public class AliasesMetaCreatedOptions
    {
        public bool AdsGoodDeal { get; set; }
        public bool AdsVIP { get; set; }
        public bool IsAuction { get; set; }
        public bool IsOwner { get; set; }
        //public bool AllAnyType { get; set; }
        public string AdsTypeCssClass { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }
        public string TypeGroupCssClass { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }
        public int[] ProvinceIds { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }
        public bool IsCheckProvince { get; set; }
        public bool IsCheckUpdateMeta { get; set; }
        public bool IsCheckUpdateTitle { get; set; }
        public bool IsCheckUpdateMetaKeyword { get; set; }
        public bool IsCheckUpdateMetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string MetaKeyword { get; set; }
        public string MetaDescription { get; set; }
    }

    public class AliasesMetaIndexViewModel
    {
        public AliasesMetaIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public IList<AliasesMetaEntry> AliasesMetas { get; set; }
    }

    public class AliasesMetaIndexOptions
    {
        public AliasesMetaBulkAction BulkAction { get; set; }
        public AliasesMetaFilter Filter { get; set; }
        public AliasesMetaOrder Order { get; set; }
        public string Search { get; set; }
    }

    public class AliasesMetaCreateViewModel
    {
        public string Title { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public int SeqOrder { get; set; }
        public string DomainGroupCurrent { get; set; }
        public IContent AliasesMeta { get; set; }
    }

    public class AliasesMetaEditViewModel
    {
        public string Title { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public int SeqOrder { get; set; }
        public string DomainGroupCurrent { get; set; }
        public IContent AliasesMeta { get; set; }
    }

    public class AliasesMetaEntry
    {
        public bool IsChecked { get; set; }
        public AliasesMetaPartRecord AliasesMeta { get; set; }
    }

    public enum AliasesMetaBulkAction
    {
        None = 0,
        Enable = 1,
        Disable = 2,
        Delete = 3,
    }

    public enum AliasesMetaFilter
    {
        All = 0,
    }

    public enum AliasesMetaOrder
    {
        SeqOrder = 0,
        Name = 1,
    }
}