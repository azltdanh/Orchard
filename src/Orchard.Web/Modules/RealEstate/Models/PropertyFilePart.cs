using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Users.Models;

namespace RealEstate.Models
{
    public class PropertyFilePartRecord : ContentPartRecord
    {
        public virtual string Name { get; set; }
        public virtual string Type { get; set; }
        public virtual string Path { get; set; }
        public virtual int Size { get; set; }
        public virtual bool Published { get; set; }
        public virtual bool IsAvatar { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual DateTime CreatedDate { get; set; }
        public virtual UserPartRecord CreatedUser { get; set; }
        public virtual PropertyPartRecord PropertyPartRecord { get; set; }
        public virtual LocationApartmentPartRecord LocationApartmentPartRecord { get; set; }
        public virtual ApartmentBlockInfoPartRecord ApartmentBlockInfoPartRecord { get; set; }
    }

    public class PropertyFilePart : ContentPart<PropertyFilePartRecord>
    {
        public string Name
        {
            get { return Retrieve(r => r.Name); }
            set { Store(r => r.Name, value); }
        }

        public string Type
        {
            get { return Retrieve(r => r.Type); }
            set { Store(r => r.Type, value); }
        }

        public string Path
        {
            get { return Retrieve(r => r.Path); }
            set { Store(r => r.Path, value); }
        }

        public int Size
        {
            get { return Retrieve(r => r.Size); }
            set { Store(r => r.Size, value); }
        }

        public bool Published
        {
            get { return Retrieve(r => r.Published); }
            set { Store(r => r.Published, value); }
        }

        public bool IsAvatar
        {
            get { return Retrieve(r => r.IsAvatar); }
            set { Store(r => r.IsAvatar, value); }
        }

        public bool IsDeleted
        {
            get { return Retrieve(r => r.IsDeleted); }
            set { Store(r => r.IsDeleted, value); }
        }

        public DateTime CreatedDate
        {
            get { return Retrieve(r => r.CreatedDate); }
            set { Store(r => r.CreatedDate, value); }
        }

        public UserPartRecord CreatedUser
        {
            get { return Record.CreatedUser; }
            set { Record.CreatedUser = value; }
        }

        public PropertyPartRecord Property
        {
            get { return Record.PropertyPartRecord; }
            set { Record.PropertyPartRecord = value; }
        }

        public LocationApartmentPartRecord Apartment
        {
            get { return Record.LocationApartmentPartRecord; }
            set { Record.LocationApartmentPartRecord = value; }
        }

        public ApartmentBlockInfoPartRecord ApartmentBlockInfoPartRecord
        {
            get { return Record.ApartmentBlockInfoPartRecord; }
            set { Record.ApartmentBlockInfoPartRecord = value; }
        }
    }
}