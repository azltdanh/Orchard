using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Laughlin.ErrorLog
{
    public class Permissions : IPermissionProvider {

        public static readonly Permission ViewErrorLog = new Permission { Description = "View Error Log", Name = "ViewErrorLog" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                
                ViewErrorLog

            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {

                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {
                        ViewErrorLog
                    }
                },
                new PermissionStereotype {
                    Name = "Developer",
                    Permissions = new[] {
                        ViewErrorLog
                    }
                },
            };
        }

    }
}


