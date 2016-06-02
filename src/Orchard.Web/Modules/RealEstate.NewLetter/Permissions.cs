using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.NewLetter
{
    public class Permissions : IPermissionProvider
    {
        // ManageNewsletter
        public static readonly Permission ManageNewsletter = new Permission { Description = "Manage Newsletter setting", Name = "ManageNewsletter" };

        //Message contact Inbox
        public static readonly Permission ContactInobxNewLetter = new Permission { Description = "Quản lý hộp thư liên hệ", Name = "ContactInobxNewLetter" };
        public static readonly Permission ContactInobxReplyNewLetter = new Permission { Description = "Trả lời tin nhắn, hộp thư", Name = "ContactInobxReplyNewLetter", ImpliedBy = new[] { ContactInobxNewLetter } };

        public virtual Feature Feature { get; set; }


        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {

                 //ManageNewsletter
                 ManageNewsletter,
                 ContactInobxNewLetter,
                 ContactInobxReplyNewLetter
                   
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
               
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {
                        //ManageNewsletter
                        ManageNewsletter,
                        ContactInobxNewLetter,
                        ContactInobxReplyNewLetter
                    }
                }
            };
        }
    }
}