using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service.ViewModels
{
    public class PostIndexAdminViewModel
    {
        public List<ForumPostEntry> ForumPostEntry { get; set; }
        public PostIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
    }
    public class ForumPostEntry
    {
        public ForumPostPart ForumPostItem { get; set; }
        public bool IsChecked { get; set; }
    }
 
    public class PostIndexOptions
    {
        public string ReturnUrl { get; set; }
        public List<ThreadInfo> ListThread { get; set; }
        public int? ThreadIdIndex { get; set; }
        public List<ThreadInfo> ListTopic { get; set; }
        public int[] TopicIds { get; set; }
        public List<HashViewModel> ListPublishStatus { get; set; }
        public int PublishStatusId { get; set; }
        public List<HashViewModel> ListPostStatus { get; set; }
        public int PostStatusId { get; set; }
        public List<HashViewModel> ListUserPost { get; set; }
        public string UserNameOrEmail { get; set; }
        public string SearchText { get; set; }
        public string Title { get; set; }
        public int? PostId { get; set; }
        public string DateCreateFrom { get; set; }
        public string DateCreateTo { get; set; }
        public bool IsProject { get; set; }
        public bool IsMarket { get; set; }
        public bool IsHeighLight { get; set; }
        public bool IsPinned { get; set; }
        public PostOrder Order { get; set; }
        public PostFilter Filter { get; set; }
        public PostBulkAction BulkAction { get; set; }
    }
    public class PostFilterOptions
    {
        public string ReturnUrl { get; set; }
        public List<ThreadInfo> ListThread { get; set; }
        public int? ThreadId { get; set; }
        public List<ThreadInfo> ListTopic { get; set; }
        public int TopicId { get; set; }
        public string UserNameOrEmail { get; set; }
        public string SearchText { get; set; }
        public string Title { get; set; }
        public string DateCreateFrom { get; set; }
        public string DateCreateTo { get; set; }

    }
    public class PostCreateAdminViewModel
    {
        public int ThreadId { get; set; }

        [Required(ErrorMessage="Vui lòng chọn chuyên đề")]
        public int TopicId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên bài viết")]
        [StringLength(100, ErrorMessage = "Tiêu đề quá dài, yêu cầu số ký tự phải nhỏ hơn 100")]
        public string Title { get; set; }

        [StringLength(500000)]
        //[Required(ErrorMessage = "Vui lòng nhập mô tả bài viết")]
        public string Description { get; set; }

        [StringLength(500000)]
        [Required(ErrorMessage = "Vui lòng nhập nội dung bài viết")]
        public string Content { get; set; }
        public string CssImage { get; set; }

        public bool IsPinned { get; set; }
        public DateTime? TimeExpiredPinned { get; set; }

        public bool IsProject { get; set; }
        public bool IsMarket { get; set; }
        public bool IsHeighLight { get; set; }
        public int PublishStatusId { get; set; }
        public int StatusPostId { get; set; }
        public bool IsShareBlog { get; set; }
        public List<ThreadInfo> ListThread { get; set; }
        public List<ThreadInfo> ListTopic { get; set; }
        public List<HashViewModel> ListPublishStatus { get; set; }
        public List<HashViewModel> ListPostStatus { get; set; }
        public string ReturnUrl { get; set; }

        // Accept post facebook
        public bool HaveFacebookUserId { get; set; }
        public bool AcceptPostToFacebok { get; set; }
    }
    public class PostEditAdminViewModel
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
        //[Required(ErrorMessage = "Vui lòng nhập mô tả bài viết")]
        public string Description
        {
            get { return ForumPost.As<ForumPostPart>().Description; }
            set { ForumPost.As<ForumPostPart>().Description = value; }
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
        
        public bool IsPinned
        {
            get { return ForumPost.As<ForumPostPart>().IsPinned; }
            set { ForumPost.As<ForumPostPart>().IsPinned = value; }
        }
        public DateTime? TimeExpiredPinned
        {
            get { return ForumPost.As<ForumPostPart>().TimeExpiredPinned; }
            set { ForumPost.As<ForumPostPart>().TimeExpiredPinned = value; }
        }

        public bool IsProject
        {
            get { return ForumPost.As<ForumPostPart>().IsProject; }
            set { ForumPost.As<ForumPostPart>().IsProject = value; }
        }
        public bool IsMarket
        {
            get { return ForumPost.As<ForumPostPart>().IsMarket; }
            set { ForumPost.As<ForumPostPart>().IsMarket = value; }
        }
        public bool IsHeighLight
        {
            get { return ForumPost.As<ForumPostPart>().IsHeighLight; }
            set { ForumPost.As<ForumPostPart>().IsHeighLight = value; }
        }
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
        public bool UpdateMeta { get; set; }

        public string ReturnUrl { get; set; }

        // Accept post facebook
        public bool HaveFacebookUserId { get; set; }
        public bool AcceptPostToFacebok { get; set; }

        public IContent ForumPost { get; set; }
    }
    public class HashViewModel
    {
        public int Id { get; set; }
        public object Value { get; set; }
    }

    public enum PostOrder
    {
        DateUpdated,
        Id,
        Name,
        DateCreated
    }
    public enum PostFilter
    {
        All,
        Id,
        Name,
    }
    public enum PostBulkAction
    {
        None,
        Delete,
        UnDelete,
        Approve,
        UpdateMetaKeyWord
    }

    public class PersonalPageViewModel
    {
        public List<PersonalPageEntry> ListPostHomePage { get; set; }
        public bool IsAdminOrManagement { get; set; }
        public UserInfo UserCurent { get; set; }
        public UserInfo UserSelect { get; set; }
        public int TotalCount { get; set; }
        public bool IsPageOwner { get; set; }
        public dynamic Pager { get; set; }
    }
    public class PersonalPageEntry
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string Title { get; set; }
        public string TopicShortName { get; set; }
        public string ThreadShortName { get; set; }
        public string Content { get; set; }
        public UserInfo UserInfo { get; set; }
        public string DefaultImage { get; set; }
        public DateTime? BlogDateCreated { get; set; }
        public string TimeAgo { get; set; }
        public bool IsShareBlog { get; set; }
        public string SignatureUser { get; set; }
        public CommentIndexViewModel PostComment { get; set; }
        public string DisplayForNameForUrl { get; set; }
        public PostItem PostItemInfo { get; set; }
    }
}