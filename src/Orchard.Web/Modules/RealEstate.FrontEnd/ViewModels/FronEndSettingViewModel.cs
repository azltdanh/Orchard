using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.FrontEnd.Models;

namespace RealEstate.FrontEnd.ViewModels
{
    internal class FronEndSettingViewModel
    {
    }

    public class FrontEndSettingsIndexViewModel
    {
        public FrontEndSettingIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public IList<FrontEndSettingEntry> FrontEndSettings { get; set; }
    }

    public class FrontEndSettingIndexOptions
    {
        public FrontEndSettingsBulkAction BulkAction { get; set; }
        public FrontEndSettingsFilter Filter { get; set; }
        public FrontEndSettingsOrder Order { get; set; }
        public string Search { get; set; }
    }

    public class FrontEndSettingCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Value { get; set; }

        public IContent FrontEndSetting { get; set; }
    }

    public class FrontEndSettingEditViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Value { get; set; }

        public IContent FrontEndSetting { get; set; }
    }

    public class FrontEndSettingEntry
    {
        public bool IsChecked { get; set; }
        public FrontEndSettingPartRecord FrontEndSetting { get; set; }
    }

    public enum FrontEndSettingsBulkAction
    {
        None = 0,
        Enable = 1,
        Disable = 2,
        Delete = 3,
    }

    public enum FrontEndSettingsFilter
    {
        All = 0,
    }

    public enum FrontEndSettingsOrder
    {
        SeqOrder = 0,
        Name = 1,
    }
}