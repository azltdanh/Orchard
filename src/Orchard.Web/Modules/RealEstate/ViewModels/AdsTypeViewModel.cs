using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class AdsTypeViewModel
    {
    }

    public class AdsTypeCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public IContent AdsType { get; set; }
    }

    public class AdsTypeEditViewModel
    {
        [Required]
        public string Name
        {
            get { return AdsType.As<AdsTypePart>().Name; }
            set { AdsType.As<AdsTypePart>().Name = value; }
        }

        public string ShortName
        {
            get { return AdsType.As<AdsTypePart>().ShortName; }
            set { AdsType.As<AdsTypePart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return AdsType.As<AdsTypePart>().CssClass; }
            set { AdsType.As<AdsTypePart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return AdsType.As<AdsTypePart>().SeqOrder; }
            set { AdsType.As<AdsTypePart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return AdsType.As<AdsTypePart>().IsEnabled; }
            set { AdsType.As<AdsTypePart>().IsEnabled = value; }
        }

        public IContent AdsType { get; set; }
    }

    public class AdsTypesIndexViewModel
    {
        public IList<AdsTypeEntry> AdsTypes { get; set; }
        public AdsTypeIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class AdsTypeEntry
    {
        public AdsTypePartRecord AdsType { get; set; }
        public bool IsChecked { get; set; }
    }

    public class AdsTypeIndexOptions
    {
        public string Search { get; set; }
        public AdsTypesOrder Order { get; set; }
        public AdsTypesFilter Filter { get; set; }
        public AdsTypesBulkAction BulkAction { get; set; }
    }

    public enum AdsTypesOrder
    {
        SeqOrder,
        Name
    }

    public enum AdsTypesFilter
    {
        All
    }

    public enum AdsTypesBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}