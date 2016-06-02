using System;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace RealEstate.Models
{
    internal class DiskFileStore : IFileStore
    {
        private readonly string _uploadsFolder = HostingEnvironment.MapPath("/UserFiles");

        public Guid SaveUploadedFile(HttpPostedFileBase fileBase)
        {
            Guid identifier = Guid.NewGuid();
            fileBase.SaveAs(GetDiskLocation(identifier));
            return identifier;
        }

        private string GetDiskLocation(Guid identifier)
        {
            return Path.Combine(_uploadsFolder, identifier.ToString());
        }
    }
}