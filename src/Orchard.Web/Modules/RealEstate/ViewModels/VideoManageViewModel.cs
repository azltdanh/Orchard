
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class VideoManageIndexViewModel
    {
        public List<VideoManageEntry> VideoManages { get; set; }
        public VideoManagePart VideoManagePart { get; set; }
        public VideoOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class VideoOptions
    {
        public string Search { get; set; }
        public int? VideoTypeId { get; set; }
        public IEnumerable<VideoTypePart> VideoTypes { get; set; }
        public VideoManageBulkAction BulkAction { get; set; }
    }
    public class VideoManageEntry
    {
        public VideoManagePart VideoManagePart { get; set; }
        public bool IsChecked { get; set; }
    }

    public class VideoManageCreateViewModel
    {
        public string ReturnUrl { get; set; }
        public IEnumerable<VideoTypePart> VideoTypes { get; set; }
        public int VideoTypeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung tiêu đề.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung keyword")]
        public string Keyword { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập description")]
        public string Description { get; set; }
        //[Required(ErrorMessage = "Vui lòng chọn file video và upload trước khi lưu thông tin lại.")]
        public string YoutubeId { get; set; }
        public string ExistYoutubeId { get; set; }
        public bool Enable { get; set; }
        public bool Publish { get; set; }
        public int SeqOrder { get; set; }
        public string Image { get; set; }
    }

    public class VideoManageEditViewModel
    {
        public string ReturnUrl { get; set; }

        public IEnumerable<VideoTypePart> VideoTypes { get; set; }
        public int VideoTypeId { get; set; }
        public int Id
        {
            get { return VideoManage.As<VideoManagePart>().Id; }
        }

        [Required(ErrorMessage = "Vui lòng nhập nội dung tiêu đề.")]
        public string Title
        {
            get { return VideoManage.As<VideoManagePart>().Title; }
            set { VideoManage.As<VideoManagePart>().Title = value; }
        }
        [Required(ErrorMessage = "Vui lòng nhập nội dung keyword")]
        public string Keyword
        {
            get { return VideoManage.As<VideoManagePart>().Keyword; }
            set { VideoManage.As<VideoManagePart>().Keyword = value; }
        }
        [Required(ErrorMessage = "Vui lòng nhập description")]
        public string Description
        {
            get { return VideoManage.As<VideoManagePart>().Description; }
            set { VideoManage.As<VideoManagePart>().Description = value; }
        }
        public string YoutubeId
        {
            get { return VideoManage.As<VideoManagePart>().YoutubeId; }
            set { VideoManage.As<VideoManagePart>().YoutubeId = value; }
        }
        public string ExistYoutubeId {
            get { return VideoManage.As<VideoManagePart>().YoutubeId; }
            set { VideoManage.As<VideoManagePart>().YoutubeId = value; }
        }
        public bool Publish
        {
            get { return VideoManage.As<VideoManagePart>().Publish; }
            set { VideoManage.As<VideoManagePart>().Publish = value; }
        }
        public bool Enable
        {
            get { return VideoManage.As<VideoManagePart>().Enable; }
            set { VideoManage.As<VideoManagePart>().Enable = value; }
        }
        public int SeqOrder
        {
            get { return VideoManage.As<VideoManagePart>().SeqOrder; }
            set { VideoManage.As<VideoManagePart>().SeqOrder = value; }
        }
        public string Image
        {
            get { return VideoManage.As<VideoManagePart>().Image; }
            set { VideoManage.As<VideoManagePart>().Image = value; }
        }

        public IContent VideoManage { get; set; }
    }

    public enum VideoManageBulkAction
    {
        Delete,
        Enable,
        Disable,
        Publish,
        UnPublish
    }
}
