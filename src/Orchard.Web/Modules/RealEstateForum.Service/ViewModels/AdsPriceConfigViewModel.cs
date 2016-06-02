using Orchard.ContentManagement;
using  RealEstateForum.Service.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateForum.Service.ViewModels
{
    public class AdsPriceConfigViewModel
    {
        public List<AdsPriceConfigEntry> AdsPriceConfigs { get; set; }
        public dynamic Pager { get; set; }
    }
    public class AdsPriceConfigEntry
    {
        public AdsPriceConfigPart AdsPriceConfig { get; set; }
    }
    public class AdsPriceConfigCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập giá.")]
        public string Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Css class Vị trí.")]
        public string CssClass { get; set; }
        public string ReturnUrl { get; set; }

        public IContent AdsPriceConfig { get; set; }
    }

    public class AdsPriceConfigEditViewModel
    {
        [Required]
        public string Price
        {
            get { return AdsPriceConfig.As<AdsPriceConfigPart>().Price; }
            set { AdsPriceConfig.As<AdsPriceConfigPart>().Price = value; }
        }
        [Required]
        public string CssClass
        {
            get { return AdsPriceConfig.As<AdsPriceConfigPart>().CssClass; }
            set { AdsPriceConfig.As<AdsPriceConfigPart>().CssClass = value; }
        }

        public string ReturnUrl { get; set; }
        public IContent AdsPriceConfig { get; set; }
    }
}