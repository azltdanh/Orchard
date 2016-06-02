using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class AdsPaymentIndexViewModel
    {
        public AdsPaymentOptions Options { get; set; }
        public List<AdsPaymentEntry> AdsPaymentEntry { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
        public string AmountTotalVND { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class AdsPaymentEntry
    {
        public AdsPaymentHistoryPartRecord AdsPaymentHistory { get; set; }
        public PropertyPart PropertyPartEntry { get; set; }
        public bool IsChecked { get; set; }
    }

    public class AdsPaymentOptions
    {
        public string TypeVipCssClass { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string StartDateTrading { get; set; }
        public string EndDateTrading { get; set; }
        public IEnumerable<AdsPaymentConfigPartRecord> ListAdsPaymentConfig { get; set; }
        public IEnumerable<UserPart> ListUsers { get; set; }
        public bool IsInternal { get; set; }
        public long TotalAmount { get; set; }
    }

    public class AdsPaymentCreate
    {
        public string PaymentCssClass { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn một user")]
        public int ToUserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền. VD: 500000")]
        public long Amount { get; set; }

        public IEnumerable<UserPart> Users { get; set; }
        public string Note { get; set; }
    }

    public class AdsPaymentConfigIndexViewModel
    {
        public List<PaymentConfigEntry> AdsPaymentConfigs { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
    }

    public class PaymentConfigEntry
    {
        public AdsPaymentConfigPartRecord AdsPaymentConfig { get; set; }
        public bool IsChecked { get; set; }
    }

    public class AdsPaymentHistoryFrontEndIndexViewModel
    {
        public AdsPaymentFrontEndOptions Options { get; set; }
        public List<AdsPaymentFrontEndEntry> AdsPaymentEntry { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class AdsPaymentFrontEndEntry
    {
        public PropertyPart PropertyPartEntry { get; set; }
        public AdsPaymentHistoryPartRecord AdsPaymentHistory { get; set; }
        public string AmountVND { get; set; }
        public string EndBlanceVND { get; set; }
    }

    public class AdsPaymentFrontEndOptions
    {
        public string TypeVipCssClass { get; set; }
        public string StartDateTrading { get; set; }
        public string EndDateTrading { get; set; }
        public IEnumerable<AdsPaymentConfigPartRecord> ListAdsPaymentConfig { get; set; }
        public string TotalAmount { get; set; }
    }

    public class PaymentConfigCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập CssClass")]
        public string CssClass { get; set; }

        public string Description { get; set; }
        public long Value { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class PaymentConfigEditViewModel
    {
        public int Id
        {
            get { return PaymentConfig.As<AdsPaymentConfigPart>().Id; }
        }

        [Required]
        public string Name
        {
            get { return PaymentConfig.As<AdsPaymentConfigPart>().Name; }
            set { PaymentConfig.As<AdsPaymentConfigPart>().Name = value; }
        }

        [Required]
        public string CssClass
        {
            get { return PaymentConfig.As<AdsPaymentConfigPart>().CssClass; }
            set { PaymentConfig.As<AdsPaymentConfigPart>().CssClass = value; }
        }

        [Required]
        public string Description
        {
            get { return PaymentConfig.As<AdsPaymentConfigPart>().Description; }
            set { PaymentConfig.As<AdsPaymentConfigPart>().Description = value; }
        }

        [Required]
        public long Value
        {
            get { return PaymentConfig.As<AdsPaymentConfigPart>().Value; }
            set { PaymentConfig.As<AdsPaymentConfigPart>().Value = value; }
        }

        public bool IsEnabled
        {
            get { return PaymentConfig.As<AdsPaymentConfigPart>().IsEnabled; }
            set { PaymentConfig.As<AdsPaymentConfigPart>().IsEnabled = value; }
        }

        public IContent PaymentConfig { get; set; }
    }

    public class PaymentCardCreate
    {
        [Required]
        public string Provider { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã thẻ")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số seri thẻ")]
        public string Serial { get; set; }

        [StringLength(255, ErrorMessage = "Nội dung ghi chú tối đa 255 ký tự.")]
        public string Note { get; set; }

        public int GetPromotion { get; set; }
    }

    public enum PayType
    {
        PayByCard,
        PayByPhone
    }
}