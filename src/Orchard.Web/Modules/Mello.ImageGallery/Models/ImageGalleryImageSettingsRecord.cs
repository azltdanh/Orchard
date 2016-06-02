using System;
namespace Mello.ImageGallery.Models {
    public class ImageGalleryImageSettingsRecord {
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string Caption { get; set; }

        public virtual string Title { get; set; }

        public virtual int Position { get; set; }

        //added
        public virtual string href { get; set; }
        public virtual DateTime? DateBegin { get; set; }
        public virtual DateTime? DateEnd { get; set; }
        public virtual bool Enable { get; set; }
        public virtual bool Blank { get; set; }
        public virtual bool IsPublish { get; set; }
    }
}