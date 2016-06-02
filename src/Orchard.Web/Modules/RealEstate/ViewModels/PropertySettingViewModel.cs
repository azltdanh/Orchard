using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class PropertySettingViewModel
    {
    }

    public class PropertySettingCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Value { get; set; }

        public IContent PropertySetting { get; set; }
    }

    public class PropertySettingEditViewModel
    {
        [Required]
        public string Name
        {
            get { return PropertySetting.As<PropertySettingPart>().Name; }
            set { PropertySetting.As<PropertySettingPart>().Name = value; }
        }

        [Required]
        public string Value
        {
            get { return PropertySetting.As<PropertySettingPart>().Value; }
            set { PropertySetting.As<PropertySettingPart>().Value = value; }
        }

        public IContent PropertySetting { get; set; }
    }

    public class PropertySettingsIndexViewModel
    {
        public IList<PropertySettingEntry> PropertySettings { get; set; }
        public PropertySettingIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PropertySettingEntry
    {
        public PropertySettingPartRecord PropertySetting { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PropertySettingIndexOptions
    {
        public string Search { get; set; }
        public PropertySettingsOrder Order { get; set; }
        public PropertySettingsFilter Filter { get; set; }
        public PropertySettingsBulkAction BulkAction { get; set; }
    }

    public enum PropertySettingsOrder
    {
        SeqOrder,
        Name
    }

    public enum PropertySettingsFilter
    {
        All
    }

    public enum PropertySettingsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
