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

#region FrontEnd

#region Estimate

                // dinh-gia-bat-dong-san
                new RouteDescriptor {
                    Route = new Route(
                        "dinh-gia-bat-dong-san",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "Estimate"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                
                // dinh-gia-bat-dong-san/{id}
                new RouteDescriptor {
                    Route = new Route(
                        "dinh-gia-bat-dong-san/{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "Estimate"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                
                // dinh-gia
                new RouteDescriptor {
                    Route = new Route(
                        "dinh-gia",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "Estimate"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                
                // dinh-gia/{id}
                new RouteDescriptor {
                    Route = new Route(
                        "dinh-gia/{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "Estimate"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                
#endregion

#region HomeController
                
                // quan-ly-tin-rao
                new RouteDescriptor {
                    Priority = 1,
                    Route = new Route(
                        "quan-ly-tin-rao",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "Home"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },

                // dang-tin
                new RouteDescriptor {
                    Route = new Route(
                        "dang-tin",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "Home"},
                            {"action", "Create"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Route = new Route(
                        "dang-tin/{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "Home"},
                            {"action", "Edit"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },

                // dang-tin/tin-can-mua-thue
                new RouteDescriptor {
                    Route = new Route(
                        "dang-tin-can-mua-thue",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "Home"},
                            {"action", "CreateCustomerRequirements"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },

                new RouteDescriptor {
                    Route = new Route(
                        "dang-tin-can-mua-thue/{groupid}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "Home"},
                            {"action", "EditCustomerRequirements"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
#endregion

#region RealEstateDetail

                // tin-{title}-{id}
                new RouteDescriptor {
                    Route = new Route(
                        "tin-{title}-{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "RealEstateDetail"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },

                // {title}-{id}
                new RouteDescriptor {
                    Priority = 0,
                    Route = new Route(
                        "{title}-{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "RealEstateDetail"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                
                // bat-dong-san/chi-tiet
                new RouteDescriptor {
                    Route = new Route(
                        "bat-dong-san/chi-tiet/{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "RealEstateDetail"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                
                // bat-dong-san-chi-tiet
                new RouteDescriptor {
                    Route = new Route(
                        "bat-dong-san-chi-tiet/{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "RealEstateDetail"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 3,
                    Route = new Route(
                        "khach-hang-{url}-{id}",//khach-hang-can-mua-dat-o-va-nha-12345
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "NewRequirmentDetail"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 4,
                    Route = new Route(
                        "bat-dong-san/chi-tiet-yeu-cau/{id}",//RedicrectToAction NewRequirmentDetail
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "RequirmentDetail"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                
#endregion

#region nofollow

                // AjaxRating
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/Estimate/AjaxRating",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "Estimate"},
                            {"action", "AjaxRating"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                // /RealEstate.FrontEnd/PropertySearch/AjaxGetPropertyHightLight
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/AjaxGetPropertyHightLight",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "AjaxGetPropertyHightLight"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                // /RealEstate.FrontEnd/PropertySearch/AjaxProperties
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/AjaxProperties",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "AjaxProperties"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                // /RealEstate.FrontEnd/PropertySearch/AjaxBreadcrumb
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/AjaxBreadcrumb",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "AjaxBreadcrumb"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                // /RealEstate.FrontEnd/PropertySearch/AjaxBreadcrumbAdsType
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/AjaxBreadcrumbAdsType",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "AjaxBreadcrumbAdsType"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                // /RealEstate.FrontEnd/PropertySearch/ViewFrontProperty
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/ViewFrontProperty",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "ViewFrontProperty"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                // /RealEstate.FrontEnd/PropertySearch/ViewSameProperty
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/ViewSameProperty/{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "ViewSameProperty"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                // /RealEstate.FrontEnd/PropertySearch/LoadSameProperty
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/LoadSameProperty",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "LoadSameProperty"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                // /RealEstate.FrontEnd/PropertySearch/AddComment
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/ReloadComments",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "ReloadComments"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                // /RealEstate.FrontEnd/PropertySearch/AddComment
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/AddComment",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "AddComment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                // /RealEstate.FrontEnd/PropertySearch/DeleteComment
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/DeleteComment",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "DeleteComment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                // /RealEstate.FrontEnd/Comments/AjaxEditComment
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/Comments/AjaxEditComment",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "AjaxEditComment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                // Post face
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/Comments/AjaxCommentToFaceBook",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "AjaxCommentToFaceBook"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                // /RealEstate.FrontEnd/PropertySearch/LoadProvinceById
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/LoadProvinceById",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "LoadProvinceById"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },//
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/AjaxResultFilterRequirement",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "AjaxResultFilterRequirement"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/AjaxLoadTheSameCustomer",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "AjaxLoadTheSameCustomer"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/AjaxResultFilter",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "AjaxResultFilter"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/SaveProperty",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "SaveProperty"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertySearch/DeleteUserProperty",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "DeleteUserProperty"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "ajax/RealEstate.FrontEnd/PropertyExchange/AjaxLoadRequirementDetail",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertyExchange"},
                            {"action", "AjaxLoadRequirementDetail"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstateFrontEnd/Aliases/AliasSearch",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "AliasesMeta"},
                            {"action", "AliasSearch"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/RealEstateFrontEnd/Aliases/DeleteAlias",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "AliasesMeta"},
                            {"action", "DeleteAlias"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                
#endregion

                new RouteDescriptor {
                    Route = new Route(
                        "du-an/{title}-{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "LocationApartmentDetail"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                 new RouteDescriptor {
                    Route = new Route(
                        "du-an/gio-hang-du-an-{title}/{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "LocationApartmentCart"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 4,
                    Route = new Route(
                        "tat-ca-du-an",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "ResultFilterApartment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 4,
                    Route = new Route(
                        "so-sanh-du-an",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "CompareApartment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 3,
                    Route = new Route(
                        "so-sanh-du-an-vs/{apName}-{apId}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "WithCompareApartment"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 4,
                    Route = new Route(
                        "so-sanh-du-an/{apName}-{apId}-vs-{wName}-{wId}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertySearch"},
                            {"action", "CompareDetail"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 4,
                    Route = new Route(
                        "dang-tin/bat-dong-san-muon-nhan",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertyExchange"},
                            {"action", "RequirementExchangeCreate"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Priority = 4,
                    Route = new Route(
                        "dang-tin/cap-nhat-bat-dong-san-muon-nhan/{id}",
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"},
                            {"controller", "PropertyExchange"},
                            {"action", "RequirementExchangeEdit"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "RealEstate.FrontEnd"}
                        },
                        new MvcRouteHandler())
                },
#endregion

            };
        }
    }
}