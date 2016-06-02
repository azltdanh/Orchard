using System;
using System.Collections.Generic;
using RealEstateForum.Service.Models;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace RealEstateForum.Service.ViewModels
{
    public class ThreadAdminIndexViewModel
    {
        public ThreadOptions Options { get; set; }
        public List<ThreadEntry> ListThreadEntry { get; set; }
        public ThreadInfo ThreadInfo { get; set; }
        public dynamic Pager { get; set; }
    }
    public class ThreadEntry
    {
        public ForumThreadPart ThreadPart { get; set; }
        public int CountChild { get; set; }
        public bool IsChecked { get; set; }
    }
    public class ThreadOptions
    {
        public string ReturnUrl { get; set; }
        public ThreadOrder Order { get; set; }
        public ThreadBulkAction BulkAction { get; set; }
    }

    public class ThreadAdminCreateViewModel
    {
        [Required(ErrorMessage="Vui lòng nhập tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên ShortName (VD: ten-chuyen-muc)")]
        public string ShortName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả chuyên mục")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thứ tự sắp xếp")]
        public int SeqOrder { get; set; }

        public string DefaultImage { get; set; }
        public bool IsOpen { get; set; }

        public int ThreadId { get; set; }

        public string ReturnUrl { get; set; }
    }
    public class ThreadAdminEditViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name
        {
            get { return ForumThread.As<ForumThreadPart>().Name; }
            set { ForumThread.As<ForumThreadPart>().Name = value; }
        }
        [Required(ErrorMessage = "Vui lòng nhập tên ShortName (VD: ten-chuyen-muc)")]
        public string ShortName
        {
            get { return ForumThread.As<ForumThreadPart>().ShortName; }
            set { ForumThread.As<ForumThreadPart>().ShortName = value; }
        }
        [Required(ErrorMessage = "Vui lòng nhập mô tả chi tiết")]
        public string Description
        {
            get { return ForumThread.As<ForumThreadPart>().Description; }
            set { ForumThread.As<ForumThreadPart>().Description = value; }
        }
        [Required(ErrorMessage = "Vui lòng nhập thứ tự sắp xếp")]
        public int SeqOrder
        {
            get { return ForumThread.As<ForumThreadPart>().SeqOrder; }
            set { ForumThread.As<ForumThreadPart>().SeqOrder = value; }
        }
        public string DefaultImage { get; set; }
        
        public string DefaultImagePath
        {
            get { return ForumThread.As<ForumThreadPart>().DefaultImage; }
        }
        public bool IsOpen
        {
            get { return ForumThread.As<ForumThreadPart>().IsOpen; }
            set { ForumThread.As<ForumThreadPart>().IsOpen = value; }
        }
        public int? ThreadId 
        {
            get { return ForumThread.As<ForumThreadPart>().ParentThreadId; }
            set { ForumThread.As<ForumThreadPart>().ParentThreadId = value; }
        }
        public List<ThreadInfo> ListThread { get; set; }
        public IContent ForumThread { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class ThreadInfo
    {
        public int Id { get; set; }
        public int? ThreadParentId { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string DefaultImage { get; set; }
    }


    public enum ThreadOrder
    {
        Id,
        Name,
        ShortName
    }
    public enum ThreadFilter
    {
        Id,
        Name,
    }
    public enum ThreadBulkAction
    {
        None,
        Delete,
        Enable,
        Disable
    }
}