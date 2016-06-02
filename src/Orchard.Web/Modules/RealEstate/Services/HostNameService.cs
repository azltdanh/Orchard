using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IHostNameService : IDependency
    {
        string GetHostNameSite();
        HostNamePart GetHostNamePart(string hostname);
        HostNamePart GetHostNamePartByClass(string cssClass);
        HostNamePart GetHostNameByProperty(PropertyPart p);
        IEnumerable<HostNamePart> GetHostNamePartCssClass(string cssClass);
        HostNamePart GetHostNameById(int userGroup);
    }

    public class HostNameService : IHostNameService
    {
        private readonly IContentManager _contentManager;

        public HostNameService(IOrchardServices services, IContentManager contentManager)
        {
            Services = services;
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }

        public string GetHostNameSite()
        {
            if (Services.WorkContext.HttpContext.Request.Url != null)
                return Services.WorkContext.HttpContext.Request.Url.Host; //should be "www.dulieunhadat.vn"
            return "dinhgianhadat.vn";
        }

        public HostNamePart GetHostNameById(int userGroup)
        {
            var _record = _contentManager.Query<HostNamePart, HostNamePartRecord>()
                    .Where(a => a.Id == userGroup);

            if (_record.List().Any())
                return
                _record
                    .Slice(1)
                    .FirstOrDefault();
            else
                return null;
        }

        public HostNamePart GetHostNamePart(string hostname)
        {
            if (string.IsNullOrEmpty(hostname)) return null;
            if (_contentManager.Query<HostNamePart, HostNamePartRecord>()
                    .Where(a => a.Name == hostname).List().Any())
                return
                _contentManager.Query<HostNamePart, HostNamePartRecord>()
                    .Where(a => a.Name == hostname)
                    .Slice(1)
                    .FirstOrDefault();
            else
                return null;
        }

        public HostNamePart GetHostNamePartByClass(string cssClass)
        {
            if (string.IsNullOrEmpty(cssClass)) return null;
            HostNamePart hostPart =
                _contentManager.Query<HostNamePart, HostNamePartRecord>()
                    .Where(a => a.CssClass == cssClass)
                    .Slice(1)
                    .FirstOrDefault();
            return hostPart;
        }

        public HostNamePart GetHostNameByProperty(PropertyPart p)
        {
            if (p == null) return null;
            HostNamePart hostPart =
                _contentManager.Query<HostNamePart, HostNamePartRecord>()
                    .Where(a => a.Id == p.UserGroup.Id)
                    .Slice(1)
                    .FirstOrDefault();
            return hostPart;
        }

        public IEnumerable<HostNamePart> GetHostNamePartCssClass(string cssClass)
        {
            if (string.IsNullOrEmpty(cssClass)) return null;
            IEnumerable<HostNamePart> hostPart =
                _contentManager.Query<HostNamePart, HostNamePartRecord>().Where(a => a.CssClass == cssClass).List();
            return hostPart;
        }
    }
}