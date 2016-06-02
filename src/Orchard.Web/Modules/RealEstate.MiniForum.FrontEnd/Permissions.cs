using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;


namespace RealEstate.MiniForum.FrontEnd
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission RelAddAttribute = new Permission { Description = "Rel Add Attribute", Name = "RelAddAttribute" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
               RelAddAttribute
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {
                        RelAddAttribute
                    }
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] {
                        RelAddAttribute
                    }
                },
            };
        }

        public static Permission ManagePropertySettings { get; set; }
    }
}