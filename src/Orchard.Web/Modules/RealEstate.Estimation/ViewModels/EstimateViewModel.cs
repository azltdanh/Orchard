using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using System.Collections.Generic;
using System;
using RealEstate.Estimation.Models;

namespace RealEstate.Estimation.ViewModels
{
    public class EstimateViewModel  {
        //public IEnumerable<PropertyPart> Properties { get; set; }
        //public IList<string> Messages { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalItems { get; set; }
        public int StartIndex { get; set; }
        public string ReturnUrl { get; set; }
    }


    public class EstimateIndexViewModel
    {
        public IList<EstimateRecord> Estimates { get; set; }
        public EstimateIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public int PropertiesCount { get; set; }
    }

    public class EstimateIndexOptions
    {
        public string Search { get; set; }
    }

}