using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace RealEstate {
    public class Routes : IRouteProvider {
        
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {

                //control-panel/tat-ca
                new RouteDescriptor {
                    Priority = 11,
                    Route = new Route(
                        "control-panel",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Route = new Route(
                        "control-panel/profile",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "EditProfile"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Route = new Route(
                        "control-panel/list_requirement",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "ListCustomerRequirement"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Route = new Route(
                        "control-panel/change-password",
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"},
                            {"controller", "Account"},
                            {"action", "ChangePassword"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Orchard.Users"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "control-panel/soan-tin-moi",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxFrontEnd"},
                            {"action", "SendMessage"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"}
                        },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "control-panel/tin-da-nhan",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxFrontEnd"},
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
                        "control-panel/tin-da-gui",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxFrontEnd"},
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
                        "control-panel/xem-tin-nhan-{Id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.NewLetter"},
                            {"controller", "MessageInboxFrontEnd"},
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
                        "ajax/control-panel/AjaxViewIndex",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "ViewIndexAjax"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/control-panel/AjaxDeleteMessage",
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
                        "control-panel/noi-dung-tin-nhan/{id}",
                        new RouteValueDictionary {
                            {"area", "Contrib.UserMessage"},
                            {"controller", "UserMessage"},
                            {"action", "Details"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Contrib.UserMessage"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "control-panel/tin-da-luu",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "ListUserProperty"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "control-panel/lich-su-giao-dich",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "PaymentHistory"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "control-panel/nap-tien-bang-the-cao",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "CardPayment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/sms-execute",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "PhonePayment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/1payvn/sms-execute",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "OnePayPhonePayment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/ajaxExcuteCollection",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "AjaxExcuteCollection"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/ajaxPanelSearch",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "AjaxPanelSearch"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/ajaxDelete",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "AjaxDelete"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/ajaxTrashedDeleted",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "AjaxTrashedDeleted"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/ajaxRefresh",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "AjaxRefresh"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/ajaxStartPublished",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "AjaxStartPublished"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/ajaxStopPublished",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "AjaxStopPublished"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/ajaxRePending",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "AjaxRePending"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/loadAjaxPostVIP/{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "AjaxPostVIP"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/AjaxSubmitPostVIP",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "AjaxSubmitPostVIP"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/AjaxLoadAdsPrice",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "Contact"},
                            {"action", "AjaxLoadAdsPrice"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.miniforum/AjaxLoadBannerPrice",
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"},
                            {"controller", "AdsPriceConfig"},
                            {"action", "AjaxLoadBannerPrice"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.MiniForum"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/AjaxLoadContactOnline",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "Contact"},
                            {"action", "AjaxLoadContactOnline"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/AjaxLoadValuationCertificatePrice",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "Contact"},
                            {"action", "AjaxLoadValuationCertificatePrice"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                }                ,
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/realestate.usercontrolpanel/AjaxLoadUserMenu",
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"},
                            {"controller", "User"},
                            {"action", "AjaxLoadUserMenu"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.UserControlPanel"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}