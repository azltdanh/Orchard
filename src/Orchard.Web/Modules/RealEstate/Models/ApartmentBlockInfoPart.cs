
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
namespace RealEstate.Models
{
    public class ApartmentBlockInfoPartRecord : ContentPartRecord
    {
        public virtual LocationApartmentBlockPartRecord ApartmentBlock { get; set; }
        public virtual string ApartmentName { get; set; }
        public virtual double ApartmentArea { get; set; }
        public virtual int ApartmentBedrooms { get; set; }
        public virtual int ApartmentBathrooms { get; set; }
        public virtual string OrtherContent { get; set; }
        //public virtual double PriceAverage { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual string Avatar { get; set; }
        public virtual double RealAreaUse { get; set; }//Dien tich su dung thuc te (dien tich thong thuy)
    }
    public class ApartmentBlockInfoPart : ContentPart<ApartmentBlockInfoPartRecord>
    {
        public LocationApartmentBlockPartRecord ApartmentBlock 
        {
            get { return Record.ApartmentBlock; }
            set { Record.ApartmentBlock = value; }
        }
        public string ApartmentName 
        {
            get { return Retrieve(r => r.ApartmentName); }
            set { Store(r => r.ApartmentName, value); }
        }
        public double ApartmentArea 
        {
            get { return Retrieve(r => r.ApartmentArea); }
            set { Store(r => r.ApartmentArea, value); }
        }
        public int ApartmentBedrooms 
        {
            get { return Retrieve(r => r.ApartmentBedrooms); }
            set { Store(r => r.ApartmentBedrooms, value); }
        }
        public int ApartmentBathrooms 
        {
            get { return Retrieve(r => r.ApartmentBathrooms); }
            set { Store(r => r.ApartmentBathrooms, value); }
        }
        public string OrtherContent 
        {
            get { return Retrieve(r => r.OrtherContent); }
            set { Store(r => r.OrtherContent, value); }
        }
        //public double PriceAverage
        //{
        //    get { return Retrieve(r => r.PriceAverage); }
        //    set { Store(r => r.PriceAverage, value); }
        //}
        public bool IsActive
        {
            get { return Retrieve(r => r.IsActive); }
            set { Store(r => r.IsActive, value); }
        }
        public string Avatar
        {
            get { return Retrieve(r => r.Avatar); }
            set { Store(r => r.Avatar, value); }
        }
        public double RealAreaUse
        {
            get { return Retrieve(r => r.RealAreaUse); }
            set { Store(r => r.RealAreaUse, value); }
        }
    }
}
