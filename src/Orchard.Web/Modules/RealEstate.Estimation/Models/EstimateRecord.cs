using System;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Estimation.Models
{

    public class EstimateRecord
    {
        public virtual int Id { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public virtual int StartIndex { get; set; }
        public virtual int EndIndex { get; set; }
        public virtual int TotalItems { get; set; }
        public virtual int SucessItems { get; set; }
        public virtual string Msg { get; set; }
        public virtual string ErrorMsg { get; set; }
    }

}
