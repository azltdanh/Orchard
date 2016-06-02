using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class PropertyTypeConstructionPartRecord : ContentPartRecord
    {
        public virtual PropertyTypeGroupPartRecord PropertyGroup { get; set; }
        public virtual PropertyTypePartRecord PropertyType { get; set; }
        public virtual int? MinFloor { get; set; }
        public virtual int? MaxFloor { get; set; }
        public virtual bool IsDefaultInFloorsRange { get; set; }
        public virtual string Name { get; set; }
        public virtual string ShortName { get; set; }
        public virtual string CssClass { get; set; }
        public virtual int SeqOrder { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual float UnitPrice { get; set; }
        public virtual string DefaultImgUrl { get; set; }
    }

    public class PropertyTypeConstructionPart : ContentPart<PropertyTypeConstructionPartRecord>
    {
        public PropertyTypeGroupPartRecord PropertyGroup
        {
            get { return Record.PropertyGroup; }
            set { Record.PropertyGroup = value; }
        }

        public PropertyTypePartRecord PropertyType
        {
            get { return Record.PropertyType; }
            set { Record.PropertyType = value; }
        }

        public int? MinFloor
        {
            get { return Retrieve(r => r.MinFloor); }
            set { Store(r => r.MinFloor, value); }
        }

        public int? MaxFloor
        {
            get { return Retrieve(r => r.MaxFloor); }
            set { Store(r => r.MaxFloor, value); }
        }

        public bool IsDefaultInFloorsRange
        {
            get { return Retrieve(r => r.IsDefaultInFloorsRange); }
            set { Store(r => r.IsDefaultInFloorsRange, value); }
        }

        public string Name
        {
            get { return Retrieve(r => r.Name); }
            set { Store(r => r.Name, value); }
        }

        public string ShortName
        {
            get { return Retrieve(r => r.ShortName); }
            set { Store(r => r.ShortName, value); }
        }

        public string CssClass
        {
            get { return Retrieve(r => r.CssClass); }
            set { Store(r => r.CssClass, value); }
        }

        public int SeqOrder
        {
            get { return Retrieve(r => r.SeqOrder); }
            set { Store(r => r.SeqOrder, value); }
        }

        public bool IsEnabled
        {
            get { return Retrieve(r => r.IsEnabled); }
            set { Store(r => r.IsEnabled, value); }
        }

        public float UnitPrice
        {
            get { return Retrieve(r => r.UnitPrice); }
            set { Store(r => r.UnitPrice, value); }
        }

        public string DefaultImgUrl
        {
            get { return Retrieve(r => r.DefaultImgUrl); }
            set { Store(r => r.DefaultImgUrl, value); }
        }
    }
}