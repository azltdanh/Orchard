using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace RealEstateForum.Service
{
    public class MiniForumPermissions : IPermissionProvider
    {
        public static readonly Permission ManagementMiniForum = new Permission { Description = "Management Mini Forum", Name = "ManagementMiniForum" };
        public static readonly Permission ManagementMiniUpdatePostForum = new Permission { Description = "Management Mini Update Post Forum", Name = "ManagementMiniUpdatePostForum", ImpliedBy = new[] { ManagementMiniForum } };
        public static readonly Permission ManagementCommentPostForum = new Permission { Description = "Management Comment Post Forum", Name = "ManagementCommentPostForum", ImpliedBy = new[] { ManagementMiniForum } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
               //Management Forum 
                ManagementMiniForum,
                ManagementMiniUpdatePostForum,
                ManagementCommentPostForum
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
               
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {
                        ManagementMiniForum,
                        ManagementMiniUpdatePostForum,
                        ManagementCommentPostForum
                    }
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] {
                        ManagementMiniUpdatePostForum
                    }
                },
            };
        }
    }
}