using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace RealEstate.Forum
{
    public class Routes : IRouteProvider {
        
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                // AutoLogin
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/CustomerNewLetter",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "ListCustomer"},
                            {"action", "index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "xac-nhan-tu-choi-nhan-email",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "VerifyCustomerException"},
                            {"action", "VerifyExeptionEmail"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 3,
                    Route = new Route(
                        "lien-he",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "ContactInboxFrontEnd"},
                            {"action", "SendMessageContact"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 4,
                    Route = new Route(
                        "thu-gop-y",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "ContactInboxFrontEnd"},
                            {"action", "MessageContactRedirect"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/ContactInbox",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "ContactInboxAdmin"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/ViewContactInbox/{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "ContactInboxAdmin"},
                            {"action", "ViewInbox"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/ContactInboxDelete/{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "ContactInboxAdmin"},
                            {"action", "Delete"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestatenewletter/ajaxSendMessageContact",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "ContactInboxFrontEnd"},
                            {"action", "AjaxSendMessageContact"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestatenewletter/AjaxCheckIsRead",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "ContactInboxAdmin"},
                            {"action", "AjaxCheckIsRead"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestatenewletter/AjaxCountMessageNotifi",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxFrontEnd"},
                            {"action", "AjaxCountMessageNotifi"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/MessageInbox",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxAdmin"},
                            {"action", "Inbox"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/MessageSendInbox",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxAdmin"},
                            {"action", "SendInbox"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/NewMessageInbox",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxAdmin"},
                            {"action", "CreateMessage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/ViewMessageInbox/{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxAdmin"},
                            {"action", "ViewMessage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/AjaxListUserForCombobox",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxAdmin"},
                            {"action", "ListUserForCombobox"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "admin/ajax/CountMessageInbox",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxAdmin"},
                            {"action", "CountMessageInbox"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/loadmessage/AjaxLoadMessageInboxBlog",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxFrontEnd"},
                            {"action", "AjaxLoadMessageInboxBlog"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/loadmessage/AjaxLoadMessageSendBlog",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxFrontEnd"},
                            {"action", "AjaxLoadMessageSendBlog"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/control-panel/AjaxDeleteMessageUser",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxFrontEnd"},
                            {"action", "AjaxDeleteMessage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestatenewletter/AjaxSendMessageUser",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxFrontEnd"},
                            {"action", "AjaxSendMessageUser"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "blog-ca-nhan/tin-nhan/gui-tin-nhan",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxFrontEnd"},
                            {"action", "SendMessageUser"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.AjaxDeleteMessageUser"}
                        },
                        new MvcRouteHandler())
                }

            };
        }
    }
}