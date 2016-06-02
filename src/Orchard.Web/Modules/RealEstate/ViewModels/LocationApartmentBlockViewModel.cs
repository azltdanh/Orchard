using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class LocationApartmentBlockViewModel
    {
    }
    public class LocationApartmentBlockCreateViewModel
    {
        public string ReturnUrl { get; set; }

        [Required(ErrorMessage="Vui lòng chọn dự án")]
        public int ApartmentId { get; set; }
        public IEnumerable<LocationApartmentPart> LocationApartments { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên block dự án")]
        public string BlockName { get; set; }

        public string ShortName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tổng số tầng của block")]
        public int FloorTotal { get; set; }

        //[Required(ErrorMessage = "Vui lòng nhập tổng số nhóm tầng của block")]
        //public int FloorGroupTotal { get; set; }

        //[Required]
        public int ApartmentEachFloor { get; set; }

        [Required(ErrorMessage = "Vui lòng không để trống giá trung bình")]
        public double PriceAverage { get; set; }

        //[Required]
        public int SeqOrder { get; set; }

        public IContent LocationApartmentBlock { get; set; }
    }
    public class LocationApartmentBlockEditViewModel
    {
        public string ReturnUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn dự án")]
        public int ApartmentId { get; set; }
        public IEnumerable<LocationApartmentPart> LocationApartments { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên block dự án")]
        public string BlockName
        {
            get { return LocationApartmentBlock.As<LocationApartmentBlockPart>().BlockName; }
            set { LocationApartmentBlock.As<LocationApartmentBlockPart>().BlockName = value; }
        }
        
        //[Required]
        //public string ShortName
        //{
        //    get { return LocationApartmentBlock.As<LocationApartmentBlockPart>().ShortName; }
        //    set { LocationApartmentBlock.As<LocationApartmentBlockPart>().ShortName = value; }
        //}

        [Required(ErrorMessage = "Vui lòng nhập tổng số tầng của block")]
        public int FloorTotal
        {
            get { return LocationApartmentBlock.As<LocationApartmentBlockPart>().FloorTotal; }
            set { LocationApartmentBlock.As<LocationApartmentBlockPart>().FloorTotal = value; }
        }

        //[Required(ErrorMessage = "Vui lòng nhập tổng số nhóm tầng của block")]
        //public int FloorGroupTotal
        //{
        //    get { return LocationApartmentBlock.As<LocationApartmentBlockPart>().GroupFloorInBlockTotal; }
        //    set { LocationApartmentBlock.As<LocationApartmentBlockPart>().GroupFloorInBlockTotal = value; }
        //}

        //[Required]
        public int ApartmentEachFloor
        {
            get { return LocationApartmentBlock.As<LocationApartmentBlockPart>().ApartmentEachFloor; }
            set { LocationApartmentBlock.As<LocationApartmentBlockPart>().ApartmentEachFloor = value; }
        }

        //[Required]
        //public int SeqOrder
        //{
        //    get { return LocationApartmentBlock.As<LocationApartmentBlockPart>().SeqOrder; }
        //    set { LocationApartmentBlock.As<LocationApartmentBlockPart>().SeqOrder = value; }
        //}

        [Required(ErrorMessage = "Vui lòng không để trống giá trung bình")]
        public double PriceAverage
        {
            get { return LocationApartmentBlock.As<LocationApartmentBlockPart>().PriceAverage; }
            set { LocationApartmentBlock.As<LocationApartmentBlockPart>().PriceAverage = value; }
        }

        public bool IsActive
        {
            get { return LocationApartmentBlock.As<LocationApartmentBlockPart>().IsActive; }
            set { LocationApartmentBlock.As<LocationApartmentBlockPart>().IsActive = value; }
        }
        public IContent LocationApartmentBlock { get; set; }

    }
    public class LocationApartmentBlockIndexViewModel
    {
        public IList<LocationApartmentBlockEntry> LocationApartmentBlocks { get; set; }
        public LocationApartmentBlockIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }
    public class LocationApartmentBlockEntry
    {
        public LocationApartmentBlockPartRecord LocationApartmentBlock { get; set; }
        public bool IsChecked { get; set; }
    }
    public class LocationApartmentBlockIndexOptions
    {
        public string Search { get; set; }
        public int? ApartmentId { get; set; }
        public IEnumerable<LocationApartmentPart> LocationApartments { get; set; }

        public LocationApartmentsOrder Order { get; set; }
        public LocationApartmentsFilter Filter { get; set; }
        public LocationApartmentsBulkAction BulkAction { get; set; }
    }

    public class ApartmentBlockInfoIndex
    {
        public string ApartmentName { get; set; }
        public LocationApartmentBlockPart ApartmentBlockPart { get; set; }
        public IList<ApartmentBlockInfoEntry> ApartmentBlockInfoParts { get; set; }
        public ApartmentBlockInfoOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }
    public class ApartmentBlockInfoEntry
    {
        public ApartmentBlockInfoPart ApartmentBlockInfoPart { get; set; }
        public bool IsChecked { get; set; }
    }
    public class ApartmentBlockInfoOptions
    {
        public LocationApartmentsBulkAction BulkAction { get; set; }
    }
    public enum ApartmentBlockInfoBulkAction
    {
        None,
        Delete,
    }

    public class ApartmentBlockInfoCreateViewmodel
    {
        [Required(ErrorMessage="Vui lòng không để trống tên từng căn hộ")]
        public string ApartmentName { get; set; }

        [Required(ErrorMessage = "Vui lòng không để trống diện tích")]
        public double ApartmentArea { get; set; }

        [Required(ErrorMessage = "Vui lòng không để trống số phòng ngủ")]
        public int ApartmentBedrooms { get; set; }

        [Required(ErrorMessage = "Vui lòng không để trống số WC")]
        public int ApartmentBathrooms { get; set; }

        //[Required(ErrorMessage = "Vui lòng không để trống số WC")]
        public int RealAreaUse { get; set; }

        //[Required(ErrorMessage = "Vui lòng không để trống ảnh avatar")]
        //public string Avatar { get; set; }

        public string OrtherContent { get; set; }
        public int ApartmentBlockId { get; set; }
    }

    public class ApartmentBlockInfoEditViewmodel
    {
        [Required(ErrorMessage = "Vui lòng không để trống tên từng căn hộ")]
        public string ApartmentName
        {
            get { return ApartmentBlockInfo.As<ApartmentBlockInfoPart>().ApartmentName; }
            set { ApartmentBlockInfo.As<ApartmentBlockInfoPart>().ApartmentName = value; }
        }

        [Required(ErrorMessage = "Vui lòng không để trống diện tích")]
        public double ApartmentArea 
        {
            get { return ApartmentBlockInfo.As<ApartmentBlockInfoPart>().ApartmentArea; }
            set { ApartmentBlockInfo.As<ApartmentBlockInfoPart>().ApartmentArea = value; }
        }

        [Required(ErrorMessage = "Vui lòng không để trống số WC")]
        public int ApartmentBedrooms 
        {
            get { return ApartmentBlockInfo.As<ApartmentBlockInfoPart>().ApartmentBedrooms; }
            set { ApartmentBlockInfo.As<ApartmentBlockInfoPart>().ApartmentBedrooms = value; }
        }

        [Required(ErrorMessage = "Vui lòng không để trống số phòng tắm")]
        public int ApartmentBathrooms 
        {
            get { return ApartmentBlockInfo.As<ApartmentBlockInfoPart>().ApartmentBathrooms; }
            set { ApartmentBlockInfo.As<ApartmentBlockInfoPart>().ApartmentBathrooms = value; }
        }

        public double RealAreaUse
        {
            get { return ApartmentBlockInfo.As<ApartmentBlockInfoPart>().RealAreaUse; }
            set { ApartmentBlockInfo.As<ApartmentBlockInfoPart>().RealAreaUse = value; }
        }
        //[Required(ErrorMessage = "Vui lòng không để trống ảnh avatar")]
        public string Avatar
        {
            get { return ApartmentBlockInfo.As<ApartmentBlockInfoPart>().Avatar; }
            set { ApartmentBlockInfo.As<ApartmentBlockInfoPart>().Avatar = value; }
        }

        public string OrtherContent 
        {
            get { return ApartmentBlockInfo.As<ApartmentBlockInfoPart>().OrtherContent; }
            set { ApartmentBlockInfo.As<ApartmentBlockInfoPart>().OrtherContent = value; }
        }
        public int ApartmentBlockId { get; set; }

        public bool EnableEditImages { get; set; }
        public IEnumerable<PropertyFilePart> Files { get; set; }

        public IContent ApartmentBlockInfo { get; set; }
    }
}
