using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;
using Orchard.Users.Models;

namespace RealEstate.ViewModels
{
    class PropertyFileViewModel
    {
    }

    public class PropertyFileCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string Type { get; set; }

        public string Path { get; set; }

        public int Size { get; set; }

        public bool Published { get; set; }

        public bool IsAvatar { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public UserPartRecord CreatedUser { get; set; }

        public PropertyPartRecord Property { get; set; }

        public LocationApartmentPartRecord Apartment { get; set; }

        public IContent PropertyFile { get; set; }
    }

    public class PropertyFileEditViewModel
    {
        [Required]
        public string Name
        {
            get { return PropertyFile.As<PropertyFilePart>().Name; }
            set { PropertyFile.As<PropertyFilePart>().Name = value; }
        }

        public string Type
        {
            get { return PropertyFile.As<PropertyFilePart>().Type; }
            set { PropertyFile.As<PropertyFilePart>().Type = value; }
        }

        public string Path
        {
            get { return PropertyFile.As<PropertyFilePart>().Path; }
            set { PropertyFile.As<PropertyFilePart>().Path = value; }
        }

        public int Size
        {
            get { return PropertyFile.As<PropertyFilePart>().Size; }
            set { PropertyFile.As<PropertyFilePart>().Size = value; }
        }

        public bool Published
        {
            get { return PropertyFile.As<PropertyFilePart>().Published; }
            set { PropertyFile.As<PropertyFilePart>().Published = value; }
        }

        public bool IsAvatar
        {
            get { return PropertyFile.As<PropertyFilePart>().IsAvatar; }
            set { PropertyFile.As<PropertyFilePart>().IsAvatar = value; }
        }

        public bool IsDeleted
        {
            get { return PropertyFile.As<PropertyFilePart>().IsDeleted; }
            set { PropertyFile.As<PropertyFilePart>().IsDeleted = value; }
        }

        public DateTime CreatedDate
        {
            get { return PropertyFile.As<PropertyFilePart>().CreatedDate; }
            set { PropertyFile.As<PropertyFilePart>().CreatedDate = value; }
        }

        public UserPartRecord CreatedUser
        {
            get { return PropertyFile.As<PropertyFilePart>().CreatedUser; }
            set { PropertyFile.As<PropertyFilePart>().CreatedUser = value; }
        }

        public PropertyPartRecord Property
        {
            get { return PropertyFile.As<PropertyFilePart>().Property; }
            set { PropertyFile.As<PropertyFilePart>().Property = value; }
        }

        public LocationApartmentPartRecord Apartment
        {
            get { return PropertyFile.As<PropertyFilePart>().Apartment; }
            set { PropertyFile.As<PropertyFilePart>().Apartment = value; }
        }

        public IContent PropertyFile { get; set; }
    }

    public class PropertyFileIndexViewModel
    {
        public IList<PropertyFileEntry> PropertyFiles { get; set; }
        public PropertyFileIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PropertyFileEntry
    {
        public PropertyFilePartRecord PropertyFile { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PropertyFileIndexOptions
    {
        public string Search { get; set; }
        public PropertyFileOrder Order { get; set; }
        public PropertyFileFilter Filter { get; set; }
        public PropertyFileBulkAction BulkAction { get; set; }
    }

    public enum PropertyFileOrder
    {
        Name,
        Type,
        Size
    }

    public enum PropertyFileFilter
    {
        All
    }

    public enum PropertyFileBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
