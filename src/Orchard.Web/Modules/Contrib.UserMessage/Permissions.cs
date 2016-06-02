using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace Contrib.UserMessage {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageContactMessages = new Permission { Description = "Manage Contact Messages", Name = "ManageContactMessages" };
        public static readonly Permission SendContactMessages = new Permission { Description = "Send Contact Messages", Name = "SendContactMessages" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageContactMessages,
                SendContactMessages
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageContactMessages, SendContactMessages }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { ManageContactMessages, SendContactMessages }
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] { ManageContactMessages, SendContactMessages }
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] { ManageContactMessages, SendContactMessages }
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] { ManageContactMessages, SendContactMessages }
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] { ManageContactMessages, SendContactMessages }
                },
            };
        }
    }
}


