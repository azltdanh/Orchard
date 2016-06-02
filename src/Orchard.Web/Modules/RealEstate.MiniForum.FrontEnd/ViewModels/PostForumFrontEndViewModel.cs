using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Orchard.ContentManagement;
using RealEstateForum.Service.Models;
using RealEstateForum.Service.ViewModels;

namespace RealEstate.MiniForum.FrontEnd.ViewModels
{
    public class PostForumFrontEndViewModel
    {
    }
    public class PostForumCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn chuyên mục")]
        public int ThreadId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chuyên đề")]
        public int TopicId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên bài viết")]
        [StringLength(100, ErrorMessage = "Tiêu đề quá dài, yêu cầu số ký tự phải nhỏ hơn 100")]
        public string Title { get; set; }

        [StringLength(500000)]
        [Required(ErrorMessage = "Vui lòng nhập nội dung bài viết")]
        public string Content { get; set; }
        public string CssImage { get; set; }

        public bool IsShareBlog { get; set; }
        public bool IsShowSignature { get; set; }

        public List<ThreadInfo> ListThread { get; set; }
        public List<ThreadInfo> ListTopic { get; set; }
        public List<HashViewModel> ListPublishStatus { get; set; }
        public int PublishStatusId { get; set; }
        public List<HashViewModel> ListPostStatus { get; set; }
        public int StatusPostId { get; set; }
        public string HostName { get; set; }

        // Accept post facebook
        public bool HaveFacebookUserId { get; set; }
        public bool AcceptPostToFacebok { get; set; }

        public string ReturnUrl { get; set; }
    }
    public class PostForumEditViewModel
    {
        public int? ThreadId
        {
            get { return ForumPost.As<ForumPostPart>().Thread.ParentThreadId; }
        }
        [Required(ErrorMessage = "Vui lòng chọn chuyên đề")]
        public int TopicId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên bài viết")]
        [StringLength(100, ErrorMessage = "Tiêu đề quá dài, yêu cầu số ký tự phải nhỏ hơn 100")]
        public string Title
        {
            get { return ForumPost.As<ForumPostPart>().Title; }
            set { ForumPost.As<ForumPostPart>().Title = value; }
        }
        [StringLength(500000)]
        [Required(ErrorMessage = "Vui lòng nhập nội dung bài viết")]
        public string Content
        {
            get { return ForumPost.As<ForumPostPart>().Content; }
            set { ForumPost.As<ForumPostPart>().Content = value; }
        }
        public string CssImagePath
        {
            get { return ForumPost.As<ForumPostPart>().CssImage; }
        }
        public string CssImage { get; set; }
        
        public int PublishStatusId { get; set; }
        public int StatusPostId { get; set; }
        public bool IsShareBlog
        {
            get { return ForumPost.As<ForumPostPart>().IsShareBlog; }
            set { ForumPost.As<ForumPostPart>().IsShareBlog = value; }
        }
        public List<ThreadInfo> ListThread { get; set; }
        public List<ThreadInfo> ListTopic { get; set; }
        public List<HashViewModel> ListPublishStatus { get; set; }
        public List<HashViewModel> ListPostStatus { get; set; }

        public bool IsShowSignature { get; set; }
        public string ReturnUrl { get; set; }

        // Accept post facebook
        public bool HaveFacebookUserId { get; set; }
        public bool AcceptPostToFacebok { get; set; }

        public IContent ForumPost { get; set; }
    }
}