using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class DirectionViewModel
    {
    }

    public class DirectionCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public IContent Direction { get; set; }
    }

    public class DirectionEditViewModel
    {
        [Required]
        public string Name
        {
            get { return Direction.As<DirectionPart>().Name; }
            set { Direction.As<DirectionPart>().Name = value; }
        }

        [Required]
        public string ShortName
        {
            get { return Direction.As<DirectionPart>().ShortName; }
            set { Direction.As<DirectionPart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return Direction.As<DirectionPart>().CssClass; }
            set { Direction.As<DirectionPart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return Direction.As<DirectionPart>().SeqOrder; }
            set { Direction.As<DirectionPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return Direction.As<DirectionPart>().IsEnabled; }
            set { Direction.As<DirectionPart>().IsEnabled = value; }
        }

        public IContent Direction { get; set; }
    }

    public class DirectionsIndexViewModel
    {
        public IList<DirectionEntry> Directions { get; set; }
        public DirectionIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class DirectionEntry
    {
        public DirectionPart Direction { get; set; }
        public bool IsChecked { get; set; }
    }

    public class DirectionIndexOptions
    {
        public string Search { get; set; }
        public DirectionsOrder Order { get; set; }
        public DirectionsFilter Filter { get; set; }
        public DirectionsBulkAction BulkAction { get; set; }
    }

    public enum DirectionsOrder
    {
        SeqOrder,
        Name
    }

    public enum DirectionsFilter
    {
        All
    }

    public enum DirectionsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}