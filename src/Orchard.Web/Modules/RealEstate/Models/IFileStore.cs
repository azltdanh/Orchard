using System;
using System.Web;

namespace RealEstate.Models
{
    public interface IFileStore
    {
        Guid SaveUploadedFile(HttpPostedFileBase fileBase);
    }
}